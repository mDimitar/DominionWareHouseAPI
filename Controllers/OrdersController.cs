using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using DominionWarehouseAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

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
        [Authorize(Roles = "EMPLOYEE,OWNER")]
        public IActionResult GetAllOrders()
        {
            var orders = dbContext.Orders
                .Select(order => new
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    OrderStatus = order.OrderStatus,
                    ShoppingCartId = order.ShoppingCartId,
                    soldFromWarehouseId = order.soldFromWarehouseId,
                    soldFromEmployeeId = order.soldFromEmployeeId,
                    DateCreated = order.DateCreated
                })
                .ToList();

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
        [Authorize(Roles = "BUYER")]
        public IActionResult CreateOrder(OrderDTO request)
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return BadRequest("You must be authorized");
            }

            var neworder = new Order
            {
                UserId = user.Id,
                Comment = request.Comment,
                TotalSum = user.ShoppingCart.TotalPrice,
                OrderStatus = OrderStatus.Processing,
                ShoppingCartId = user.ShoppingCart.Id,
                soldFromWarehouseId = request.soldFromWarehouseId,
                soldFromEmployeeId = null //later to be assigned when finalizing order
            };

            dbContext.Orders.Add(neworder);
            dbContext.SaveChanges();

            var prodsInShoppingCart = dbContext.ProductsInShoppingCarts
                .Where(sc => sc.ShoppingCartId == user.ShoppingCartId).ToList();

            if(prodsInShoppingCart.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "There are no products in the shopping cart" });
            }

            foreach (var product in prodsInShoppingCart)
            {
                var prodInOrder = new OrderProduct
                {
                    OrderId = neworder.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                };
                dbContext.ProductsInOrder.Add(prodInOrder);
                dbContext.SaveChanges();
            }


            dbContext.SaveChanges();

            var rectToDelete = dbContext.ProductsInShoppingCarts.Where(sc => sc.ShoppingCartId == user.ShoppingCartId).ToList();

            dbContext.ProductsInShoppingCarts.RemoveRange(rectToDelete);
            user.ShoppingCart.TotalPrice = 0;
            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The order has been created succesfully." });
        }

        [HttpPut("FinalizeOrder/{id}")]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> FinalizeOrder(int id)
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefault(u => u.Username == username);

            var order = dbContext.Orders.SingleOrDefault(o => o.Id == id);

            if (order == null)
            {
                return BadRequest(new { Success = false, Message = "Order not found" });
            }

            var shoppingCartId = order.ShoppingCartId;

            var prodsInSc = dbContext.ProductsInShoppingCarts.Include(p => p.Product).
                Where(psc => psc.ShoppingCartId == shoppingCartId).ToList();

            var prodsInWarehouse = dbContext.Products.ToList();

            foreach (var prod in prodsInSc)
            {
                var product = dbContext.ProductsInWarehouses.SingleOrDefault(p => p.Product.Id == prod.ProductId);
                product.Quantity -= prod.Quantity;
                dbContext.SaveChanges();
            }

            order.OrderStatus = OrderStatus.Delivered;
            order.soldFromEmployeeId = user.Id;
            dbContext.SaveChanges();

            return Ok(new {Success = true, Message = "The order has been finalized."});
        }

        [HttpPut("CancelOrder/{id}")]
        [Authorize(Roles = "BUYER,EMPLOYEE")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = dbContext.Orders.FirstOrDefault(o => o.Id == id);

            if(order == null)
            {
                return BadRequest(new { Success = false, Message = "Order not found" });
            }

            order.OrderStatus = OrderStatus.Canceled;

            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The order has been finalized." });
        }
    }
}
