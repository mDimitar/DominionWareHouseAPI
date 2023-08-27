namespace DominionWarehouseAPI.Models.Data_Transfer_Objects
{
    public class ProductWarehouseDTOForEdit
    {
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int ProductPriceForSelling { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
    }
}
