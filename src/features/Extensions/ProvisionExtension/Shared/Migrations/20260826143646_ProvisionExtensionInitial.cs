using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zeus.Academia.Features.Extensions.ProvisionExtension.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ProvisionExtensionInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Extensions",
                columns: table => new
                {
                    Number = table.Column<int>(type: "int", nullable: false),
                    AssignedEmpNr = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extensions", x => x.Number);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Extensions_AssignedEmpNr",
                table: "Extensions",
                column: "AssignedEmpNr",
                unique: true,
                filter: "[AssignedEmpNr] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Extensions");
        }
    }
}
