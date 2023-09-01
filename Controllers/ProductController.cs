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
            var products = dbContext.Products.ToList();

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

            if (product)
            {
                return BadRequest(new { Success = false, Message = "The product already exists. Please enter a new name." });
            }

            var newProduct = new Product
            {
                ProductName = request.ProductName,
                ProductDescription = request.ProductDescription,
                CategoryId = request.CategoryId,
                ProductPrice = request.ProductPrice,
                ProductImageURL = request.ImageURL,
                ProductPriceForSelling = request.ProductPriceForSelling,
            };

            dbContext.Products.Add(newProduct);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The product has been successfully registered." });
        }


        [HttpPut("EditProduct/{id}")]
        public ActionResult<Product> EditProducts(int id,[FromBody] ProductDTO request)
        {
            var product = dbContext.Products.Any(p => p.Id == id);

            if (!product)
            {
                return BadRequest(new { Success = false, Message = "The product does not exists" });
            }

            var productToBeEdited = dbContext.Products.SingleOrDefault(p => p.Id == id);

            productToBeEdited.ProductName = request.ProductName;
            productToBeEdited.ProductDescription = request.ProductDescription; 
            productToBeEdited.ProductPrice = request.ProductPrice;
            productToBeEdited.ProductImageURL = request.ImageURL;
            productToBeEdited.ProductPriceForSelling= request.ProductPriceForSelling;

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
    }
}
