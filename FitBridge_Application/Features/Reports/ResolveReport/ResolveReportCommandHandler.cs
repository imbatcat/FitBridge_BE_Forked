using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Services.Notifications;
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
        INotificationService notificationService) : IRequestHandler<ResolveReportCommand>
    {
        public async Task Handle(ResolveReportCommand request, CancellationToken cancellationToken)
        {
            var existingReport = await unitOfWork.Repository<ReportCases>().GetByIdAsync(request.ReportId, asNoTracking: false)
                ?? throw new NotFoundException(nameof(ReportCases));

            if (existingReport.Status != ReportCaseStatus.Processing)
            {
                throw new DataValidationFailedException("??n ki?n ph?i ? tr?ng thái ?ang x? lý ?? gi?i quy?t");
            }

            existingReport.Status = ReportCaseStatus.Resolved;
            existingReport.ResolvedAt = DateTime.UtcNow;
            existingReport.Note = request.Note;
            existingReport.IsPayoutPaused = false; // Resume payout

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
                BodyStatus = "?ã gi?i quy?t",
                BodyNote = report.Note ?? "??n ki?n c?a b?n ?ã ???c gi?i quy?t. Không phát hi?n l?a ??o."
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
