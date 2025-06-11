using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master_pice.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PCs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PCParts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Laptops",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessLicensePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PCs_CompanyId",
                table: "PCs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PCParts_CompanyId",
                table: "PCParts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Laptops_CompanyId",
                table: "Laptops",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Laptops_Companies_CompanyId",
                table: "Laptops",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PCParts_Companies_CompanyId",
                table: "PCParts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PCs_Companies_CompanyId",
                table: "PCs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Laptops_Companies_CompanyId",
                table: "Laptops");

            migrationBuilder.DropForeignKey(
                name: "FK_PCParts_Companies_CompanyId",
                table: "PCParts");

            migrationBuilder.DropForeignKey(
                name: "FK_PCs_Companies_CompanyId",
                table: "PCs");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_PCs_CompanyId",
                table: "PCs");

            migrationBuilder.DropIndex(
                name: "IX_PCParts_CompanyId",
                table: "PCParts");

            migrationBuilder.DropIndex(
                name: "IX_Laptops_CompanyId",
                table: "Laptops");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PCs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PCParts");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Laptops");
        }
    }
}
