using MediatR;

namespace FitBridge_Application.Features.Blogs.EnableBlog;

public class EnableBlogCommand : IRequest
{
    public Guid Id { get; set; }
}
