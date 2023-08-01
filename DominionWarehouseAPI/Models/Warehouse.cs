using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DominionWarehouseAPI.Models
{
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        [Required]
        public int userId { get; set; }
        public virtual User User{ get; set; }
    }
}
