using DominionWarehouseAPI.Controllers;
using DominionWarehouseAPI.Database;
using DominionWarehouseAPI.Models;
using DominionWarehouseAPI.Models.Data_Transfer_Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DominionWarehouseAPI.Tests
{
    public class MoqTests
    {
        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithListOfUsers()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase1")
                .Options;

            using var context = new AppDbContext(options);

            context.Users.Add(new User
            {
                Id = 1,
                Username = "user1",
                WorksAtWarehouse = 1,
                RoleId = 1,
                Role = new Roles { RoleName = "ADMIN" },
                ShoppingCartId = 1,
                ShoppingCart = new ShoppingCart(),
                Orders = new List<Order>(),
                ReceivedGoods = new List<ReceivedGoodsBy>()
            });
            context.SaveChanges();

            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new UserController(context, mockConfiguration.Object);

            var result = await controller.GetAllUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            var userList = returnValue.Cast<object>().ToList();
            Assert.Equal(context.Users.Count(), userList.Count);
        }

        [Fact]
        public async Task RegisterRole_ReturnsOkResult_WhenRoleIsAddedSuccessfully()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase2")
                .Options;

            using var context = new AppDbContext(options);

            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new RolesController(context, mockConfiguration.Object);

            var roleDto = new RoleDTO { RoleName = "test123123" };

            var result = await controller.RegisterRole(roleDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<object>(okResult.Value);
            dynamic resultData = returnValue;
            Assert.True(resultData.Success);
            Assert.Equal("The role has been added successfully.", resultData.Message);
        }

        [Fact]
        public async Task EditProducts_ReturnsOkResult_WhenProductIsEditedSuccessfully()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase3")
                .Options;

            using var context = new AppDbContext(options);

            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ProductController(context, mockConfiguration.Object);

            var productToEdit = new Product
            {
                Id = 1,
                ProductName = "test123123",
                ProductDescription = "test123123",
                CategoryId = 1,
                ProductPrice = 100,
                ProductImageURL = "test123123",
                ProductPriceForSelling = 150
            };

            context.Products.Add(productToEdit);
            context.SaveChanges();

            var productDto = new ProductDTO
            {
                ProductName = "test123",
                ProductDescription = "test123",
                CategoryId = 2,
                ProductPrice = 200,
                ImageURL = "testurl",
                ProductPriceForSelling = 250
            };

            var result = await controller.EditProducts(productToEdit.Id, productDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic returnValue = okResult.Value;
            Assert.True(returnValue.Success);
            Assert.Equal("The changes has been successfully registered.", returnValue.Message);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsOkResult_WhenProductIsDeletedSuccessfully()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase4")
                .Options;

            using var context = new AppDbContext(options);

            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ProductController(context, mockConfiguration.Object);

            var productToDelete = new Product
            {
                Id = 1,
                ProductName = "TestProduct",
                ProductDescription = "TestDescription",
                CategoryId = 1,
                ProductPrice = 100,
                ProductImageURL = "http://test.com/image.jpg",
                ProductPriceForSelling = 150
            };

            context.Products.Add(productToDelete);
            context.SaveChanges();

            var result = await controller.DeleteProduct(productToDelete.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic returnValue = okResult.Value;
            Assert.True(returnValue.Success);
            Assert.Equal("The product has been successfully deleted.", returnValue.Message);
        }


    }
}