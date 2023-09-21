namespace DominionWarehouseAPI.Models.Data_Transfer_Objects
{
    public class ProductDTO
    {
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductPrice { get; set; }
        public string? ImageURL { get; set; }
        public int? ProductPriceForSelling { get; set; }
    }
}
