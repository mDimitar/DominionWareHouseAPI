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
            var warehouses = await dbContext.Warehouse.ToListAsync();
            if (warehouses.IsNullOrEmpty())
            {
                return BadRequest("warehouse empty");
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

    }
}
