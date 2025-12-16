using System;

namespace FitBridge_Application.Dtos.GymAssets;

public class SessionActivityAssetDto
{
    public Guid AssetId { get; set; }
    public string VietNameseName { get; set; }
    public string Name { get; set; }
    public string VietnameseDescription { get; set; }
    public List<string>? TargetMuscularGroups { get; set; } = new List<string>();
    public string? MetadataImage { get; set; }
}
