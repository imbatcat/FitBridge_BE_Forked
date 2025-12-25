using FitBridge_Application.Commons.Utils;
using FitBridge_Application.Dtos.Coupons;
using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Services.Notifications;
using FitBridge_Application.Specifications.Orders.GetOrderItemById;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Entities.Reports;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Enums.Reports;
using FitBridge_Domain.Exceptions;
using MediatR;
using System.Text.Json;

namespace FitBridge_Application.Features.Reports.ConfirmReport
{
    internal class ConfirmReportCommandHandler(
        IUnitOfWork unitOfWork,
        IScheduleJobServices scheduleJobServices,
        ITransactionService transactionService,
        ICourseCompletionService courseCompletionService,
        INotificationService notificationService) : IRequestHandler<ConfirmReportCommand, ConfirmReportResponseDto>
    {
        public async Task<ConfirmReportResponseDto> Handle(ConfirmReportCommand request, CancellationToken cancellationToken)
        {
            var existingReport = await unitOfWork.Repository<ReportCases>().GetByIdAsync(request.ReportId, asNoTracking: false)
                ?? throw new NotFoundException(nameof(ReportCases));

            if (existingReport.Status != ReportCaseStatus.Processing)
            {
                throw new DataValidationFailedException("Đơn kiện phải ở trạng thái Đang xử lý để xác nhận lừa đảo");
            }

            var orderItemSpec = new GetOrderItemByIdSpec(
                existingReport.OrderItemId,
                isIncludeTransaction: true,
                isIncludeGymCourse: true,
                isIncludeFreelancePackage: true,
                isIncludeProduct: true);

            var orderItem = await unitOfWork.Repository<OrderItem>()
                .GetBySpecificationAsync(orderItemSpec, asNoTracking: false)
                ?? throw new NotFoundException(nameof(OrderItem));

            if (orderItem.IsRefunded)
            {
                throw new DataValidationFailedException("Mục đơn hàng đã được hoàn tiền");
            }

            existingReport.Status = ReportCaseStatus.FraudConfirmed;
            existingReport.ResolvedAt = DateTime.UtcNow;
            existingReport.Note = request.Note;
            existingReport.IsPayoutPaused = true;

            orderItem.IsRefunded = true;

            var responseDto = new ConfirmReportResponseDto();

            var isProduct = orderItem.ProductDetailId.HasValue;

            await SendNotificationToReporter(existingReport);
            if (!isProduct)
            {
                var jobName = $"ProfitDistribution_{existingReport.OrderItemId}";

                var jobGroup = "ProfitDistribution";
                await scheduleJobServices.CancelScheduleJob(jobName, jobGroup);

                var refundAmount = await transactionService.CalculateMerchantProfit(orderItem, orderItem.Order.Coupon);
                await CreateDeductPendingBalanceTransactionAsync(orderItem, refundAmount);
                await SendNotificationToReported(existingReport, orderItem);

                var courseCompletion = await courseCompletionService.GetCourseCompletionAsync(existingReport.OrderItemId);
                MapResponseDto(responseDto, orderItem, refundAmount, courseCompletion.CompletionPercentage);
            }
            else
            {
                var discountAmount = 0;
                if (orderItem.Order.Coupon != null)
                {
                    var coupon = orderItem.Order.Coupon;
                    discountAmount = (int)Math.Min(orderItem.Price * (decimal)coupon.DiscountPercent / 100, coupon.MaxDiscount);
                }
                var refundAmount = orderItem.Price - discountAmount;
                await CreateProductRefundTransactionAsync(orderItem, refundAmount);

                MapResponseDto(responseDto, orderItem, refundAmount);
            }

            unitOfWork.Repository<OrderItem>().Update(orderItem);
            unitOfWork.Repository<ReportCases>().Update(existingReport);

            await unitOfWork.CommitAsync();

            return responseDto;
        }

        private void MapResponseDto(ConfirmReportResponseDto responseDto, OrderItem orderItem, decimal refundAmount, decimal? completionPercentage = null)
        {
            responseDto.CompletionPercentage = completionPercentage;

            responseDto.OrderItemPrice = orderItem.Price;
            responseDto.CouponDto = orderItem.Order.Coupon != null
                ? new ApplyCouponDto
                {
                    CouponCode = orderItem.Order.Coupon.CouponCode,
                    DiscountPercent = orderItem.Order.Coupon.DiscountPercent,
                    DiscountAmount = orderItem.Order.Coupon.MaxDiscount
                }
                : null;
            responseDto.RefundAmount = refundAmount;
        }

        private async Task CreateProductRefundTransactionAsync(OrderItem orderItem, decimal refundAmount)
        {
            var deductTransaction = new Transaction
            {
                Amount = -refundAmount,
                OrderId = orderItem.OrderId,
                OrderItemId = orderItem.Id,
                OrderCode = GenerateOrderCode(),
                TransactionType = TransactionType.ProductRefund,
                Status = TransactionStatus.Success,
                Description = $"Hoàn tiền sản phẩm cho khách hàng - Mục đơn hàng: {orderItem.Id}",
                PaymentMethodId = await GetSystemPaymentMethodId.GetPaymentMethodId(MethodType.System, unitOfWork)
            };
            unitOfWork.Repository<Transaction>().Insert(deductTransaction);
        }

        private async Task CreateDeductPendingBalanceTransactionAsync(OrderItem orderItem, decimal amount)
        {
            var sellerWalletId = orderItem.GymCourseId != null
                ? orderItem.GymCourse!.GymOwnerId
                : orderItem.FreelancePTPackage!.PtId;

            var deductTransaction = new Transaction
            {
                Amount = -amount,
                WalletId = sellerWalletId,
                OrderId = orderItem.OrderId,
                OrderItemId = orderItem.Id,
                OrderCode = GenerateOrderCode(),
                TransactionType = TransactionType.PendingDeduction,
                Status = TransactionStatus.Success,
                Description = $"Trừ số dư chờ thanh toán do xác nhận lừa đảo - Mục đơn hàng: {orderItem.Id}",
                PaymentMethodId = await GetSystemPaymentMethodId.GetPaymentMethodId(MethodType.System, unitOfWork)
            };

            var sellerWallet = await unitOfWork.Repository<Wallet>()
                .GetByIdAsync(sellerWalletId, asNoTracking: false)
                ?? throw new NotFoundException(nameof(Wallet));

            sellerWallet.PendingBalance -= amount; // if negative, the seller owes the system

            unitOfWork.Repository<Transaction>().Insert(deductTransaction);
            unitOfWork.Repository<Wallet>().Update(sellerWallet);
        }

        private async Task SendNotificationToReporter(ReportCases report)
        {
            var model = new ReportStatusUpdatedModel
            {
                TitleReportTitle = report.Title,
                BodyReportTitle = report.Title,
                BodyStatus = "Xác nhận lừa đảo",
                BodyNote = report.Note ?? "Đơn kiện của bạn đã được xử lí. Hệ thống sẽ hoàn tiền lại trong giây lát."
            };

            var notificationMessage = new NotificationMessage(
                EnumContentType.ReportStatusUpdated,
                [report.ReporterId],
                model,
                JsonSerializer.Serialize(new { report.Id }));

            await notificationService.NotifyUsers(notificationMessage);
        }

        private async Task SendNotificationToReported(ReportCases report, OrderItem orderItem)
        {
            var model = new ReportStatusUpdatedModel
            {
                TitleReportTitle = report.Title,
                BodyReportTitle = report.Title,
                BodyStatus = "Xác nhận lừa đảo",
                BodyNote = report.Note ?? "Bạn đã bị report"
            };

            var isGymReported = orderItem.GymCourseId.HasValue;
            var sendee = isGymReported
                ? orderItem.GymCourse!.GymOwnerId
                : orderItem.FreelancePTPackage!.PtId;

            var notificationMessage = new NotificationMessage(
                EnumContentType.ReportStatusUpdated,
                [sendee],
                model,
                JsonSerializer.Serialize(new { report.Id }));

            await notificationService.NotifyUsers(notificationMessage);
        }

        private long GenerateOrderCode()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}