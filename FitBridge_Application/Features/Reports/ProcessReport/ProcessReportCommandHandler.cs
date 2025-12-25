using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Services.Notifications;
using FitBridge_Application.Specifications.Orders.GetOrderItemById;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Entities.Reports;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Domain.Enums.Reports;
using FitBridge_Domain.Exceptions;
using MediatR;
using System.Text.Json;

namespace FitBridge_Application.Features.Reports.ProcessReport
{
    internal class ProcessReportCommandHandler(
        IUnitOfWork unitOfWork,
        IScheduleJobServices scheduleJobServices,
        INotificationService notificationService) : IRequestHandler<ProcessReportCommand>
    {
        public async Task Handle(ProcessReportCommand request, CancellationToken cancellationToken)
        {
            var existingReport = await unitOfWork.Repository<ReportCases>().GetByIdAsync(request.ReportId, asNoTracking: false)
                ?? throw new NotFoundException(nameof(ReportCases));
            if (existingReport.Status != ReportCaseStatus.Pending)
            {
                throw new DataValidationFailedException("Đơn kiện phải ở trạng thái Chờ xử lý để bắt đầu điều tra");
            }

            var orderItemSpec = new GetOrderItemByIdSpec(existingReport.OrderItemId);
            var orderItem = await unitOfWork.Repository<OrderItem>()
                .GetBySpecificationAsync(orderItemSpec, asNoTracking: true)
                ?? throw new NotFoundException(nameof(OrderItem));

            existingReport.Status = ReportCaseStatus.Processing;
            existingReport.IsPayoutPaused = true;

            var isProduct = orderItem.ProductDetailId.HasValue;
            if (!isProduct)
            {
                var jobName = $"ProfitDistribution_{existingReport.OrderItemId}";
                var jobGroup = "ProfitDistribution";
                await scheduleJobServices.CancelScheduleJob(jobName, jobGroup);
            }

            unitOfWork.Repository<ReportCases>().Update(existingReport);
            await unitOfWork.CommitAsync();

            await SendNotificationToReporter(existingReport);
        }

        private async Task SendNotificationToReporter(ReportCases report)
        {
            var model = new ReportStatusUpdatedModel
            {
                TitleReportTitle = report.Title,
                BodyReportTitle = report.Title,
                BodyStatus = "Đang điều tra",
                BodyNote = "Đơn kiện của bạn đang được đội ngũ của chúng tôi điều tra."
            };

            var notificationMessage = new NotificationMessage(
                EnumContentType.ReportStatusUpdated,
                [report.ReporterId],
                model,
                JsonSerializer.Serialize(new { report.Id }));

            await notificationService.NotifyUsers(notificationMessage);
        }
    }
}