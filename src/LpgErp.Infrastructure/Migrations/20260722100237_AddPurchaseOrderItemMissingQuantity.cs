using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderItemMissingQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MissingQuantity",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MissingQuantity",
                table: "PurchaseOrderItems");
        }
    }
}
