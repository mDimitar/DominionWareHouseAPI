using Azure.Core;
using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        public ActionResult<User> GetAllUsers()
        {
            var users = dbContext.Users
            .Include(user => user.Role)
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.WorksAtWarehouse,
                RoleName = user.Role.RoleName
            })
            .ToList(); ;

            if(users.IsNullOrEmpty())
            {
                var failResponse = new
                {
                    Success = false,
                    Message = "There are no users currently in the database"
                };
                return BadRequest(failResponse);
            }
            return Ok(users);
        }


        [HttpPost("Register")]
        public ActionResult<User> Register(UserDTOforRegistering request)
        {
            var userExists = dbContext.Users.Any(user => user.Username == request.Username);

            if (userExists)
            {

                var failedResponse = new
                {
                    Success = false,
                    Message = "The user already exists.Please enter a new username.",
                };

                return new JsonResult(failedResponse);
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
            dbContext.SaveChanges();

            var successfullResponse = new
            {
                Success = true,
                Message = "The user has been successfully registered.",
            };

            return new JsonResult(successfullResponse);
        }

        [HttpPost("Login")]
        public ActionResult<User> Login(UserDTOforLogin request)
        {
            var userExists = dbContext.Users.Any(user => user.Username == request.Username);

            if (!userExists)
            {
                var failResponse = new
                {
                    Success = false,
                    Message = "The requested user cannot be found.",
                };
                return new JsonResult(failResponse);
            }

            var user = dbContext.Users.Include(u => u.Role).FirstOrDefault(user => user.Username == request.Username);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var failResponse = new
                {
                    Success = false,
                    Message = "Wrong password."
                };
                return new JsonResult(failResponse);
            }

            string token = CreateToken(user);

            var successResponse = new
            {
                Success = true,
                Message = "Token successfully generated.",
                Token = token
            };

            return new JsonResult(successResponse);
        }


        //validation starts here

        /*[HttpPost]
        [Route("VerifyToken")]
        public IActionResult VerifyToken([FromBody] string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };


                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                var successResponse = new CustomizedResponse
                {
                    Success = true,
                    Message = "The requested token is valid.",
                };

                return new JsonResult(successResponse);
            }
            catch (Exception)
            {
                var failResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "The requested token is not valid.",
                };

                return new JsonResult(failResponse);
            }
        }*/

        //and ends here


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
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }

        //edit user

        [HttpPut("EditUser/{id}")]
        [Authorize(Roles = "ADMIN,OWNER")]
        public IActionResult EditUser(int id, [FromBody] UserDTOforRegistering userDTO)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);

            string NewPasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

            user.Username = userDTO.Username;
            user.WorksAtWarehouse = userDTO.WorksAtWarehouse;
            user.PasswordHash = NewPasswordHash;

            dbContext.SaveChanges();

            var successResponse = new
            {
                Success = true,
                Message = "The user data has been successfully updated.",
            };

            return new JsonResult(successResponse);
        }

        [HttpDelete("DeleteUser/{id}")]
        [Authorize(Roles = "OWNER,ADMIN")]
        public IActionResult DeleteUser(int id)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);

            dbContext.Remove(user);

            dbContext.SaveChanges();

            var successResponse = new
            {
                Success = true,
                Message = "The user has been successfully deleted.",
            };

            return new JsonResult(successResponse);
        }


    }

}
