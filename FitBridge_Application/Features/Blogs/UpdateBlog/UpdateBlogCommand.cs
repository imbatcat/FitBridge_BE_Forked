using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Blogs.UpdateBlog;

public class UpdateBlogCommand : IRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
}
