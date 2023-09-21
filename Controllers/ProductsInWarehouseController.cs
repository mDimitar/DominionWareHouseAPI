using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsInWarehouseController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public ProductsInWarehouseController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("FilterProductsFromWarehouseByCategory/{id}")]
        public async Task<IActionResult> FilterProductsByIncomingCategory(int id)
        {
            var query = dbContext.ProductsInWarehouses
                .Include(wp => wp.Product)
                .Where(wp => wp.Product.Category.Id == id)
                .Select(wp => new
                {
                    wp.Product.Id,
                    wp.Product.ProductName,
                    wp.Product.ProductDescription,
                    wp.Product.Category.CategoryName,
                    wp.Product.ProductPriceForSelling,
                    wp.Product.ProductImageURL,
                });

            var products = await query.ToListAsync();

            return Ok(products);
        }

        [HttpGet("FilterProductsFromWarehouseByString")]
        public async Task<IActionResult> FilterProductsByIncomingString(string? q) 
        {
            if (q.IsNullOrEmpty())
            {
                return Ok(dbContext.ProductsInWarehouses.ToList());
            }

            var query = dbContext.ProductsInWarehouses
                .Include(wp => wp.Product)
                .Where(wp => wp.Product.ProductName.Contains(q) ||
                             wp.Product.ProductDescription.Contains(q))
                .Select(wp => new
                {
                    wp.Product.Id,
                    wp.Product.ProductName,
                    wp.Product.ProductDescription,
                    wp.Product.Category.CategoryName,
                    wp.Product.ProductPriceForSelling,
                    wp.Product.ProductImageURL,
                });

            var products = await query.ToListAsync();

            return Ok(products);
        }

        [HttpGet("GetProductsFromWarehouse")]
        public IActionResult GetAllProductsFromWarehouse()
        {

            var wh = dbContext.Warehouse.FirstOrDefault();

            if(wh == null)
            {
                return BadRequest(new { Success = false, Message = "No warehouse found in the database." });
            }

            var prodsinwh = dbContext.ProductsInWarehouses.
                Where(piw => piw.WarehouseId == wh.Id && piw.Quantity > 0).
                Include(piw => piw.Product).
                Select(p => new
                {
                    Id = p.ProductId,
                    ProductName = p.Product.ProductName,
                    ProductDescription = p.Product.ProductDescription,
                    ProductPrice = p.Product.ProductPriceForSelling,
                    ProductImageUrl = p.Product.ProductImageURL,
                    ProductQuantity = p.Quantity,
                    ReceivedBy = p.Received
                }).ToList();

            if(!prodsinwh.Any())
            {
                return BadRequest(new { Success = false, Message = "There are no products in the warehouse at the moment" });
            }

            return Ok(prodsinwh);
        }

        [HttpPost("AddProductToWareHouse")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult AddProductToWareHouse(ProductWareHouseDTO request)
        {

            if ((request.Quantity.Equals(null) || request.Quantity <= 0) && request.WarehouseId.Equals(null))
            {
                return BadRequest(new { Success = false, Message = "Both fields are required" });
            }

            if (request.Quantity.Equals(null) || request.Quantity <= 0)
            {
                return BadRequest(new { Success = false, Message = "Invalid quantity" });
            }

            if (request.WarehouseId.Equals(null))
            {
                return BadRequest(new { Success = false, Message = "Warehouse must be selected" });
            }

            string username = User.FindFirstValue(ClaimTypes.Name);

            var prodToBeAdded = 
                dbContext.ProductsInWarehouses.FirstOrDefault(p => p.ProductId == request.ProductId);

            if (prodToBeAdded == null)
            {
                var prodInWarehouse = new ProductsInWarehouse
                {
                    WarehouseId = (int)request.WarehouseId,
                    ProductId = (int)request.ProductId,
                    Quantity = (int)request.Quantity,
                    Received = username,
                };
                dbContext.ProductsInWarehouses.Add(prodInWarehouse);
                dbContext.SaveChanges();
            }
            else
            {
                prodToBeAdded.Quantity += (int)request.Quantity;
                dbContext.SaveChanges();
            }
            return Ok(new {Success =  true, Message = "The product has been successfully added to the warehouse."});
        }

        [HttpPost("EditProductInWarehouse")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult EditProductInWarehouse(ProductWarehouseDTOForEdit request)
        {
            var prodToBeEdited =
                dbContext.ProductsInWarehouses.
                Include(p => p.Product).FirstOrDefault(p => p.ProductId == request.ProductId);

            if (prodToBeEdited == null)
            {
                return BadRequest(new { Success = false, Message = "The product does not exist in the warehouse." });
            }

            prodToBeEdited.Quantity = request.Quantity;
            prodToBeEdited.Product.ProductName = request.ProductName;
            prodToBeEdited.Product.ProductImageURL = request.ProductImageUrl;
            prodToBeEdited.Product.ProductName = request.ProductName;
            prodToBeEdited.Product.ProductDescription = request.ProductDescription;

            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The changes have been successfully registered" });
        }

    }
}
