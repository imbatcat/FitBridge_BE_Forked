using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Blogging;

namespace FitBridge_Application.Specifications.Blogs;

public class GetBlogByIdSpec : BaseSpecification<Blog>
{
    public GetBlogByIdSpec(Guid blogId) : base(x => x.Id == blogId)
    {
    }
}