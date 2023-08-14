namespace DominionWarehouseAPI.Models.Data_Transfer_Objects
{
    public class OrderDTO
    {
        public int UserId { get; set; }
        public string Comment { get; set; }
        public int ShoppingCartId { get; set; }
    }
}
