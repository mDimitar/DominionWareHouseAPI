using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using DominionWarehouseAPI.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("register")]
        public ActionResult<User> Register(UserDTO request)
        {
            var userExists = dbContext.Users.Any(user => user.Username == request.Username);

            if (userExists)
            {

                var failedResponse = new CustomizedResponse
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
                PasswordHash = passwordHash
            };

            dbContext.Users.Add(newUser);
            dbContext.SaveChanges();

            var successfullResponse = new CustomizedResponse
            {
                Success = true,
                Message = "The user has been successfully registered.",
            };

            return new JsonResult(successfullResponse);
        }

        [HttpPost("login")]
        public ActionResult<User> Login(UserDTO request)
        {
            var userExists = dbContext.Users.Any(user => user.Username == request.Username);

            if (!userExists)
            {
                var failResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "The requested user cannot be found.",
                };
                return new JsonResult(failResponse);
            }

            var user = dbContext.Users.FirstOrDefault(user => user.Username == request.Username);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var failResponse = new CustomizedResponse
                {
                    Success = false,
                    Message = "Wrong password."
                };
                return new JsonResult(failResponse);
            }

            string token = CreateToken(user);

            var successResponse = new CustomizedResponse
            {
                Success = true,
                Message = "Token successfully generated.",
                Token = token
            };

            return new JsonResult(successResponse);
        }


        //validation starts here

        [HttpPost]
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
        }

        //and ends here


        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
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

    }

}
