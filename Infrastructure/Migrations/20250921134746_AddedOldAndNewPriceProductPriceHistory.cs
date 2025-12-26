using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedOldAndNewPriceProductPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductPriceHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductPriceHistories");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ProductPriceHistories",
                newName: "OldPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "NewPrice",
                table: "ProductPriceHistories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewPrice",
                table: "ProductPriceHistories");

            migrationBuilder.RenameColumn(
                name: "OldPrice",
                table: "ProductPriceHistories",
                newName: "Price");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductPriceHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductPriceHistories",
                type: "datetime2",
                nullable: true);
        }
    }
}
