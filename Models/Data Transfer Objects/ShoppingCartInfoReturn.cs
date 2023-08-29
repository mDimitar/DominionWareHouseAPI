namespace DominionWarehouseAPI.Models.Data_Transfer_Objects
{
    public class ShoppingCartInfoReturn
    {
        public int Id { get; set; }
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public int Quantity { get; set; }

    }
}
