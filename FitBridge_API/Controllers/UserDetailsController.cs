using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FitBridge_API.Helpers.RequestHelpers;
using FitBridge_Application.Dtos.Accounts.UserDetails;
using FitBridge_Application.Features.UserDetails.GetUserDetail;
using FitBridge_Application.Features.UserDetails.UpdateUserDetails;

namespace FitBridge_API.Controllers;

public class UserDetailsController(IMediator _mediator) : _BaseApiController
{
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetUserDetails([FromRoute] Guid customerId)
    {
        var result = await _mediator.Send(new GetUserDetailsQuery { CustomerId = customerId });
        return Ok(new BaseResponse<UserDetailDto>(StatusCodes.Status200OK.ToString(), "User details fetched successfully", result));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDetailCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<UserDetailDto>(StatusCodes.Status200OK.ToString(), "User details updated successfully", result));
    }
}
