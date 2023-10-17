using DominionWarehouseAPI.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace DominionWarehouseAPI.Controllers
{
    public class PDFGeneratorController : Controller
    {
        private readonly AppDbContext dbContext;

        public PDFGeneratorController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("OrderPDF")]
        public async Task<IActionResult> OrderPDF(string model)
        {
            var orderId = JsonSerializer.Deserialize<int>(model);
            var order = await dbContext.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(o => o.Product)
                .ThenInclude(o => o.Category)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            ViewBag.order = order;
            return View();
        }

        [HttpGet("ReceivedGoods")]
        public async Task<IActionResult> ReceivedGoodsPDF(string model)
        {
            var userId = JsonSerializer.Deserialize<int>(model);
            var receivedGoods = await dbContext.ReceivedGoodsBy
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .Where(r => r.AcceptanceDate.Date == DateTime.Now.Date)
                .ToListAsync();
            ViewBag.receivedGoods = receivedGoods;
            return View();
        }
    }
}
