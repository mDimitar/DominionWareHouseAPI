using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using DominionWarehouseAPI.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<Warehouse>> GetAllWarehouses()
        {
            var warehouses = await dbContext.Warehouse.Include(w => w.User).ToListAsync();

            if (warehouses.IsNullOrEmpty())
            {
                var failPullResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "There are no existing warehouse records in the database.",
                };
                return new JsonResult(failPullResponse);
            }

            return Ok(warehouses);
        }

        [HttpPost("RegisterWarehouse")]
        public ActionResult<Warehouse> RegisterWarehouse(WarehouseDTO warehouse)
        {
            var warehouseExists = dbContext.Warehouse.Any(w => w.Name == warehouse.Name);

            if (warehouseExists)
            {
                var failedResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "The warehouse already exists. Please enter a new name.",
                };
                return new JsonResult(failedResponse);
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

                var successfullResponse = new CustomizedResponse
                {
                    Success = true,
                    Message = "The warehouse has been successfully registered.",
                };

                return new JsonResult(successfullResponse);
            }
            else
            {
                // Handle the case when the user is not authenticated
                var unauthorizedResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "You must be logged in to register a warehouse.",
                };

                return new JsonResult(unauthorizedResponse);
            }
        }

        //edit option

        [HttpPut("EditWarehouse/{id}")]
        public IActionResult EditWarehouse(int id, [FromBody] WarehouseDTO updatedWarehouseDTO)
        {
            var existingWarehouse = dbContext.Warehouse.Include(w => w.User).FirstOrDefault(w => w.Id == id);

            if (existingWarehouse == null)
            {
                var failEditResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "The warehouse you are trying to edit cannot be found.",
                };
                return new JsonResult(failEditResponse);
            }

            existingWarehouse.Name = updatedWarehouseDTO.Name;
            existingWarehouse.Address = updatedWarehouseDTO.Address;

            dbContext.SaveChanges();

            var successEditResponse = new CustomizedResponse
            {
                Success = true,
                Message = "The changes has been successfully registered.",
            };

            return new JsonResult(successEditResponse);
        }

        [HttpDelete("DeleteWarehouse/{id}")]
        public IActionResult DeleteWarehouse(int id)
        {
            var warehouse = dbContext.Warehouse.FirstOrDefault(w => w.Id == id);
            
            if(warehouse == null)
            {
                var failDeleteResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "The warehouse you are trying to delete cannot be found.",
                };
                return new JsonResult(failDeleteResponse);
            }

            dbContext.Remove(warehouse);

            dbContext.SaveChanges();

            var successDeleteResponse = new CustomizedResponse
            {
                Success = true,
                Message = "The changes has been successfully deleted.",
            };

            return new JsonResult(successDeleteResponse);
        }

    }
}
