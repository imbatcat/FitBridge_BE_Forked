using System;

namespace FitBridge_Application.Dtos.RevenueCat;

public class ExperimentDto
{
    public string ExperimentId { get; set; }
    public string ExperimentVariant { get; set; }
    public long EnrolledAtMs { get; set; }
}



