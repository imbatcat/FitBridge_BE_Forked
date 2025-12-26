using AutoMapper;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Reports;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.Reports.GetAllReports;
using FitBridge_Application.Specifications.Reports.GetReportSummary;
using FitBridge_Domain.Entities.Orders;
using FitBridge_Domain.Entities.Reports;
using FitBridge_Domain.Enums.Reports;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FitBridge_Application.Features.Reports.GetAllReports
{
    internal class GetAllReportsQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ITransactionService transactionService) : IRequestHandler<GetAllReportsQuery, ReportPagingResultDto>
    {
        public async Task<ReportPagingResultDto> Handle(GetAllReportsQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetAllReportsSpec(request.Params);

            var reports = await unitOfWork.Repository<ReportCases>()
                .GetAllWithSpecificationAsync(spec);

            var mappedReports = new List<GetCustomerReportsResponseDto>();

            foreach (var report in reports)
            {
                var dto = mapper.Map<GetCustomerReportsResponseDto>(report);
                
                if (report.OrderItem != null)
                {
                    dto.RefundAmount = await CalculateRefundAmount(report.OrderItem);
                }

                mappedReports.Add(dto);
            }

            var totalItems = await unitOfWork.Repository<ReportCases>()
                .CountAsync(spec);

            var repository = unitOfWork.Repository<ReportCases>();
            var summary = new ReportSummaryResponseDto
            {
                TotalReports = await repository.CountAsync(new ReportByTypeSpecification()),
                ProductReportCount = await repository.CountAsync(new ReportByTypeSpecification(ReportCaseType.ProductReport)),
                FreelancePtReportCount = await repository.CountAsync(new ReportByTypeSpecification(ReportCaseType.FreelancePtReport)),
                GymCourseReportCount = await repository.CountAsync(new ReportByTypeSpecification(ReportCaseType.GymCourseReport)),
                PendingCount = await repository.CountAsync(new ReportByStatusSpecification(ReportCaseStatus.Pending)),
                ProcessingCount = await repository.CountAsync(new ReportByStatusSpecification(ReportCaseStatus.Processing)),
                ResolvedCount = await repository.CountAsync(new ReportByStatusSpecification(ReportCaseStatus.Resolved)),
                FraudConfirmedCount = await repository.CountAsync(new ReportByStatusSpecification(ReportCaseStatus.FraudConfirmed))
            };

            return new ReportPagingResultDto
            {
                Total = totalItems,
                Items = mappedReports,
                Summary = summary
            };
        }

        private async Task<decimal> CalculateRefundAmount(OrderItem orderItem)
        {
            var isProduct = orderItem.ProductDetailId.HasValue;

            if (isProduct)
            {
                // Calculate product refund amount
                var discountAmount = 0m;
                if (orderItem.Order?.Coupon != null)
                {
                    var coupon = orderItem.Order.Coupon;
                    discountAmount = Math.Min(
                        orderItem.Price * (decimal)coupon.DiscountPercent / 100, 
                        coupon.MaxDiscount);
                }
                return orderItem.Price - discountAmount;
            }
            else
            {
                // Calculate service (GymCourse/FreelancePTPackage) refund amount
                return await transactionService.CalculateMerchantProfit(orderItem, orderItem.Order?.Coupon);
            }
        }
    }
}