using DominionWarehouseAPI.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DominionWarehouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "OWNER,ADMIN")]
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
        public IActionResult ViewReceivedGoods()
        {
            var log = dbContext.ReceivedGoodsBy.Include(p => p.Product).Include(u => u.User)
                .Select(log => new
                {
                    Product = log.Product,
                    ProductQuantity = log.ProductQuantity,
                    User = log.User,
                    ReceivedOn = log.AcceptanceDate.ToString("f"),
                })
                .ToList();

            return Ok(log);
        }
    }
}
