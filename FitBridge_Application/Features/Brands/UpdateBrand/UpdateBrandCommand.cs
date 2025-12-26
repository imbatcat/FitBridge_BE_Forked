using System;
using FitBridge_Application.Dtos.Brands;
using MediatR;
using System.Text.Json.Serialization;

namespace FitBridge_Application.Features.Brands.UpdateBrand;

public class UpdateBrandCommand : IRequest<BrandResponseDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string Name { get; set; }
}
