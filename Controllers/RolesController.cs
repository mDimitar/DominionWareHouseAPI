using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public RolesController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("Roles")]
        public async Task<ActionResult<Warehouse>> GetAllRoles()
        {
            var roles = await dbContext.Roles.ToListAsync();

            if(roles.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false, Message = "No roles can be found in the database." });
            }

            return Ok(roles);
        }

        [HttpPost("AddRole")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RegisterRole(RoleDTO role)
        {
            var existingRole = await dbContext.Roles.AnyAsync(w => w.RoleName == role.RoleName);

            if(existingRole)
            {
                return BadRequest(new { Success = false, Message = "The role already exists." });
            }

            if(role.RoleName.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "Invalid role name." });
            }

            var newRole = new Roles
            {
                RoleName = role.RoleName.Trim().ToUpper(),
            };

            dbContext.Roles.Add(newRole);
            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The role has been added successfully." });
        }

    }
}
