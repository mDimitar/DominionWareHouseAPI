using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DominionWarehouseAPI.Models
{
    public class ProductsInWarehouse
    {

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
