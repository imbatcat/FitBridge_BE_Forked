using System;
using MediatR;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Certificates;
using FitBridge_Domain.Exceptions;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Certificates.DeleteCertificate;

public class DeleteCertificateCommandHandler(
    IUnitOfWork _unitOfWork, 
    IScheduleJobServices _scheduleJobServices, 
    IUploadService _uploadService,
    IGraphService _graphService,
    ILogger<DeleteCertificateCommandHandler> _logger) : IRequestHandler<DeleteCertificateCommand, bool>
{
    public async Task<bool> Handle(DeleteCertificateCommand request, CancellationToken cancellationToken)
    {
        var certificate = await _unitOfWork.Repository<PtCertificates>().GetByIdAsync(request.CertificateId);
        if (certificate == null)
        {
            throw new NotFoundException(nameof(PtCertificates));
        }
        if(certificate.ExpirationDate == null)
        {
            await _scheduleJobServices.CancelScheduleJob($"AutoExpiredCertificate_{request.CertificateId}", "AutoExpiredCertificate");
        }
        if(certificate.CertUrl != null)
        {
            await _uploadService.DeleteFileAsync(certificate.CertUrl);
        }
        _unitOfWork.Repository<PtCertificates>().Delete(certificate);
        await _unitOfWork.CommitAsync();
        
        // Delete from graph after commit (non-blocking)
        await DeleteCertificateFromGraph(certificate);
        
        return true;
    }

    private async Task DeleteCertificateFromGraph(PtCertificates certificate)
    {
        try
        {
            try
            {
                var relationship = new HasCertificateRelationship
                {
                    FreelancePTId = certificate.PtId.ToString(),
                    CertificateId = certificate.Id.ToString()
                };
                
                await _graphService.DeleteRelationship(relationship);
                _logger.LogInformation("Deleted HAS_CERTIFICATE relationship for certificate {CertificateId}", certificate.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete HAS_CERTIFICATE relationship for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
            }

            try
            {
                var certificateNode = new CertificateNode
                {
                    DbId = certificate.Id.ToString()
                };
                
                await _graphService.DeleteNode(certificateNode);
                _logger.LogInformation("Deleted certificate node for certificate {CertificateId}", certificate.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete certificate node for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete certificate from graph for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
        }
    }
}
