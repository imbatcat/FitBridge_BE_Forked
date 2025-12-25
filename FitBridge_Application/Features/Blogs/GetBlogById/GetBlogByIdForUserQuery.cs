using FitBridge_Application.Dtos.Blogs;
using MediatR;

namespace FitBridge_Application.Features.Blogs.GetBlogById;

public class GetBlogByIdForUserQuery : IRequest<BlogDto>
{
    public Guid Id { get; set; }
}
