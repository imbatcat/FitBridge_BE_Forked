using System;
using FitBridge_Application.Dtos.Brands;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Specifications.Brands.GetBrandByName;
using FitBridge_Domain.Entities.Ecommerce;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.Brands.UpdateBrand;

public class UpdateBrandCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateBrandCommand, BrandResponseDto>
{
    public async Task<BrandResponseDto> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await unitOfWork.Repository<Brand>().GetByIdAsync(request.Id, asNoTracking: false)
            ?? throw new NotFoundException(nameof(Brand));
        if (brand == null)
        {
            throw new NotFoundException("Brand not found");
        }
        var spec = new GetBrandByNameSpec(request.Name);
        var existingBrand = await unitOfWork.Repository<Brand>()
            .GetBySpecificationAsync(spec);
        if (existingBrand != null && existingBrand.Id != request.Id)
        {
            throw new DataValidationFailedException($"Brand with name '{request.Name}' already exists.");
        }
        brand.Name = request.Name;
        brand.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.CommitAsync();
        return new BrandResponseDto { Id = brand.Id, Name = brand.Name };
    }
}
