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
        public async Task<IActionResult> GetAllProductsInShoppingCart()
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = await dbContext.Users.Include(sc => sc.ShoppingCart).FirstOrDefaultAsync(u => u.Username == username);

            var query = from psc in dbContext.ProductsInShoppingCarts
                        .Where(sc => sc.ShoppingCartId == user.ShoppingCartId)
                        join product in dbContext.Products
                        on psc.ProductId equals product.Id
                        select new 
                        {
                            Id = product.Id,
                            ProductImage = product.ProductImageURL,
                            ProductName = product.ProductName,
                            ProductDescription = product.ProductDescription,
                            ProductPrice = product.ProductPriceForSelling,
                            Quantity = psc.Quantity
                        };

            var products = await query.ToListAsync();

            var response = new
            {
                Products = products,
                TotalPrice = user.ShoppingCart.TotalPrice,
                Quantity = query.Count(),
            };

            if (response.Products.IsNullOrEmpty())
            {
                return NotFound(new { Success = false, Message = "No products have been found in your shopping cart." });
            }

            return Ok(response);
        }

        [HttpPost("AddProductToShoppingCart")]
        public async Task<IActionResult> AddProductToShoppingCart(ProductsInShoppingCartDTO productDTO)
        {
            try
            {
                if(productDTO.Quantity < 0 || productDTO.Quantity == 0 || productDTO.Quantity == null)
                {
                    return BadRequest("Quantity of 0 or less cannot be added to the shopping cart.");
                }

                var product = await dbContext.ProductsInWarehouses.Include(p => p.Product).FirstOrDefaultAsync(p => p.ProductId == productDTO.ProductId);

                if(productDTO.Quantity > dbContext.ProductsInWarehouses.FirstOrDefault(p => p.ProductId == productDTO.ProductId).Quantity)
                {
                    return BadRequest(new { Success = false, Message = "The requested product is not available in that quantity" });
                }

                if (product == null)
                {
                    return BadRequest(new { Success = false, Message = "The requested product cannot be found" });
                }

                string username = User.FindFirstValue(ClaimTypes.Name);
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
                
                int userId = user.Id; 

                var shoppingCart = await dbContext.ShoppingCart
                    .Include(sc => sc.ProductShoppingCarts)
                    .FirstOrDefaultAsync(sc => sc.UserId == userId);

                var existingProduct = shoppingCart.ProductShoppingCarts
                    .FirstOrDefault(psc => psc.ProductId == productDTO.ProductId);

                if (existingProduct != null)
                {
                    shoppingCart.TotalPrice += productDTO.Quantity * existingProduct.Product.ProductPriceForSelling;

                    existingProduct.Quantity += productDTO.Quantity; 
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

                    shoppingCart.TotalPrice += productToBeAdded.Quantity * product.Product.ProductPriceForSelling;
                }

                await dbContext.SaveChangesAsync(CancellationToken.None);

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
            var user = await dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "The user cannot be found." });
            }

            var userProdsInSc = await dbContext.ProductsInShoppingCarts.
                Include(psc => psc.Product).
                Where(sc => sc.ProductId == ProductId && sc.ShoppingCartId == user.ShoppingCartId)
                .FirstOrDefaultAsync();


            dbContext.ProductsInShoppingCarts.Remove(userProdsInSc);
            user.ShoppingCart.TotalPrice -= userProdsInSc.Product.ProductPriceForSelling * userProdsInSc.Quantity;
            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "Product has been deleted successfully from the shopping cart." });
        }
    }
}
