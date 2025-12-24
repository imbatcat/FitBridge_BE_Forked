using FitBridge_Application.Dtos.Blogs;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Blogging;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Blogs.GetBlogById;

internal class GetBlogByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBlogByIdQuery, BlogDto>
{
    public async Task<BlogDto> Handle(GetBlogByIdQuery request, CancellationToken cancellationToken)
    {
        var blog = await unitOfWork.Repository<Blog>().GetByIdAsync(request.Id)
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
