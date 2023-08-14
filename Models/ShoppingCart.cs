using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DominionWarehouseAPI.Models
{
    public class ShoppingCart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [JsonIgnore]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public ICollection<ProductsInShoppingCart> ProductShoppingCarts { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public int TotalPrice { get; set; }
    }
}
