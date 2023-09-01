﻿using DominionWarehouseAPI.Database;
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
    [Authorize]
    public class ProductInShoppingCartController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public ProductInShoppingCartController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("GetAllProductsInShoppingCart")]
        public IActionResult GetAllProductsInShoppingCart()
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(sc => sc.ShoppingCart).FirstOrDefault(u => u.Username == username);

            var query = from psc in dbContext.ProductsInShoppingCarts
                        .Where(sc => sc.ShoppingCartId == user.ShoppingCartId)
                        join product in dbContext.Products
                        on psc.ProductId equals product.Id
                        select new ShoppingCartInfoReturn
                        {
                            Id = product.Id,
                            ProductName = product.ProductName,
                            ProductDescription = product.ProductDescription,
                            Quantity = psc.Quantity
                        };

            var response = new
            {
                Products = query,
                TotalPrice = user.ShoppingCart.TotalPrice
            };

            if (response.Products.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "No products have been found in your shopping cart." });
            }

            return Ok(response);
        }

        [HttpPost("AddProductToShoppingCart")]
        public async Task<IActionResult> AddProductToShoppingCart(ProductsInShoppingCartDTO productDTO)
        {
            try
            {
                if(productDTO.Quantity < 0 || productDTO.Quantity == 0)
                {
                    return BadRequest("Quantity of 0 or less cannot be added to the shopping cart.");
                }

                var product = await dbContext.Products.FindAsync(productDTO.ProductId);

                if (product == null)
                {
                    return BadRequest(new { Success = false, Message = "The requested product cannot be found" });
                }

                string username = User.FindFirstValue(ClaimTypes.Name);
                var user = dbContext.Users.FirstOrDefault(u => u.Username==username);
                
                int userId = user.Id; 

                var shoppingCart = await dbContext.ShoppingCart
                    .Include(sc => sc.ProductShoppingCarts)
                    .FirstOrDefaultAsync(sc => sc.UserId == userId);

                var existingProduct = shoppingCart.ProductShoppingCarts
                    .FirstOrDefault(psc => psc.ProductId == productDTO.ProductId);

                if (existingProduct != null)
                {
                    shoppingCart.TotalPrice = shoppingCart.TotalPrice - 
                        (existingProduct.Quantity * product.ProductPriceForSelling) +
                        (productDTO.Quantity * product.ProductPriceForSelling);

                    existingProduct.Quantity = productDTO.Quantity;
                }
                else
                {
                    var productToBeAdded = new ProductsInShoppingCart
                    {
                        ProductId = productDTO.ProductId,
                        ShoppingCartId = shoppingCart.Id,
                        Quantity = productDTO.Quantity,
                    };
                    
                    shoppingCart.ProductShoppingCarts.Add(productToBeAdded);

                    shoppingCart.TotalPrice += productToBeAdded.Quantity * product.ProductPriceForSelling;
                }

                await dbContext.SaveChangesAsync();

                return Ok(new {Success = true, Message = "Product has been added successfully to the shopping cart." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
            }
        }

        [HttpDelete("DeleteProductFromShoppingCart")]
        public async Task<IActionResult> DeleteProductFromShoppingCart(int ProductId)
        {

            string username = User.FindFirstValue(ClaimTypes.Name);
            var user = dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "The user cannot be found." });
            }

            var userProdsInSc = await dbContext.ProductsInShoppingCarts.
                Where(sc => sc.ProductId == ProductId && sc.ShoppingCartId == user.ShoppingCartId)
                .FirstOrDefaultAsync();

            dbContext.ProductsInShoppingCarts.Remove(userProdsInSc);
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "Product has been deleted successfully from the shopping cart." });
        }
    }
}
