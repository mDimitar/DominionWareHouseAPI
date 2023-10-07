using DominionWarehouseAPI.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Enums;
using System.Globalization;
using System;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,OWNER")]
    public class AdminPanelController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public AdminPanelController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("GetAllOrdersCountByStatus")]
        public async Task<IActionResult> GetAllOrdersCountByStatus()
        {

            var totalOrders = await dbContext.Orders.ToListAsync();

            if(totalOrders.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "No orders have been found in the database" });
            }

            var DeliveredOrders = await dbContext.Orders.Where(order => order.OrderStatus == OrderStatus.Delivered)
                .Select(order => new
                {
                    Id = order.Id,
                    User = order.User.Username,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    Date = order.DateCreated.ToString("f"),
                    SoldFromEmployee = dbContext.Users.FirstOrDefault(u => u.Id == order.soldFromEmployeeId).Username
                }).ToListAsync();

            var ProcessingOrders = await dbContext.Orders.Where(order => order.OrderStatus == OrderStatus.Processing)
                .Select(order => new
                {
                    Id = order.Id,
                    User = order.User.Username,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    Date = order.DateCreated.ToString("f"),
                    SoldFromEmployee = dbContext.Users.FirstOrDefault(u => u.Id == order.soldFromEmployeeId).Username
                }).ToListAsync();

            var CanceledOrders = await dbContext.Orders.Where(order => order.OrderStatus == OrderStatus.Canceled)
                .Select(order => new
                {
                    Id = order.Id,
                    User = order.User.Username,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    Date = order.DateCreated.ToString("f"),
                    SoldFromEmployee = dbContext.Users.FirstOrDefault(u => u.Id == order.soldFromEmployeeId).Username
                }).ToListAsync();

            return Ok(new { Delivered = DeliveredOrders.Count, Processing = ProcessingOrders.Count, Canceled = CanceledOrders.Count});
        }


        [HttpGet("GetAllFilteredFinishedOrdersByDate")]
        public async Task<IActionResult> GetAllFilteredFinishedOrdersByDate(DateOnly startDate, DateOnly endDate)
        {

            DateTime startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
            DateTime endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            endDateTime = endDateTime.AddDays(1);

            var orders = await dbContext.Orders
                .Where(order => order.OrderStatus == OrderStatus.Delivered)
                .Where(order => order.DateCreated >= startDateTime && order.DateCreated < endDateTime)
                .Select(order => new
                {
                    Id = order.Id,
                    User = order.User.Username,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    DeliveryAddress = order.DeliveryAddress,
                    Date = order.DateCreated.ToString("M/d/yyyy h:mm:ss tt"),
                    SoldFromEmployee = dbContext.Users.FirstOrDefault(u => u.Id == order.soldFromEmployeeId).Username
                }).ToListAsync();

            if (orders.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "No orders have been found between the dates." });
            }
            return Ok(orders);
        }

        [HttpGet("CalculateProfitFromDateToDate")]
        public async Task<IActionResult> CalculateProfitFromDateToDate(DateOnly startDate, DateOnly endDate)
        {

            DateTime startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
            DateTime endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            endDateTime = endDateTime.AddDays(1);

            var orders = await dbContext.Orders
                .Include(pio => pio.OrderProducts)
                .Where(order => order.OrderStatus == OrderStatus.Delivered)
                .Where(order => order.DateCreated >= startDateTime && order.DateCreated < endDateTime)
                .Select(order => new
                {
                    Id = order.Id,
                    User = order.User.Username,
                    TotalSum = order.TotalSum,
                    Comment = order.Comment,
                    Date = order.DateCreated.ToString("f"),
                    SoldFromEmployee = dbContext.Users.FirstOrDefault(u => u.Id == order.soldFromEmployeeId).Username
                }).ToListAsync();

            if (orders.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "No orders have been found between the dates." });
            }

            int totalSum = 0;

            foreach (var order in orders)
            {
                var prodsInOrder = await dbContext.ProductsInOrder.Include(p => p.Product).Where(pio => pio.OrderId == order.Id).ToListAsync();

                foreach (var prods in prodsInOrder)
                {
                    totalSum += (prods.Quantity * prods.Product.ProductPriceForSelling) - (prods.Quantity * prods.Product.ProductPrice);
                }
            }

            var returnedObject = new
            {
                Orders = orders,
                TotalSum = totalSum,
            };

            return Ok(returnedObject);
        }

            [HttpGet("GetMonthlyOrderCount")]
            public async Task<IActionResult> GetMonthlyOrderCount()
            {
                var monthlyOrderCounts = Enumerable.Range(1, 12)
                    .Select(month => new
                    {
                        Month = DateTimeFormatInfo.CurrentInfo.GetMonthName(month),
                        OrderCount = dbContext.Orders
                            .Where(o => o.DateCreated.Year == DateTime.Now.Year && o.DateCreated.Month == month)
                            .Where(o => o.OrderStatus == OrderStatus.Delivered)
                            .Count()
                    })
                    .ToList();

            var result = await Task.FromResult(monthlyOrderCounts);

            return Ok(result);
            }
    }
}
