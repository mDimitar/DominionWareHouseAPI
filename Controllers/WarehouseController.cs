using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.RegularExpressions;

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
        [Authorize(Roles = "OWNER,ADMIN,EMPLOYEE")]
        public async Task<ActionResult<Warehouse>> GetAllWarehouses()
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = await dbContext.Users.Include(r => r.Role).FirstOrDefaultAsync(u => u.Username == username);

            if (user.Role.RoleName.Equals("ADMIN"))
            {
                var allWarehouses = await dbContext.Warehouse.Include(u => u.User).Include(pr => pr.WarehouseProducts).ToListAsync();

                if (allWarehouses.IsNullOrEmpty())
                {
                    return BadRequest(new {Success = false, Message = "No warehouses found in the database"});
                }
                    
                return Ok(dbContext.Warehouse.Include(u => u.User).ToList());
            }

            var warehouses = await dbContext.Warehouse.ToListAsync();

            if (warehouses.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false, Message = "There are no existing warehouse records in the database." });
            }

            return Ok(warehouses);
        }

        [HttpPost("RegisterWarehouse")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public async Task<IActionResult> RegisterWarehouse(WarehouseDTO warehouse)
        {
            var warehouses = await dbContext.Warehouse.ToListAsync();

            if(warehouses.Count >= 1)
            {
                return BadRequest(new { Success = false, Message = "You have already registered a warehouse." });
            }

            if(!warehouse.Address.IsNullOrEmpty() || !warehouse.Name.IsNullOrEmpty())
            {
                if(!IsValidString(warehouse.Address) && !IsValidString(warehouse.Name))
                {
                    return BadRequest(new { Success = false, Message = "Invalid data." });
                }
                if (!IsValidString(warehouse.Address) && IsValidString(warehouse.Name))
                {
                    return BadRequest(new { Success = false, Message = "Invalid address." });
                }
                if (IsValidString(warehouse.Address) && !IsValidString(warehouse.Name))
                {
                    return BadRequest(new { Success = false, Message = "Invalid name." });
                }
            }
            else
            {
                return BadRequest(new { Success = false, Message = "Invalid data." });
            }

            // Check if the user is authenticated and get their UserId
            if (HttpContext.User.Identity.IsAuthenticated)
            {

                string username = User.FindFirstValue(ClaimTypes.Name);

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                var newWarehouse = new Warehouse
                {
                    Name = warehouse.Name,
                    Address = warehouse.Address,
                    User = user
                };

                dbContext.Warehouse.Add(newWarehouse);
                await dbContext.SaveChangesAsync(CancellationToken.None);

                user.WorksAtWarehouse = newWarehouse.Id;
                await dbContext.SaveChangesAsync(CancellationToken.None);

                return Ok(new {Success = true, Message = "The warehouse has been successfully registered." });
            }
            else
            {
                return BadRequest(new {Success = false, Message = "You must be logged in to register a warehouse." });
            }
        }
        static bool IsValidString(string input)
        {
            string validPattern = "^[a-zA-Z0-9!@#$%^&*]+( [a-zA-Z0-9!@#$%^&*]+)*$";

            if (input.IsNullOrEmpty())
            {
                return true;
            }

            return Regex.IsMatch(input, validPattern) && !string.IsNullOrWhiteSpace(input);
        }

    }
}
