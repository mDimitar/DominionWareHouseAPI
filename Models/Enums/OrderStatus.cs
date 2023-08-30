using System.ComponentModel.DataAnnotations;

namespace DominionWarehouseAPI.Models.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Processing")]
        Processing,
        [Display(Name = "Delivered")]
        Delivered,
        [Display(Name = "Canceled")]
        Canceled
    }
}
