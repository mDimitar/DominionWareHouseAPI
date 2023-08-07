using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DominionWarehouseAPI.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        [StringLength(500)]
        public string ProductDescription { get; set; }

        //rel m-to-n
        [JsonIgnore]
        public ICollection<ProductsInShoppingCart> ProductShoppingCarts { get; set; }

    }
}
