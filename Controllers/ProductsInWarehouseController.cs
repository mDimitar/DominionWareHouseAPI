using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="ADMIN,OWNER,EMPLOYEE")]
    public class ProductsInWarehouseController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public ProductsInWarehouseController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("GetProductsFromWarehouse/{id}")]
        public IActionResult GetAllProductsFromWarehouse(int id)
        {
            var prodsinwh = dbContext.ProductsInWarehouses.Where(p => p.WarehouseId == id).ToList();

            return Ok(prodsinwh);
        }

        [HttpPost("AddProductToWareHouse")]
        public IActionResult AddProductToWareHouse(ProductWareHouseDTO request)
        {
            var user = User.FindFirstValue(ClaimTypes.Name);

            var prodToBeAdded = 
                dbContext.ProductsInWarehouses.FirstOrDefault(p => p.ProductId == request.ProductId);

            if (prodToBeAdded == null)
            {
                var prodInWarehouse = new ProductsInWarehouse
                {
                    WarehouseId = request.WarehouseId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Received = user
                };
                dbContext.ProductsInWarehouses.Add(prodInWarehouse);
                dbContext.SaveChanges();
            }
            else
            {
                prodToBeAdded.Quantity += request.Quantity;
                dbContext.SaveChanges();
            }
            return Ok("success");
        }

        [HttpPost("EditProductInWarehouse/{id}")]
        public IActionResult EditProductInWarehouse(int id,ProductWareHouseDTO request)
        {
            var prodToBeAdded =
                dbContext.ProductsInWarehouses.FirstOrDefault(p => p.ProductId == request.ProductId);

            
            return Ok("success");
        }

    }
}
