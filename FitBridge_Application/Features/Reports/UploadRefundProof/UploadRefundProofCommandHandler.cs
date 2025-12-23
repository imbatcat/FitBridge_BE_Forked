using FitBridge_Application.Dtos.Notifications;
using FitBridge_Application.Dtos.Templates;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services.Notifications;
using FitBridge_Domain.Entities.Reports;
using FitBridge_Domain.Enums.MessageAndReview;
using FitBridge_Domain.Enums.Reports;
using FitBridge_Domain.Exceptions;
using MediatR;
using System.Text.Json;

namespace FitBridge_Application.Features.Reports.UploadRefundProof
{
    internal class UploadRefundProofCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService) : IRequestHandler<UploadRefundProofCommand>
    {
        public async Task Handle(UploadRefundProofCommand request, CancellationToken cancellationToken)
        {
            var existingReport = await unitOfWork.Repository<ReportCases>().GetByIdAsync(request.ReportId, asNoTracking: false)
                ?? throw new NotFoundException(nameof(ReportCases));

            if (existingReport.Status != ReportCaseStatus.FraudConfirmed)
            {
                throw new DataValidationFailedException("Đơn kiện phải ở trạng thái Xác nhận lừa đảo để tải lên bằng chứng hoàn tiền");
            }

            if (request.ResolvedEvidenceImageUrl == null || request.ResolvedEvidenceImageUrl.Length == 0)
            {
                throw new DataValidationFailedException("Cần cung cấp ít nhất một ảnh bằng chứng hoàn tiền");
            }

            // Update report with refund proof images
            existingReport.ResolvedEvidenceImageUrl = request.ResolvedEvidenceImageUrl;
            existingReport.Status = ReportCaseStatus.Resolved;
            existingReport.ResolvedAt = DateTime.UtcNow;
            existingReport.Note = request.Note ?? existingReport.Note;
            existingReport.IsPayoutPaused = false;

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
                BodyStatus = "Đã hoàn tiền",
                BodyNote = report.Note ?? "Đơn kiện của bạn đã được giải quyết và hoàn tiền."
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