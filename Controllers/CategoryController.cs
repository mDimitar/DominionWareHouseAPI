using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class CategoryController : ControllerBase
    {

        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public CategoryController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("Categories")]
        public IActionResult GetAllCategories()
        {

            var categories = dbContext.Categories.ToList();

            if (categories.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false, Message = "There are no existing categories in the database." });
            }
            return Ok(categories);
        }



        [HttpPost("RegisterCategory")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult RegisterCategory(CategoryDTO request)
        {

            if(!IsValidString(request.CategoryName))
            {
                return BadRequest(new { Success = false, Message = "Invalid Category name." });
            }

            var category = dbContext.Categories.FirstOrDefault(c => c.CategoryName == request.CategoryName);

            if (category != null)
            {
                return BadRequest(new { Success = false, Message = "The category already exists." });
            }

            var newCategory = new Category
            {
                CategoryName = request.CategoryName,
            };

            dbContext.Categories.Add(newCategory);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The category has been registered successfully." });
        }

        [HttpPut("EditCategory/{id}")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult EditCategory(int id, [FromBody] CategoryDTO request)
        {

            if (!IsValidString(request.CategoryName))
            {
                return BadRequest(new { Success = false, Message = "Invalid category name." });
            }

            var category = dbContext.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return BadRequest(new { Success = false, Message = "There requested category cannot be found." });
            }

            category.CategoryName = request.CategoryName;

            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The changes have been registered successfully." });
        }

        [HttpDelete("DeleteCategory/{id}")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult DeleteCategory(int id)
        {
            var category = dbContext.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return BadRequest(new { Success = false, Message = "There requested category cannot be found." });
            }

            dbContext.Categories.Remove(category);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The category has been deleted successfully." });
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
