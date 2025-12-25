using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Blogging;

namespace FitBridge_Application.Specifications.Blogs;

public class GetBlogByIdForUserSpec : BaseSpecification<Blog>
{
    public GetBlogByIdForUserSpec(Guid blogId) : base(x => x.Id == blogId && x.IsEnabled)
    {
    }
}
