namespace DominionWarehouseAPI.Models.Data_Transfer_Objects
{
    public class UserDTOforRegistering
    {
        public required string Username { get; set; }
        public required string Password { get; set; }

        public required int RoleId { get; set; }
        public required string WorksAt { get; set; }
    }
}
