using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionBalance",
                table: "Suppliers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPerCylinder",
                table: "Suppliers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionApplied",
                table: "PurchaseOrders",
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
                name: "CommissionBalance",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CommissionPerCylinder",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CommissionApplied",
                table: "PurchaseOrders");
        }
    }
}
