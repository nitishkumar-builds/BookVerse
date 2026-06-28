using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookVerse.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterSubscribers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterSubscribers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscribers", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book1.jpg", 799.0, 699.0, 599.0, 649.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book2.jpg", 749.0, 649.0, 549.0, 599.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book3.jpg", 699.0, 599.0, 499.0, 549.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book4.jpg", 599.0, 499.0, 419.0, 449.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book5.jpg", 649.0, 549.0, 449.0, 499.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book6.jpg", 799.0, 699.0, 599.0, 649.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book7.jpg", 749.0, 649.0, 549.0, 599.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "/images/products/book8.jpg", 799.0, 749.0, 649.0, 699.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscribers");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 99.0, 90.0, 80.0, 85.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 109.0, 95.0, 85.0, 90.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 89.0, 80.0, 70.0, 75.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 79.0, 70.0, 60.0, 65.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 84.0, 75.0, 65.0, 70.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 99.0, 90.0, 80.0, 85.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 94.0, 85.0, 75.0, 80.0 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ImageUrl", "ListPrice", "Price", "Price100", "Price50" },
                values: new object[] { "", 104.0, 95.0, 85.0, 90.0 });
        }
    }
}
