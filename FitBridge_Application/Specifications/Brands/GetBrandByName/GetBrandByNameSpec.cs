using System;
using FitBridge_Application.Specifications;
using FitBridge_Domain.Entities.Ecommerce;

namespace FitBridge_Application.Specifications.Brands.GetBrandByName;

public class GetBrandByNameSpec : BaseSpecification<Brand>
{
    public GetBrandByNameSpec(string name) : base(x => x.IsEnabled && x.Name == name)
    {
    }
}
