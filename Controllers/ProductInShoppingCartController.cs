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
        public IActionResult GetAllProductsInShoppingCart()
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(sc => sc.ShoppingCart).FirstOrDefault(u => u.Username == username);

            var query = from psc in dbContext.ProductsInShoppingCarts
                        join product in dbContext.Products
                        on psc.ProductId equals product.Id
                        select new ShoppingCartInfoReturn
                        {
                            Id = product.Id,
                            ProductName = product.ProductName,
                            ProductDescription = product.ProductDescription,
                            Quantity = psc.Quantity
                        };

            if (query.IsNullOrEmpty())
            {
                return new JsonResult(new
                {
                    Success = false,
                    Message = "No products has been found in your shopping cart."
                });
            }

            return Ok(query.ToList());
        }

        [HttpPost("AddProductToShoppingCart")]
        public async Task<IActionResult> AddProductToShoppingCart(ProductsInShoppingCartDTO productDTO)
        {
            try
            {
                var product = await dbContext.Products.FindAsync(productDTO.ProductId);
                if (product == null)
                {
                    return NotFound("Product not found.");
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
                    
                    if(existingProduct.Quantity > productDTO.Quantity)
                    {
                        existingProduct.Quantity = productDTO.Quantity;
                        var fetchprod = dbContext.Products.FirstOrDefault(p => p.Id == existingProduct.ProductId);
                        shoppingCart.TotalPrice = fetchprod.ProductPrice * productDTO.Quantity;
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        existingProduct.Quantity += productDTO.Quantity;
                        var prod = dbContext.Products.FirstOrDefault(p => p.Id == existingProduct.ProductId);
                        shoppingCart.TotalPrice = prod.ProductPrice * productDTO.Quantity;
                    }
                }
                else
                {
                    
                    var productInCart = new ProductsInShoppingCart
                    {
                        ProductId = productDTO.ProductId,
                        Quantity = productDTO.Quantity
                    };
                    shoppingCart.ProductShoppingCarts.Add(productInCart);
                    
                    var addedProduct = dbContext.Products.FirstOrDefault(p => p.Id == productDTO.ProductId);

                    shoppingCart.TotalPrice = addedProduct.ProductPrice * productDTO.Quantity;


                }


                await dbContext.SaveChangesAsync();

                var successResponse = new
                {
                    Success = true,
                    Message = "Product has been added successfully to the shopping cart."
                };

                return new JsonResult(successResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
            }
        }
    }
}
