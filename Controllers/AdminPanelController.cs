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
using Microsoft.Identity.Client;

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

        [HttpGet("GetDailyStats")]
        public async Task<IActionResult> GetDailyStats()
        {

            var orders = await dbContext.Orders
                .Include(order => order.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(order => order.DateCreated.Date == DateTime.Now.Date)
                .ToListAsync();

            int profit = 0;
            int dailytotal = 0;

            foreach (var order in orders)
            {
                foreach (var orderProduct in order.OrderProducts)
                {
                    profit += (orderProduct.Product.ProductPriceForSelling - orderProduct.Product.ProductPrice) * orderProduct.Quantity;
                    dailytotal += orderProduct.Product.ProductPriceForSelling * orderProduct.Quantity;
                }
            }

            var response = new
            {
                Date = DateTime.Now.Date.ToString("D"),
                Profit = profit,
                DailyTotal = dailytotal,
            };

            return Ok(response);
        }

        [HttpGet("GetMonthlyStats")]
        public async Task<IActionResult> GetMonthlyStats()
        {
            DateTime currentDate = DateTime.Now;

            DateTime startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var orders = await dbContext.Orders
                .Include(order => order.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(order => order.DateCreated >= startDate && order.DateCreated <= endDate)
                .ToListAsync();

            int profit = 0;
            int monthlytotal = 0;

            foreach (var order in orders)
            {
                foreach (var orderProduct in order.OrderProducts)
                {
                    profit += (orderProduct.Product.ProductPriceForSelling - orderProduct.Product.ProductPrice) * orderProduct.Quantity;
                    monthlytotal += orderProduct.Product.ProductPriceForSelling * orderProduct.Quantity;
                }
            }

            var response = new
            {
                Date = DateTime.Now.Date.ToString("MMMM,yyyy"),
                Profit = profit,
                MonthlyTotal = monthlytotal,
            };

            return Ok(response);
        }

        [HttpGet("GetYearlyStats")]
        public async Task<IActionResult> GetYearlyStats()
        {

            var orders = await dbContext.Orders
                .Include(order => order.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(order => order.DateCreated.Year == DateTime.Now.Year)
                .ToListAsync();

            int profit = 0;
            int yearlytotal = 0;

            foreach (var order in orders)
            {
                foreach (var orderProduct in order.OrderProducts)
                {
                    profit += (orderProduct.Product.ProductPriceForSelling - orderProduct.Product.ProductPrice) * orderProduct.Quantity;
                    yearlytotal += orderProduct.Product.ProductPriceForSelling * orderProduct.Quantity;
                }
            }

            var response = new
            {
                Date = DateTime.Now.Date.Year.ToString(),
                Profit = profit,
                YearlyTotal = yearlytotal,
            };

            return Ok(response);
        }

        [HttpGet("CompareCurrentWithMonthBefore")]
        public async Task<IActionResult> CompareCurrentWithMonthBefore()
        {
            int currentMonthTotalPrice = await dbContext.Orders
                .Where(order => order.DateCreated.Month == DateTime.Now.Month && order.DateCreated.Year == DateTime.Now.Year)
                .SumAsync(order => order.TotalSum);

            int previousMonthTotalPrice = await dbContext.Orders
                .Where(order => order.DateCreated.Month == DateTime.Now.AddMonths(-1).Month && order.DateCreated.Year == DateTime.Now.AddMonths(-1).Year)
                .SumAsync(order => order.TotalSum);

            int currentMonthTotal = 0;
            int currentMonthProfit = 0;

            int previousMonthTotal = 0;
            int previousMonthProfit = 0;

            var currentMonthOrders = await dbContext.Orders
                .Include(order => order.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(order => order.DateCreated.Month == DateTime.Now.Month && order.DateCreated.Year <= DateTime.Now.Year)
                .ToListAsync();

            var previousMonthOrders = await dbContext.Orders
                .Include(order => order.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(order => order.DateCreated.Month == DateTime.Now.AddMonths(-1).Month && order.DateCreated.Year <= DateTime.Now.Year)
                .ToListAsync();

            foreach (var order in currentMonthOrders)
            {
                foreach (var orderProduct in order.OrderProducts)
                {
                    currentMonthProfit += (orderProduct.Product.ProductPriceForSelling - orderProduct.Product.ProductPrice) * orderProduct.Quantity;
                    currentMonthTotal += orderProduct.Product.ProductPriceForSelling * orderProduct.Quantity;
                }
            }

            foreach (var order in previousMonthOrders)
            {
                foreach (var orderProduct in order.OrderProducts)
                {
                    previousMonthProfit += (orderProduct.Product.ProductPriceForSelling - orderProduct.Product.ProductPrice) * orderProduct.Quantity;
                    previousMonthTotal += orderProduct.Product.ProductPriceForSelling * orderProduct.Quantity;
                }
            }

            return Ok(new {
                CurrentMonth = DateTime.Now.Date.ToString("MMMM,yyyy"),
                PreviousMonth = DateTime.Now.AddMonths(-1).ToString("MMMM,yyyy"),
                PreviousMonthTotal = previousMonthTotal,
                PreviousMonthProfit = previousMonthProfit,
                CurrentMonthTotal = currentMonthTotal,
                CurrentMonthProfit = currentMonthProfit
            }); ;
        }

    }
}
