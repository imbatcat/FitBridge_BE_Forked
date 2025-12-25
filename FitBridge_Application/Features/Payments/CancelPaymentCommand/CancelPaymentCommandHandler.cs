using System;
using MediatR;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Application.Specifications.Transactions;
using FitBridge_Domain.Exceptions;
using FitBridge_Domain.Enums.Orders;
using FitBridge_Domain.Entities.ServicePackages;

namespace FitBridge_Application.Features.Payments.CancelPaymentCommand;

public class CancelPaymentCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<CancelPaymentCommand, bool>
{
    public async Task<bool> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var transactionEntity = await _unitOfWork.Repository<Transaction>().GetBySpecificationAsync(new GetTransactionByOrderCodeSpec(request.OrderCode, true), false);
        if (transactionEntity == null)
        {
            throw new NotFoundException("Transaction not found");
        }
        transactionEntity.Status = TransactionStatus.Failed;
        if (transactionEntity.TransactionType == TransactionType.SubscriptionPlansOrder)
        {
            var userSubscription = await _unitOfWork.Repository<UserSubscription>().GetByIdAsync(transactionEntity.Order.OrderItems.First().UserSubscriptionId.Value);
            if (userSubscription != null)
            {
                _unitOfWork.Repository<UserSubscription>().Delete(userSubscription);
            }
        
        }
        await _unitOfWork.CommitAsync();
        return true;
    }
}
