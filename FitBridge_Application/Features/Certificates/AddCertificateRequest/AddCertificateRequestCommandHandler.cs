using System;
using MediatR;
using FitBridge_Application.Interfaces.Repositories;
using AutoMapper;
using FitBridge_Domain.Entities.Certificates;
using FitBridge_Application.Interfaces.Utils;
using Microsoft.AspNetCore.Http;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Enums.Certificates;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Commons.Constants;
using FitBridge_Domain.Graph.Entities;
using FitBridge_Domain.Graph.Entities.Relationships;
using Microsoft.Extensions.Logging;

namespace FitBridge_Application.Features.Certificates.AddCertificateRequest;

public class AddCertificateRequestCommandHandler(
    IUnitOfWork _unitOfWork, 
    IMapper _mapper, 
    IUserUtil _userUtil, 
    IHttpContextAccessor _httpContextAccessor, 
    IUploadService _uploadService, 
    IApplicationUserService _applicationUserService,
    IGraphService _graphService,
    ILogger<AddCertificateRequestCommandHandler> _logger) : IRequestHandler<AddCertificateRequestCommand, bool>
{
    public async Task<bool> Handle(AddCertificateRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await _applicationUserService.GetByIdAsync(request.PtId) ?? throw new NotFoundException("User not found");
        var role = await _applicationUserService.GetUserRoleAsync(user);
        if(role != ProjectConstant.UserRoles.FreelancePT)
        {
            throw new BusinessException("User is not a Freelance PT");
        }
        
        var certificateMetadata = await _unitOfWork.Repository<CertificateMetadata>().GetByIdAsync(request.CertificateMetadataId);
        if (certificateMetadata == null)
        {
            throw new NotFoundException("Certificate metadata not found");
        }
        
        var mappedEntity = _mapper.Map<AddCertificateRequestCommand, PtCertificates>(request);
        mappedEntity.CertificateStatus = CertificateStatus.WaitingForReview;
        mappedEntity.CertUrl = await _uploadService.UploadFileAsync(request.CertUrl);
        _unitOfWork.Repository<PtCertificates>().Insert(mappedEntity);
        await _unitOfWork.CommitAsync();
        
        // Create graph node and relationship after commit (non-blocking)
        await CreateCertificateInGraph(mappedEntity, certificateMetadata);
        
        return true;
    }

    private async Task CreateCertificateInGraph(PtCertificates certificate, CertificateMetadata metadata)
    {
        try
        {
            try
            {
                var certificateNode = new CertificateNode
                {
                    DbId = certificate.Id.ToString(),
                    CertName = metadata.CertName,
                    Description = metadata.Description ?? string.Empty,
                    CertificateType = metadata.CertificateType.ToString(),
                    ProviderName = metadata.ProviderName ?? string.Empty,
                    CertCode = metadata.CertCode ?? string.Empty
                };
                
                await _graphService.CreateNode(certificateNode);
                _logger.LogInformation("Created certificate node for certificate {CertificateId}", certificate.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create certificate node for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
                return;
            }

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
                
                await _graphService.CreateRelationship(relationship);
                _logger.LogInformation("Created HAS_CERTIFICATE relationship for certificate {CertificateId}", certificate.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create HAS_CERTIFICATE relationship for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create certificate in graph for certificate {CertificateId}: {Message}", certificate.Id, ex.Message);
        }
    }
}
