using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DominionWarehouseAPI.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;

        [JsonIgnore]
        public int? WorksAtWarehouse { get; set; }
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [ForeignKey("Roles")]
        public int? RoleId { get; set; }
        public Roles Role { get; set; }

        [ForeignKey("ShoppingCart")]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        [JsonIgnore]
        public ICollection<ReceivedGoodsBy> ReceivedGoods { get; set; } = new List<ReceivedGoodsBy>();

    }
}
