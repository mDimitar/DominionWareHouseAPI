using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Http;
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

            

            var neworder = new Order
            {
                UserId = request.UserId,

            };
            return null;
        }
    }
}
