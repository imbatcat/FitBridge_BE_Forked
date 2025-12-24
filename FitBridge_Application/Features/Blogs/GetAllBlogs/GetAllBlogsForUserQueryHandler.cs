using FitBridge_Application.Dtos.Blogs;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.Blogs;
using FitBridge_Domain.Entities.Blogging;
using MediatR;

namespace FitBridge_Application.Features.Blogs.GetAllBlogs;

internal class GetAllBlogsForUserQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllBlogsForUserQuery, List<BlogDto>>
{
    public async Task<List<BlogDto>> Handle(GetAllBlogsForUserQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetEnabledBlogsSpec();
        var blogs = await unitOfWork.Repository<Blog>().GetAllWithSpecificationAsync(spec);

        return blogs.Select(blog => new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            AuthorId = blog.AuthorId,
            Images = blog.Images,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt,
            IsEnabled = blog.IsEnabled,
        }).ToList();
    }
}