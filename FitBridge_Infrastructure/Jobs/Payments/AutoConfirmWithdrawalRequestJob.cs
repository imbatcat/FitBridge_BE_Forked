using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FitBridge_Infrastructure.Jobs.Payments;

public class AutoConfirmWithdrawalRequestJob(
    ILogger<AutoConfirmWithdrawalRequestJob> _logger, 
    IUnitOfWork _unitOfWork) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var withdrawalRequestId = Guid.Parse(context.JobDetail.JobDataMap.GetString("withdrawalRequestId")
            ?? throw new NotFoundException($"{nameof(AutoConfirmWithdrawalRequestJob)}_withdrawalRequestId"));
        
        _logger.LogInformation("AutoConfirmWithdrawalRequestJob started for WithdrawalRequest: {WithdrawalRequestId}", withdrawalRequestId);
        
        var withdrawalRequest = await _unitOfWork.Repository<WithdrawalRequest>()
            .GetByIdAsync(withdrawalRequestId, asNoTracking: false);
        
        if (withdrawalRequest == null)
        {
            _logger.LogError("WithdrawalRequest not found for Id: {WithdrawalRequestId}", withdrawalRequestId);
            return;
        }
        
        if (withdrawalRequest.Status != WithdrawalRequestStatus.AdminApproved)
        {
            _logger.LogWarning("WithdrawalRequest is not admin-approved, current status: {Status}", withdrawalRequest.Status);
            return;
        }
        
        withdrawalRequest.Status = WithdrawalRequestStatus.Resolved;
        withdrawalRequest.UpdatedAt = DateTime.UtcNow;
        withdrawalRequest.IsUserApproved = true;
        
        _unitOfWork.Repository<WithdrawalRequest>().Update(withdrawalRequest);
        await _unitOfWork.CommitAsync();
        
        _logger.LogInformation("WithdrawalRequest {WithdrawalRequestId} auto-confirmed successfully", withdrawalRequestId);
    }
}
