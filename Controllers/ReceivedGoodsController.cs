using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuppeteerSharp;
using System.Security.Claims;
using System.Text.Json;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceivedGoodsController : ControllerBase
    {

        private readonly AppDbContext dbContext;
        private readonly IConfiguration _configuration;

        public ReceivedGoodsController(AppDbContext context, IConfiguration configuration)
        {
            this.dbContext = context;
            _configuration = configuration;
        }

        [HttpGet("ViewReceivedGoods")]
        [Authorize(Roles = "EMPLOYEE,OWNER,ADMIN")]
        public async Task<IActionResult> ViewReceivedGoods()
        {
            var log = await dbContext.ReceivedGoodsBy.Include(p => p.Product).Include(u => u.User)
                .Select(log => new
                {
                    Product = log.Product,
                    ProductQuantity = log.ProductQuantity,
                    User = log.User,
                    ReceivedOn = log.AcceptanceDate.ToString("f"),
                })
                .ToListAsync();

            return Ok(log);
        }

        [HttpGet("GenerateReceivedGoodsPDF")]
        [Authorize(Roles = "EMPLOYEE,OWNER")]
        public async Task<IActionResult> GenerateReceivedGoodsPDF()
        {
            string username = User.FindFirstValue(ClaimTypes.Name);
            var user = await dbContext.Users.Include(sc => sc.ShoppingCart).FirstOrDefaultAsync(u => u.Username == username);

            var receivedGoods = await dbContext.ReceivedGoodsBy
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.UserId == user.Id)
                .Where(r => r.AcceptanceDate.Date == DateTime.Now.Date)
                .ToListAsync();

            if (receivedGoods.IsNullOrEmpty())
            {
                return BadRequest(new { Success = false, Message = "No logs were found right now." });
            }

            var options = new LaunchOptions
            {
                Headless = true,
            };
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(options);
            using var page = await browser.NewPageAsync();
            using var ms = new MemoryStream();

            var url = Url.ActionLink("ReceivedGoodsPDF", "PDFGenerator", new { model = JsonSerializer.Serialize(user.Id) });
            await page.GoToAsync(url);

            var pdfStream = await page.PdfDataAsync();

            return File(pdfStream, "application/pdf", "Report_" + DateTime.UtcNow.Date.ToString("d"));
        }
    }
}
