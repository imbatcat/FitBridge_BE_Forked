using FitBridge_Application.Dtos.Blogs;
using MediatR;

namespace FitBridge_Application.Features.Blogs.GetAllBlogs;

public class GetAllBlogsForUserQuery : IRequest<List<BlogDto>>
{
}
