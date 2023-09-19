namespace DominionWarehouseAPI.Models.Data_Transfer_Objects
{
    public class UserDTOForEdit
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public int? WorksAtWarehouse { get; set; }
    }
}
