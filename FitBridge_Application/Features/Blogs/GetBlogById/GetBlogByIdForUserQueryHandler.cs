using FitBridge_Application.Dtos.Blogs;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.Blogs;
using FitBridge_Domain.Entities.Blogging;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Blogs.GetBlogById;

internal class GetBlogByIdForUserQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBlogByIdForUserQuery, BlogDto>
{
    public async Task<BlogDto> Handle(GetBlogByIdForUserQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetBlogByIdForUserSpec(request.Id);
        var blog = await unitOfWork.Repository<Blog>().GetBySpecificationAsync(spec)
            ?? throw new NotFoundException(nameof(Blog));

        return new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            AuthorId = blog.AuthorId,
            Images = blog.Images,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };
    }
}
