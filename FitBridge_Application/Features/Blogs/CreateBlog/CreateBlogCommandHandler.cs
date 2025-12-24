using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Domain.Entities.Blogging;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Blogs.CreateBlog;

internal class CreateBlogCommandHandler(
    IUserUtil userUtil,
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBlogCommand, Guid>
{
    public async Task<Guid> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var userId = userUtil.GetAccountId(httpContextAccessor.HttpContext)
            ?? throw new NotFoundException(nameof(ApplicationUser));

        var blog = new Blog
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = userId,
            Images = request.Images
        };

        unitOfWork.Repository<Blog>().Insert(blog);
        await unitOfWork.CommitAsync();

        return blog.Id;
    }
}