using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master_pice.Migrations
{
    /// <inheritdoc />
    public partial class imgPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ProductViewModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptImage",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "ProductViewModel");

            migrationBuilder.DropColumn(
                name: "ReceiptImage",
                table: "Payments");
        }
    }
}
