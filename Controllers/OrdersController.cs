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
    [Authorize(Roles = "BUYER")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public OrdersController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }


        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            var orders = dbContext.Orders.Include(u => u.User).ToList();

            if (orders.IsNullOrEmpty())
            {
                var failedResponse = new
                {
                    Success = false,
                    Message = "No orders found in the database."
                };
                return BadRequest(failedResponse);
            }

            return Ok(orders);
        }

        [HttpPost("CreateOrder")]
        public IActionResult CreateOrder(OrderDTO request)
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefault(u => u.Username == username);

            if(user == null)
            {
                return BadRequest("not authorized");
            }

            var neworder = new Order
            {
                UserId = user.Id,
                Comment = request.Comment,
                TotalSum = user.ShoppingCart.TotalPrice,
                ShoppingCartId = user.ShoppingCart.Id,
                soldFromWarehouseId = request.soldFromWarehouseId,
                soldFromEmployeeId = null //later to be assigned when finalizing order
            };

            dbContext.Orders.Add(neworder);
            dbContext.SaveChanges();


            return Ok("success write");
        }

        [HttpPut("FinalizeOrder/{id}")]
        public async Task<IActionResult> FinalizeOrder(int id)
        {

            var order = dbContext.Orders.SingleOrDefault(o => o.Id == id);

            if(order == null)
            {
                return BadRequest("Order not found");
            }

            var shoppingCartId = order.ShoppingCartId;

            var prodsInSc = dbContext.ProductsInShoppingCarts.Include(p => p.Product).
                Where(psc => psc.ShoppingCartId == shoppingCartId).ToList();

           

            foreach(var prod in prodsInSc)
            {

            }

            return null;


            /*var rectToDelete = dbContext.ProductsInShoppingCarts.Where(sc => sc.ShoppingCartId == user.ShoppingCartId).ToList();

            dbContext.ProductsInShoppingCarts.RemoveRange(rectToDelete);
            dbContext.SaveChanges();*/
        }
    }
}
