using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Blogging;

namespace FitBridge_Application.Specifications.Blogs;

public class GetEnabledBlogsSpec : BaseSpecification<Blog>
{
    public GetEnabledBlogsSpec() : base(x => x.IsEnabled)
    {
    }
}
