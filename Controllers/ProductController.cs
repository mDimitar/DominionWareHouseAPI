using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,EMPLOYEE,OWNER")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public ProductController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("Products")]
        public async Task<ActionResult<Warehouse>> GetAllProducts()
        {

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var products = dbContext.Products.Include(cat => cat.Category).ToList();

            if (products.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false, Message = "No products can be found in the database." });
            }

            return Ok(products);
        }

        [HttpPost("RegisterProduct")]
        public ActionResult<Product> RegisterProduct(ProductDTO request)
        {
            var product = dbContext.Products.Any(p => p.ProductName == request.ProductName);

            if(request.ProductName.IsNullOrEmpty() || request.ProductDescription.IsNullOrEmpty()
                || request.CategoryId.Equals(null) || request.ProductPrice.Equals(null)
                || request.ImageURL.IsNullOrEmpty() || request.ProductPriceForSelling.Equals(null))
            {
                return BadRequest(new { Success = false, Message = "Invalid data." });
            }

            if (product)
            {
                return BadRequest(new { Success = false, Message = "The product already exists. Please enter a new name." });
            }

            var newProduct = new Product
            {
                ProductName = request.ProductName,
                ProductDescription = request.ProductDescription,
                CategoryId = (int)request.CategoryId,
                ProductPrice = (int)request.ProductPrice,
                ProductImageURL = request.ImageURL,
                ProductPriceForSelling = (int)request.ProductPriceForSelling,
            };

            dbContext.Products.Add(newProduct);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The product has been successfully registered." });
        }


        [HttpPut("EditProduct/{id}")]
        public ActionResult<Product> EditProducts(int id,[FromBody] ProductDTO request)
        {
            var product = dbContext.Products.Any(p => p.Id == id);

            if(request.ProductName.IsNullOrEmpty() && request.ProductDescription.IsNullOrEmpty() &&
                request.CategoryId.Equals(null) && request.ProductPrice.Equals(null) &&
                request.ImageURL.IsNullOrEmpty() && request.ProductPriceForSelling.Equals(null))
            {
                return BadRequest(new { Success = false, Message = "Invalid data" });
            }

            if (!product)
            {
                return BadRequest(new { Success = false, Message = "The product does not exists" });
            }

            if(request.ProductPrice < 0)
            {
                return BadRequest(new { Success = false, Message = "Invalid product procurement price" });
            }
            
            if(request.ProductPriceForSelling < 0)
            {
                return BadRequest(new { Success = false, Message = "Invalid product price for selling" });
            }

            if(!IsValidString(request.ProductName))
            {
                return BadRequest(new { Success = false, Message = "Invalid product name" });
            }

            if(!IsValidString(request.ProductDescription))
            {
                return BadRequest(new { Success = false, Message = "Invalid product description" });
            }

            if(!IsValidString(request.ImageURL))
            {
                return BadRequest(new { Success = false, Message = "Invalid product image URL" });
            }

            var productToBeEdited = dbContext.Products.SingleOrDefault(p => p.Id == id);

            productToBeEdited.ProductName = request.ProductName.IsNullOrEmpty() ? productToBeEdited.ProductName : request.ProductName;
            productToBeEdited.ProductDescription = request.ProductDescription.IsNullOrEmpty() ? productToBeEdited.ProductDescription : request.ProductDescription; 
            productToBeEdited.CategoryId = request.CategoryId.Equals(null) ? productToBeEdited.CategoryId : (int)request.CategoryId; 
            productToBeEdited.ProductPrice = request.ProductPrice.Equals(null) ? productToBeEdited.ProductPrice : (int)request.ProductPrice;
            productToBeEdited.ProductImageURL = request.ImageURL.IsNullOrEmpty() ? productToBeEdited.ProductImageURL : request.ImageURL;
            productToBeEdited.ProductPriceForSelling = request.ProductPriceForSelling.Equals(null) ? productToBeEdited.ProductPriceForSelling : (int)request.ProductPriceForSelling;

            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The changes has been successfully registered." });
        }

        [HttpDelete("DeleteProduct/{id}")]
        public ActionResult<Product> DeleteProduct(int id)
        {
            var product = dbContext.Products.FirstOrDefault(p => p.Id==id);

            if (product == null)
            {
                return BadRequest(new { Success = false, Message = "The product you are trying to delete does not exist." });
            }

            dbContext.Products.Remove(product);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The product has been successfully deleted." });
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
