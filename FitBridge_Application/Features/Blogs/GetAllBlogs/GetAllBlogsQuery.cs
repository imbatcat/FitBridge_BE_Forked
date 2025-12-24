using FitBridge_Application.Dtos.Blogs;
using MediatR;

namespace FitBridge_Application.Features.Blogs.GetAllBlogs;

public class GetAllBlogsQuery : IRequest<List<BlogDto>>
{
}