using FitBridge_Application.Dtos.Jobs;
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

namespace FitBridge_Application.Features.Reports.ResolveReport
{
    internal class ResolveReportCommandHandler(
        IUnitOfWork unitOfWork,
        IScheduleJobServices scheduleJobServices,
        INotificationService notificationService) : IRequestHandler<ResolveReportCommand>
    {
        public async Task Handle(ResolveReportCommand request, CancellationToken cancellationToken)
        {
            var existingReport = await unitOfWork.Repository<ReportCases>().GetByIdAsync(request.ReportId, asNoTracking: false)
                ?? throw new NotFoundException(nameof(ReportCases));

            if (existingReport.Status != ReportCaseStatus.Processing)
            {
                throw new DataValidationFailedException("Đơn kiện phải ở trạng thái Đang xử lý để giải quyết");
            }

            // Load the OrderItem to get the profit distribution date
            var orderItemSpec = new GetOrderItemByIdSpec(existingReport.OrderItemId);
            var orderItem = await unitOfWork.Repository<OrderItem>()
                .GetBySpecificationAsync(orderItemSpec, asNoTracking: true)
                ?? throw new NotFoundException(nameof(OrderItem));

            existingReport.Status = ReportCaseStatus.Resolved;
            existingReport.ResolvedAt = DateTime.UtcNow;
            existingReport.Note = request.Note;
            existingReport.IsPayoutPaused = false;

            unitOfWork.Repository<ReportCases>().Update(existingReport);
            await unitOfWork.CommitAsync();

            await RecreateDistributeProfitJobAsync(orderItem);

            await SendNotificationToReporter(existingReport);
        }

        private async Task RecreateDistributeProfitJobAsync(OrderItem orderItem)
        {
            if (orderItem.ProfitDistributePlannedDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var profitDistributionDate = orderItem.ProfitDistributePlannedDate.Value;

                if (profitDistributionDate <= today)
                {
                    profitDistributionDate = today.AddDays(1);
                }

                var profitJobScheduleDto = new ProfitJobScheduleDto
                {
                    OrderItemId = orderItem.Id,
                    ProfitDistributionDate = profitDistributionDate
                };
                await scheduleJobServices.ScheduleProfitDistributionJob(profitJobScheduleDto);
            }
        }

        private async Task SendNotificationToReporter(ReportCases report)
        {
            var model = new ReportStatusUpdatedModel
            {
                TitleReportTitle = report.Title,
                BodyReportTitle = report.Title,
                BodyStatus = "Đã giải quyết",
                BodyNote = report.Note ?? "Đơn kiện của bạn đã được giải quyết. Không phát hiện lừa đảo."
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