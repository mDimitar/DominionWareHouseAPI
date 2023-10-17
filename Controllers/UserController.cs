using Azure.Core;
using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("AllUsers")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await dbContext.Users
            .Include(user => user.Role)
            .Select(user => new
            {
                user.Id,
                user.Username,
                WorksAtWarehouse = dbContext.Warehouse.Where(wh => wh.Id == user.WorksAtWarehouse).FirstOrDefault().Name,
                RoleName = user.Role.RoleName
            }).ToListAsync();

            if (users.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "There are no users currently in the database" });
            }
            return Ok(users);
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDTOforRegistering request)
        {

            if(request.Username.IsNullOrEmpty() || request.Password.IsNullOrEmpty() || request.RoleId.Equals(null))
            {
                return BadRequest(new { Success = false, Message = "Username,Password and Role are required fields." });
            }

            var userExists = await dbContext.Users.AnyAsync(user => user.Username == request.Username);

            if (userExists)
            {
                return BadRequest(new { Success = false, Message = "The user already exists.Please enter a new username." });
            }

            if((request.RoleId.Equals(1) || request.RoleId.Equals(3)) && request.WorksAtWarehouse.Equals(null))
            {
                return BadRequest(new { Success = false, Message = "Employee or Owner cannot be set for an undefined warehouse" });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                WorksAtWarehouse = request.WorksAtWarehouse,
                RoleId = request.RoleId,
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The user has been successfully registered." });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserDTOforLogin request)
        {

            if (request.Username.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "Both fields are required." });
            }

            var userExists = await dbContext.Users.AnyAsync(user => user.Username == request.Username);

            if (!userExists)
            {
                return BadRequest(new { Success = false, Message = "The requested user cannot be found." });
            }

            var user = await dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(user => user.Username == request.Username);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest(new { Success = false, Message = "Wrong Password" });
            }

            string token = CreateToken(user);

            return Ok(new { Success = true,User = user.Username,Role = user.Role.RoleName,Message = "Token successfully generated", Token = token, ExpiresAt = DateTimeOffset.Now.AddHours(12).ToUnixTimeMilliseconds() });
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role,user.Role.RoleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(12),
                    signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }

        [HttpPut("EditUser/{id}")]
        [Authorize(Roles = "ADMIN,OWNER")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserDTOForEdit userDTO)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (!IsValidString(userDTO.Password) && !IsValidString(userDTO.Username))
            {
                return BadRequest(new { Success = false, Message = "Invalid data" });
            }

            if (!IsValidString(userDTO.Username))
            {
                return BadRequest(new { Success = false, Message = "Invalid Username" });
            }

            if(!IsValidString(userDTO.Password))
            {
                return BadRequest(new { Success = false, Message = "Invalid Password" });
            }

            if((userDTO.RoleId.Equals(1) || userDTO.RoleId.Equals(3)) && userDTO.WorksAtWarehouse == null)
            {
                return BadRequest(new { Success = false, Message = "Employee or Owner cannot be set for an undefined warehouse" });
            }

            user.Username = userDTO.Username.IsNullOrEmpty() ? user.Username : userDTO.Username;
            user.PasswordHash = userDTO.Password == null ? user.PasswordHash : BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
            user.WorksAtWarehouse = userDTO.WorksAtWarehouse;
            user.RoleId = userDTO.RoleId  == null ? user.RoleId : userDTO.RoleId;

            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The user data has been successfully updated." });
        }

        [HttpDelete("DeleteUser/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public async Task<IActionResult> DeleteUser(int id)
        {

            if (id.Equals(1))
            {
                return BadRequest(new { Success = false, Message = "Admin account cannot be deleted" });
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "The user cannot be found" });
            }

            dbContext.Remove(user);

            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The user has beed successfully deleted" });
        }

        //helper method
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
