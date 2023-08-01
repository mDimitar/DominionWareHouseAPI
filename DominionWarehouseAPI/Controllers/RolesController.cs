using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using DominionWarehouseAPI.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            return Ok(roles);
        }

        [HttpPost("AddRole")]
        [Authorize(Roles = "ADMIN")]
        public ActionResult<Roles> RegisterRole(RoleDTO role)
        {
            var existingRole = dbContext.Roles.Any(w => w.RoleName == role.RoleName);

            if(existingRole)
            {
                return BadRequest("role already exists");
            }

            var newRole = new Roles
            {
                RoleName = role.RoleName,
            };

            dbContext.Roles.Add(newRole);
            dbContext.SaveChanges();

            return Ok(role);
        }

    }
}
