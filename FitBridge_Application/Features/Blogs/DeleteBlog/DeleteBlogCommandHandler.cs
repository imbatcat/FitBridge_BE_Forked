using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Blogging;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Blogs.DeleteBlog;

internal class DeleteBlogCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteBlogCommand>
{
    public async Task Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await unitOfWork.Repository<Blog>().GetByIdAsync(request.Id, asNoTracking: false)
            ?? throw new NotFoundException(nameof(Blog));

        unitOfWork.Repository<Blog>().SoftDelete(blog);
        await unitOfWork.CommitAsync();
    }
}