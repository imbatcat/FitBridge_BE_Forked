using System;
using FitBridge_Domain.Entities.Orders;

namespace FitBridge_Application.Specifications.Transactions;

public class GetTransactionByDescriptionSpecification : BaseSpecification<Transaction>
{
    public GetTransactionByDescriptionSpecification(string searchTerm) 
        : base(x => x.Description.Contains(searchTerm))
    {
    }
}



