using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ProductPriceHistories");

            migrationBuilder.RenameColumn(
                name: "ValidTo",
                table: "ProductPriceHistories",
                newName: "UpdatedPriceAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedPriceAt",
                table: "ProductPriceHistories",
                newName: "ValidTo");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ProductPriceHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
