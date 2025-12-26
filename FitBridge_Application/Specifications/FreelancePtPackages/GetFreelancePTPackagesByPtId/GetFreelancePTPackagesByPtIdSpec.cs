using FitBridge_Domain.Entities.Gyms;

namespace FitBridge_Application.Specifications.FreelancePtPackages.GetFreelancePTPackagesByPtId
{
    public class GetFreelancePTPackagesByPtIdSpec : BaseSpecification<FreelancePTPackage>
    {
        public GetFreelancePTPackagesByPtIdSpec(Guid ptId) : base(x => x.PtId == ptId && x.IsDisplayed)
        {
            AddOrderBy(x => x.Price);
        }
    }
}
