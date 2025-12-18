using System.ComponentModel.DataAnnotations;

namespace FitBridge_Application.Specifications.Dashboards.GetDisbursementDetail
{
    public class GetDisbursementDetailParams : BaseParams
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}
