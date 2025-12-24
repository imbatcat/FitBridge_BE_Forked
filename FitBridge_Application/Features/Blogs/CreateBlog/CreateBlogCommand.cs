using MediatR;

namespace FitBridge_Application.Features.Blogs.CreateBlog;

public class CreateBlogCommand : IRequest<Guid>
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public List<string> Images { get; set; } = new();
}