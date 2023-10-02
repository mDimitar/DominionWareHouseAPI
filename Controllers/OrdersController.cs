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
        [Authorize(Roles = "BUYER,ADMIN,OWNER,EMPLOYEE")]
        public async Task<IActionResult> GetAllOrdersForBuyer()
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            var orders = await dbContext.Orders
                .Include(o => o.OrderProducts)
                .Select(order => new
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    OrderStatus = order.OrderStatus,
                    ShoppingCartId = order.ShoppingCartId,
                    Address = order.DeliveryAddress,
                    soldFromWarehouseId = order.soldFromWarehouseId,
                    soldFromEmployeeId = order.soldFromEmployeeId,
                    DateCreated = order.DateCreated.ToString("f"),
                    PhoneNumber = order.PhoneNumber,
                    CommentFromEmployee = order.CommentFromEmployee,
                })
                .Where(o => o.UserId == user.Id).ToListAsync();
           

            if (orders.IsNullOrEmpty())
            {
                return BadRequest(new {Success = false , Message = "No orders found in the database." });
            }

            return Ok(orders);
        }


        [HttpGet("GetAllOrders")]
        [Authorize(Roles = "ADMIN,EMPLOYEE,OWNER")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await dbContext.Orders
                .Select(order => new
                {
                    Id = order.Id,
                    Username = order.User.Username,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    OrderStatus = order.OrderStatus,
                    ShoppingCartId = order.ShoppingCartId,
                    soldFromWarehouseId = order.soldFromWarehouseId,
                    soldFromEmployeeId = order.soldFromEmployeeId,
                    DateCreated = order.DateCreated.ToString("f"),
                    OrderAddress = order.DeliveryAddress,
                    OrderCommentFromEmployee = order.CommentFromEmployee,
                    ProdsInOrder = order.OrderProducts.Select(op => new
                    {
                        ProductId = op.ProductId,
                        Quantity = op.Quantity,
                        ProductImage = op.Product.ProductImageURL,
                        ProductName = op.Product.ProductName,
                        ProductDescription = op.Product.ProductDescription,
                    }).ToList(),
                })
                .ToListAsync();

            if (orders.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "No orders found in the database." });
            }

            return Ok(orders);
        }

        [HttpPost("CreateOrder")]
        [Authorize(Roles = "BUYER,OWNER,EMPLOYEE,ADMIN")]
        public async Task<IActionResult> CreateOrder(OrderDTO request)
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = await dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefaultAsync(u => u.Username == username);

            var prodsInShoppingCart = await dbContext.ProductsInShoppingCarts
                 .Where(sc => sc.ShoppingCartId == user.ShoppingCartId).ToListAsync();

            if (prodsInShoppingCart.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "There are no products in the shopping cart." });
            }

            var wh = await dbContext.Warehouse.FirstAsync();

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

           await dbContext.SaveChangesAsync(CancellationToken.None);

            foreach (var product in prodsInShoppingCart)
            {
                var prodInOrder = new OrderProduct
                {
                    OrderId = neworder.Id,
                    ProductId = product.ProductId,
                    Quantity = product.Quantity,
                };
                dbContext.ProductsInOrder.Add(prodInOrder);
               await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            await dbContext.SaveChangesAsync(CancellationToken.None);

            var rectToDelete = await dbContext.ProductsInShoppingCarts.Where(sc => sc.ShoppingCartId == user.ShoppingCartId).ToListAsync();

            dbContext.ProductsInShoppingCarts.RemoveRange(rectToDelete);
            user.ShoppingCart.TotalPrice = 0;
            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The order has been created succesfully." });
        }

        [HttpPut("EditOrder/{id}")]
        [Authorize]
        public async Task<IActionResult> EditOrder(int id, OrderEditDTO request)
        {
            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);

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

            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The changes have been succesfully registered." });

        }

        [HttpPut("FinalizeOrder/{id}")]
        [Authorize(Roles = "EMPLOYEE,OWNER")]
        public async Task<IActionResult> FinalizeOrder(int id)
        {
            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = await dbContext.Users.Include(u => u.ShoppingCart).FirstOrDefaultAsync(u => u.Username == username);

            var order = await dbContext.Orders.SingleOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return BadRequest(new { Success = false, Message = "Order not found" });
            }

            var shoppingCartId = order.ShoppingCartId;

            var prodsInOrd = await dbContext.ProductsInOrder.Include(p => p.Product).
                Where(order => order.OrderId == id).ToListAsync();

            foreach (var prod in prodsInOrd)
            {
                var product = await dbContext.ProductsInWarehouses.SingleOrDefaultAsync(p => p.Product.Id == prod.ProductId);
                product.Quantity -= prod.Quantity;
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            order.OrderStatus = OrderStatus.Delivered;
            order.soldFromEmployeeId = user.Id;
            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new {Success = true, Message = "The order has been finalized."});
        }

        [HttpPut("CancelOrder/{id}")]
        [Authorize(Roles = "BUYER,EMPLOYEE,OWNER,ADMIN")]
        public async Task<IActionResult> CancelOrder(int id)
        {

            string username = User.FindFirstValue(ClaimTypes.Name);

            var user = await dbContext.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);

            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);

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
            order.Comment = "Canceled from buyer.";

            await dbContext.SaveChangesAsync(CancellationToken.None);

            return Ok(new { Success = true, Message = "The order has been canceled." });
        }
    }
}
