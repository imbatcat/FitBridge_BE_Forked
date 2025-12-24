using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Blogging;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Blogs.EnableBlog;

internal class EnableBlogCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<EnableBlogCommand>
{
    public async Task Handle(EnableBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await unitOfWork.Repository<Blog>().GetByIdAsync(request.Id, asNoTracking: false)
            ?? throw new NotFoundException(nameof(Blog));

        blog.IsEnabled = true;
        blog.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Repository<Blog>().Update(blog);
        await unitOfWork.CommitAsync();
    }
}
