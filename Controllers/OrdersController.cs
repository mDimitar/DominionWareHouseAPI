using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using DominionWarehouseAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
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

        [HttpGet("GetAllOrdersForBuyer")]
        [Authorize(Roles = "BUYER")]
        public IActionResult GetAllOrdersForBuyer()
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.FirstOrDefault(u => u.Username == username);

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
                    DateCreated = order.DateCreated.ToString("MM/dd/yyyy HH:mm"),
                    PhoneNumber = order.PhoneNumber,
                    CommentFromEmployee = order.CommentFromEmployee,
                })
                .Where(o => o.UserId == user.Id).ToList();

            if (orders.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false , Message = "No orders found in the database." });
            }

            return Ok(orders);
        }


        [HttpGet("GetAllOrders")]
        [Authorize(Roles = "ADMIN,EMPLOYEE,OWNER")]
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
                return BadRequest(new { Success = false, Message = "No orders found in the database." });
            }

            return Ok(orders);
        }

        [HttpPost("CreateOrder")]
        [Authorize(Roles = "BUYER")]
        public IActionResult CreateOrder(OrderDTO request)
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefault(u => u.Username == username);

            var prodsInShoppingCart = dbContext.ProductsInShoppingCarts
                 .Where(sc => sc.ShoppingCartId == user.ShoppingCartId).ToList();

            if (prodsInShoppingCart.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "There are no products in the shopping cart." });
            }

            var wh = dbContext.Warehouse.First();

            if (request.PhoneNumber.IsNullOrEmpty() || request.DeliveryAddress.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "Phone number and delivery address are required fields." });
            }

            var neworder = new Order
            {
                UserId = user.Id,
                Comment = request.Comment,
                TotalSum = user.ShoppingCart.TotalPrice,
                OrderStatus = OrderStatus.Processing,
                ShoppingCartId = user.ShoppingCart.Id,
                soldFromWarehouseId = wh.Id,
                PhoneNumber = request.PhoneNumber,
                DeliveryAddress = request.DeliveryAddress,
                soldFromEmployeeId = null //later to be assigned when finalizing order
            };

            dbContext.Orders.Add(neworder);
            dbContext.SaveChanges();


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

        [HttpPut("EditOrder/{id}")]
        [Authorize]
        public async Task<IActionResult> EditOrder(int id, OrderEditDTO request)
        {
            var order = dbContext.Orders.FirstOrDefault(o => o.Id == id);

            if(order == null)
            {
                return BadRequest(new {Success = false ,Message = "The requested order cannot be found"});
            }

            if (order.OrderStatus.Equals("Canceled"))
            {
                return BadRequest(new {Success = false, Message = "The order have been closed and cannot accept editing." });
            }

            order.PhoneNumber = request.PhoneNumber.IsNullOrEmpty() ? order.PhoneNumber : request.PhoneNumber;
            order.Comment = request.Comment.IsNullOrEmpty() ? order.Comment : request.Comment;

            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The changes have been succesfully registered." });

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

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = dbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Username == username);

            var order = dbContext.Orders.FirstOrDefault(o => o.Id == id);

            if(order == null)
            {
                return BadRequest(new { Success = false, Message = "Order not found" });
            }

            if (user.Role.RoleName.Equals("EMPLOYEE"))
            {
                order.OrderStatus = OrderStatus.Canceled;
                order.CommentFromEmployee = "The order has been canceled by an employee. Please contact us for more info.";
            }

            order.OrderStatus = OrderStatus.Canceled;

            dbContext.SaveChanges();

            return Ok(new { Success = true, Message = "The order has been canceled." });
        }
    }
}
