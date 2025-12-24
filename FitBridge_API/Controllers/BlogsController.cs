using FitBridge_API.Helpers.RequestHelpers;
using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Dtos.Blogs;
using FitBridge_Application.Features.Blogs.CreateBlog;
using FitBridge_Application.Features.Blogs.DeleteBlog;
using FitBridge_Application.Features.Blogs.GetAllBlogs;
using FitBridge_Application.Features.Blogs.GetBlogById;
using FitBridge_Application.Features.Blogs.UpdateBlog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitBridge_API.Controllers;

public class BlogsController(IMediator _mediator) : _BaseApiController
{
    [HttpPost]
    [Authorize(Roles = ProjectConstant.UserRoles.Admin)]
    public async Task<IActionResult> CreateBlog([FromBody] CreateBlogCommand command)
    {
        var blogId = await _mediator.Send(command);
        return Ok(new BaseResponse<Guid>(StatusCodes.Status201Created.ToString(), "Blog created successfully", blogId));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBlogById(Guid id)
    {
        if (User.IsInRole(ProjectConstant.UserRoles.Admin))
        {
            var adminQuery = new GetBlogByIdQuery { Id = id };
            var adminBlog = await _mediator.Send(adminQuery);
            return Ok(new BaseResponse<BlogDto>(StatusCodes.Status200OK.ToString(), "Blog retrieved successfully", adminBlog));
        }
        else
        {
            var userQuery = new GetBlogByIdForUserQuery { Id = id };
            var userBlog = await _mediator.Send(userQuery);
            return Ok(new BaseResponse<BlogDto>(StatusCodes.Status200OK.ToString(), "Blog retrieved successfully", userBlog));
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllBlogs()
    {
        if (User.IsInRole(ProjectConstant.UserRoles.Admin))
        {
            var adminQuery = new GetAllBlogsQuery();
            var adminBlogs = await _mediator.Send(adminQuery);
            return Ok(new BaseResponse<List<BlogDto>>(StatusCodes.Status200OK.ToString(), "Blogs retrieved successfully", adminBlogs));
        }
        else
        {
            var userQuery = new GetAllBlogsForUserQuery();
            var userBlogs = await _mediator.Send(userQuery);
            return Ok(new BaseResponse<List<BlogDto>>(StatusCodes.Status200OK.ToString(), "Blogs retrieved successfully", userBlogs));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = ProjectConstant.UserRoles.Admin)]
    public async Task<IActionResult> UpdateBlog(Guid id, [FromBody] UpdateBlogCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return Ok(new BaseResponse<object>(StatusCodes.Status200OK.ToString(), "Blog updated successfully", null));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = ProjectConstant.UserRoles.Admin)]
    public async Task<IActionResult> DeleteBlog(Guid id)
    {
        var command = new DeleteBlogCommand { Id = id };
        await _mediator.Send(command);
        return Ok(new BaseResponse<object>(StatusCodes.Status200OK.ToString(), "Blog deleted successfully", null));
    }
}