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
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<Warehouse>> GetAllRoles()
        {
            var roles = dbContext.Roles.ToList();

            if(roles.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false, Message = "No roles can be found in the database." });
            }

            return Ok(roles);
        }

        [HttpPost("AddRole")]
        [Authorize(Roles = "ADMIN")]
        public ActionResult<Roles> RegisterRole(RoleDTO role)
        {
            var existingRole = dbContext.Roles.Any(w => w.RoleName == role.RoleName);

            if(existingRole)
            {
                return BadRequest(new { Success = false, Message = "The role already exists." });
            }

            var newRole = new Roles
            {
                RoleName = role.RoleName,
            };

            dbContext.Roles.Add(newRole);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The role has been added successfully." });
        }

    }
}
