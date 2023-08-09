using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DominionWarehouseAPI.Models
{
    public class ProductsInShoppingCart
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [JsonIgnore]
        public int ShoppingCartId { get; set; }
        [JsonIgnore]
        public ShoppingCart ShoppingCart { get; set; }
        //quantity logic tbd
        public int Quantity { get; set; }
        
    }
}
