﻿using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("GetProductsFromWarehouse/{id}")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE,BUYER")]
        public IActionResult GetAllProductsFromWarehouse(int id)
        {
            var prodsinwh = dbContext.ProductsInWarehouses.Include(piw => piw.Product).Where(p => p.WarehouseId == id).ToList();

            return Ok(prodsinwh);
        }

        [HttpPost("AddProductToWareHouse")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult AddProductToWareHouse(ProductWareHouseDTO request)
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var prodToBeAdded = 
                dbContext.ProductsInWarehouses.FirstOrDefault(p => p.ProductId == request.ProductId);

            if (prodToBeAdded == null)
            {
                var prodInWarehouse = new ProductsInWarehouse
                {
                    WarehouseId = request.WarehouseId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Received = username,
                    ProductPriceForSelling = request.ProductPriceForSelling
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

        [HttpPost("EditProductInWarehouse/")]
        [Authorize(Roles = "ADMIN,OWNER,EMPLOYEE")]
        public IActionResult EditProductInWarehouse(ProductWarehouseDTOForEdit request)
        {
            var prodToBeEdited =
                dbContext.ProductsInWarehouses.
                Include(p => p.Product).FirstOrDefault(p => p.ProductId == request.ProductId);

            if (prodToBeEdited == null)
            {
                return BadRequest("Product Not Found");
            }

            prodToBeEdited.Quantity = request.Quantity;
            prodToBeEdited.ProductPriceForSelling = request.ProductPriceForSelling;
            prodToBeEdited.Product.ProductName = request.ProductName;
            prodToBeEdited.Product.ProductImageURL = request.ProductImageUrl;
            prodToBeEdited.Product.ProductName = request.ProductName;
            prodToBeEdited.Product.ProductDescription = request.ProductDescription;

            dbContext.SaveChanges();

            return Ok("success");
        }

    }
}
