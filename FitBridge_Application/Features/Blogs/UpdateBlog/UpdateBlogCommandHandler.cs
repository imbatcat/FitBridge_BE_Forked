using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Blogging;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Blogs.UpdateBlog;

internal class UpdateBlogCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateBlogCommand>
{
    public async Task Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await unitOfWork.Repository<Blog>().GetByIdAsync(request.Id, asNoTracking: false)
            ?? throw new NotFoundException(nameof(Blog));

        blog.Title = request.Title;
        blog.Content = request.Content;
        blog.Images = request.Images;
        blog.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Blog>().Update(blog);
        await unitOfWork.CommitAsync();
    }
}
