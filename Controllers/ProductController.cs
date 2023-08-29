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
                var failedResponse = new
                {
                    Success = false,
                    Message = "No products can be found in the database.",
                };
                return new JsonResult(failedResponse);
            }

            return Ok(products);
        }

        [HttpPost("RegisterProduct")]
        public ActionResult<Product> RegisterProduct(ProductDTO request)
        {
            var product = dbContext.Products.Any(p => p.ProductName == request.ProductName);

            if (product)
            {
                var failedResponse = new
                {
                    Success = false,
                    Message = "The product already exists. Please enter a new name.",
                };
                return new JsonResult(failedResponse);
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

            var successfullResponse = new
            {
                Success = true,
                Message = "The product has been successfully registered.",
            };

            return new JsonResult(successfullResponse);
        }


        [HttpPut("EditProduct/{id}")]
        public ActionResult<Product> EditProducts(int id,[FromBody] ProductDTO request)
        {
            var product = dbContext.Products.Any(p => p.Id == id);

            if (!product)
            {
                var failedResponse = new
                {
                    Success = false,
                    Message = "The product does not exists",
                };
                return new JsonResult(failedResponse);
            }

            var productToBeEdited = dbContext.Products.SingleOrDefault(p => p.Id == id);

            productToBeEdited.ProductName = request.ProductName;
            productToBeEdited.ProductDescription = request.ProductDescription; 
            productToBeEdited.ProductPrice = request.ProductPrice;
            productToBeEdited.ProductImageURL = request.ImageURL;
            productToBeEdited.ProductPriceForSelling= request.ProductPriceForSelling;

            dbContext.SaveChanges();

            var successEditResponse = new
            {
                Success = true,
                Message = "The changes has been successfully registered.",
            };

            return new JsonResult(successEditResponse);
        }

        [HttpDelete("DeleteProduct/{id}")]
        public ActionResult<Product> DeleteProduct(int id)
        {
            var product = dbContext.Products.FirstOrDefault(p => p.Id==id);

            if (product == null)
            {
                var failedResponse = new
                {
                    Success = false,
                    Message = "The product you are trying to delete does not exist."
                };
                return new JsonResult(failedResponse);
            }

            dbContext.Products.Remove(product);
            dbContext.SaveChanges();

            var successEditResponse = new
            {
                Success = true,
                Message = "The product has been successfully deleted.",
            };

            return new JsonResult(successEditResponse);
        }
    }
}
