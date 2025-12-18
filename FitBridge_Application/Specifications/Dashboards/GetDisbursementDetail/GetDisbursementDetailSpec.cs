using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Enums.Orders;

namespace FitBridge_Application.Specifications.Dashboards.GetDisbursementDetail
{
    public class GetDisbursementDetailSpec : BaseSpecification<Transaction>
    {
        public GetDisbursementDetailSpec(Guid userId, GetDisbursementDetailParams parameters) : base(x =>
            x.TransactionType == TransactionType.Disbursement
            && x.WalletId == userId
            // Filter by date range - From date
            && (!parameters.From.HasValue || x.CreatedAt >= parameters.From.Value)
            // Filter by date range - To date (inclusive, end of day)
            && (!parameters.To.HasValue || x.CreatedAt <= parameters.To.Value.Date.AddDays(1).AddTicks(-1)))
        {
            AddInclude(x => x.WithdrawalRequest);

            AddOrderBy(x => x.CreatedAt);

            if (parameters.DoApplyPaging)
            {
                AddPaging((parameters.Page - 1) * parameters.Size, parameters.Size);
            }
            else
            {
                parameters.Size = -1;
                parameters.Page = -1;
            }
        }
    }
}
