using System.Text.Json.Serialization;

namespace DominionWarehouseAPI.Response
{
    public class CustomizedResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public string Token { get; set; }
    }
}
