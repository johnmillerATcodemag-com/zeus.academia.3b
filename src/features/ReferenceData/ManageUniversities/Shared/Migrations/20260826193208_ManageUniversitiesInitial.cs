using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ManageUniversitiesInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Code);
                    table.CheckConstraint("CK_Universities_Code_Allowed", "[Code] IN ('BOSTON_U', 'MIT', 'STANFORD')");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Universities");
        }
    }
}
