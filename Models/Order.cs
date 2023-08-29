using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DominionWarehouseAPI.Models.Enums;

namespace DominionWarehouseAPI.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public int TotalSum { get; set; }

        public string Comment { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Processing;

        [ForeignKey("ShoppingCart")]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }

        public int soldFromWarehouseId { get; set; }

        public int? soldFromEmployeeId { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
