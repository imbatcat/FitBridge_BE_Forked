using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FitBridge_API.Helpers.RequestHelpers;
using FitBridge_Application.Dtos.Brands;
using FitBridge_Application.Features.Brands.GetAllBrands;
using FitBridge_Application.Features.Brands.CreateBrand;
using FitBridge_Application.Features.Brands.UpdateBrand;
namespace FitBridge_API.Controllers;

public class BrandsController(IMediator _mediator) : _BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAllBrands()
    {
        var result = await _mediator.Send(new GetAllBrandsQuery());
        return Ok(new BaseResponse<List<BrandResponseDto>>(StatusCodes.Status200OK.ToString(), "Brands retrieved successfully", result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<BrandResponseDto>(StatusCodes.Status200OK.ToString(), "Brand created successfully", result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand([FromRoute] Guid id, [FromBody] UpdateBrandCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(new BaseResponse<BrandResponseDto>(StatusCodes.Status200OK.ToString(), "Brand updated successfully", result));
    }
}
