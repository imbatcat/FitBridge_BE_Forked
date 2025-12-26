using System;
using FitBridge_Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using FitBridge_Domain.Entities.Certificates;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Enums.Certificates;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Domain.Graph.Entities.Relationships;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Certificates.UpdateCertificateStatus;

public class UpdateCertificateStatusCommandHandler(
    IUnitOfWork _unitOfWork, 
    IMapper _mapper, 
    IScheduleJobServices _scheduleJobServices,
    IGraphService _graphService,
    ILogger<UpdateCertificateStatusCommandHandler> _logger) : IRequestHandler<UpdateCertificateStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateCertificateStatusCommand request, CancellationToken cancellationToken)
    {
        var certificate = await _unitOfWork.Repository<PtCertificates>().GetByIdAsync(request.CertificateId);
        if (certificate == null)
        {
            throw new NotFoundException(nameof(PtCertificates));
        }
        certificate.CertificateStatus = request.CertificateStatus;
        certificate.Note = request.Note;
        if (request.CertificateStatus == CertificateStatus.Active)
        {
            if (certificate.ExpirationDate != null)
            {
                await _scheduleJobServices.ScheduleAutoExpiredCertificateJob(request.CertificateId, certificate.ExpirationDate.Value.ToDateTime(TimeOnly.MaxValue));
            }
        }
        _unitOfWork.Repository<PtCertificates>().Update(certificate);
        await _unitOfWork.CommitAsync();
        
        // Update graph relationship after commit (non-blocking)
        await UpdateCertificateRelationshipInGraph(certificate);
        
        return true;
    }

    private async Task UpdateCertificateRelationshipInGraph(PtCertificates certificate)
    {
        try
        {
            var relationship = new HasCertificateRelationship
            {
                FreelancePTId = certificate.PtId.ToString(),
                CertificateId = certificate.Id.ToString(),
                CertificateStatus = certificate.CertificateStatus.ToString(),
                CertUrl = certificate.CertUrl,
                ProvidedDate = certificate.ProvidedDate,
                ExpirationDate = certificate.ExpirationDate ?? DateOnly.MaxValue
            };
            
            await _graphService.UpdateRelationship(relationship);
            _logger.LogInformation("Updated HAS_CERTIFICATE relationship for certificate {CertificateId}", certificate.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update certificate relationship in graph for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
        }
    }
}
