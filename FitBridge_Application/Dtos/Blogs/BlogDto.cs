namespace FitBridge_Application.Dtos.Blogs;

public class BlogDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Guid AuthorId { get; set; }

    public List<string> Images { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}