using System.ComponentModel.DataAnnotations;

namespace DominionWarehouseAPI.Models.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Processing")]
        Processing,
        [Display(Name = "Shipped")]
        Shipped,
        [Display(Name = "Canceled")]
        Canceled
    }
}
