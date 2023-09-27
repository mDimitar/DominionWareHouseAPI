﻿// <auto-generated />
using System;
using DominionWarehouseAPI.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DominionWarehouseAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230927184812_TryFixRGB")]
    partial class TryFixRGB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DominionWarehouseAPI.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CommentFromEmployee")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeliveryAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderStatus")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ShoppingCartId")
                        .HasColumnType("int");

                    b.Property<int>("TotalSum")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("soldFromEmployeeId")
                        .HasColumnType("int");

                    b.Property<int>("soldFromWarehouseId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ShoppingCartId");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.OrderProduct", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("OrderId", "ProductId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductsInOrder");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("ProductDescription")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("ProductImageURL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProductPrice")
                        .HasColumnType("int");

                    b.Property<int>("ProductPriceForSelling")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ProductsInShoppingCart", b =>
                {
                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("ShoppingCartId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("ProductId", "ShoppingCartId");

                    b.HasIndex("ShoppingCartId");

                    b.ToTable("ProductsInShoppingCarts");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ProductsInWarehouse", b =>
                {
                    b.Property<int>("WarehouseId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("Received")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("WarehouseId", "ProductId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductsInWarehouses");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ReceivedGoodsBy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AcceptanceDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("ProductQuantity")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasAlternateKey("UserId", "ProductId");

                    b.HasIndex("ProductId");

                    b.ToTable("ReceivedGoodsBy");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Roles", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            RoleName = "EMPLOYEE"
                        },
                        new
                        {
                            Id = 2,
                            RoleName = "ADMIN"
                        },
                        new
                        {
                            Id = 3,
                            RoleName = "OWNER"
                        },
                        new
                        {
                            Id = 4,
                            RoleName = "BUYER"
                        });
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ShoppingCart", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("TotalPrice")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("ShoppingCart");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            TotalPrice = 0,
                            UserId = 1
                        });
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RoleId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("ShoppingCartId")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WorksAtWarehouse")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Users", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            PasswordHash = "$2a$11$p73pKGRcbl2E5UD/6A1HjO6ZCxUUHMOvqbTsYoHk7LKQAK20zBSzy",
                            RoleId = 2,
                            ShoppingCartId = 1,
                            Username = "dominionadmin"
                        });
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("userId");

                    b.ToTable("Warehouse", (string)null);
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Order", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.ShoppingCart", "ShoppingCart")
                        .WithMany("Orders")
                        .HasForeignKey("ShoppingCartId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DominionWarehouseAPI.Models.User", "User")
                        .WithMany("Orders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ShoppingCart");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.OrderProduct", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.Order", "Order")
                        .WithMany("OrderProducts")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DominionWarehouseAPI.Models.Product", "Product")
                        .WithMany("OrderProducts")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Product", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ProductsInShoppingCart", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.Product", "Product")
                        .WithMany("ProductShoppingCarts")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DominionWarehouseAPI.Models.ShoppingCart", "ShoppingCart")
                        .WithMany("ProductShoppingCarts")
                        .HasForeignKey("ShoppingCartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("ShoppingCart");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ProductsInWarehouse", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.Product", "Product")
                        .WithMany("WarehouseProducts")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DominionWarehouseAPI.Models.Warehouse", "Warehouse")
                        .WithMany("WarehouseProducts")
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ReceivedGoodsBy", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.Product", "Product")
                        .WithMany("GoodsReceived")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DominionWarehouseAPI.Models.User", "User")
                        .WithMany("ReceivedGoods")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ShoppingCart", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.User", "User")
                        .WithOne("ShoppingCart")
                        .HasForeignKey("DominionWarehouseAPI.Models.ShoppingCart", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.User", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.Roles", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Warehouse", b =>
                {
                    b.HasOne("DominionWarehouseAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Order", b =>
                {
                    b.Navigation("OrderProducts");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Product", b =>
                {
                    b.Navigation("GoodsReceived");

                    b.Navigation("OrderProducts");

                    b.Navigation("ProductShoppingCarts");

                    b.Navigation("WarehouseProducts");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Roles", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.ShoppingCart", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("ProductShoppingCarts");
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.User", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("ReceivedGoods");

                    b.Navigation("ShoppingCart")
                        .IsRequired();
                });

            modelBuilder.Entity("DominionWarehouseAPI.Models.Warehouse", b =>
                {
                    b.Navigation("WarehouseProducts");
                });
#pragma warning restore 612, 618
        }
    }
}
