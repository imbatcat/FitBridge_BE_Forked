using MediatR;

namespace FitBridge_Application.Features.Blogs.DeleteBlog;

public class DeleteBlogCommand : IRequest
{
    public Guid Id { get; set; }
}
