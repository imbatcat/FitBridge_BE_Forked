using System;
using FitBridge_Application.Dtos.Brands;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace FitBridge_Application.Features.Brands.CreateBrand;

public class CreateBrandCommand : IRequest<BrandResponseDto>
{
    [Required]
    public string Name { get; set; }
}
