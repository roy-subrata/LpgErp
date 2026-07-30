using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCylinderForfeited : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Forfeited",
                table: "CustomerCylinderBalances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Forfeited",
                table: "CustomerCylinderBalances");
        }
    }
}
