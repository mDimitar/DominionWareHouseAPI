using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class WarehouseController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public WarehouseController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("Warehouses")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public async Task<ActionResult<Warehouse>> GetAllWarehouses()
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(r => r.Role).FirstOrDefault(u => u.Username == username);

            if (user.Role.RoleName.Equals("ADMIN"))
            {
                var allWarehouses = dbContext.Warehouse.Include(u => u.User).Include(pr => pr.WarehouseProducts).ToList();

                if (allWarehouses.IsNullOrEmpty())
                {
                    return BadRequest(new {Success = false, Message = "No warehouses found in the database"});
                }
                    
                return Ok(dbContext.Warehouse.Include(u => u.User).ToList());
            }

            var warehouses = await dbContext.Warehouse.Where(w => w.userId == user.Id).ToListAsync();

            if (warehouses.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false, Message = "There are no existing warehouse records in the database." });
            }

            return Ok(warehouses);
        }

        [HttpPost("RegisterWarehouse")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public ActionResult<Warehouse> RegisterWarehouse(WarehouseDTO warehouse)
        {
            var warehouses = dbContext.Warehouse.ToList();

            if(warehouses.Count > 1)
            {
                return BadRequest(new { Success = false, Message = "You have already registered a warehouse." });
            }

            var warehouseExists = dbContext.Warehouse.Any(w => w.Name == warehouse.Name);

            if (warehouseExists)
            {
                return BadRequest(new { Success = false, Message = "The warehouse already exists. Please enter a new name." });
            }

            // Check if the user is authenticated and get their UserId
            if (HttpContext.User.Identity.IsAuthenticated)
            {

                string username = User.FindFirstValue(ClaimTypes.Name);

                var user = dbContext.Users.FirstOrDefault(u => u.Username == username);

                var newWarehouse = new Warehouse
                {
                    Name = warehouse.Name,
                    Address = warehouse.Address,
                    User = user
                };

                dbContext.Warehouse.Add(newWarehouse);
                dbContext.SaveChanges();

                user.WorksAtWarehouse = newWarehouse.Id;
                dbContext.SaveChanges();

                return Ok(new {Success = true, Message = "The warehouse has been successfully registered." });
            }
            else
            {
                return BadRequest(new {Success = false, Message = "You must be logged in to register a warehouse." });
            }
        }

        [HttpPut("EditWarehouse/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public IActionResult EditWarehouse(int id, [FromBody] WarehouseDTO updatedWarehouseDTO)
        {
            var existingWarehouse = dbContext.Warehouse.Include(w => w.User).FirstOrDefault(w => w.Id == id);

            if (existingWarehouse == null)
            {
                return BadRequest(new {Success = false, Message = "The warehouse you are trying to edit cannot be found." });
            }

            existingWarehouse.Name = updatedWarehouseDTO.Name;
            existingWarehouse.Address = updatedWarehouseDTO.Address;

            dbContext.SaveChanges();

            return Ok(new {Success = true, Message = "The changes has been successfully registered." });
        }

        [HttpDelete("DeleteWarehouse/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public IActionResult DeleteWarehouse(int id)
        {
            var warehouse = dbContext.Warehouse.FirstOrDefault(w => w.Id == id);
            
            if(warehouse == null)
            {
                return BadRequest(new {Success = false, Message = "The warehouse you are trying to delete cannot be found." });
            }

            dbContext.Remove(warehouse);

            dbContext.SaveChanges();

            return Ok(new {Success = true, Message = "The changes has been successfully deleted." });
        }

    }
}
