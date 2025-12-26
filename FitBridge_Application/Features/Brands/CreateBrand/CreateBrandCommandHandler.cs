using System;
using FitBridge_Application.Dtos.Brands;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.Brands.GetBrandByName;
using FitBridge_Domain.Entities.Ecommerce;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Brands.CreateBrand;

public class CreateBrandCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateBrandCommand, BrandResponseDto>
{
    public async Task<BrandResponseDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetBrandByNameSpec(request.Name);
        var brand = await unitOfWork.Repository<Brand>()
            .GetBySpecificationAsync(spec);
        if (brand != null)
        {
            throw new DataValidationFailedException($"Brand with name '{request.Name}' already exists.");
        }
        var newBrand = new Brand
        {
            Name = request.Name,
        };
        unitOfWork.Repository<Brand>().Insert(newBrand);
        await unitOfWork.CommitAsync();
        return new BrandResponseDto { Id = newBrand.Id, Name = newBrand.Name };
    }
}
