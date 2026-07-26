using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CylinderExchangeQuantity",
                table: "SalesOrderItems");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "CylinderExchanges",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CylinderExchanges_WarehouseId",
                table: "CylinderExchanges",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CylinderExchanges_Warehouses_WarehouseId",
                table: "CylinderExchanges",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CylinderExchanges_Warehouses_WarehouseId",
                table: "CylinderExchanges");

            migrationBuilder.DropIndex(
                name: "IX_CylinderExchanges_WarehouseId",
                table: "CylinderExchanges");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "CylinderExchanges");

            migrationBuilder.AddColumn<int>(
                name: "CylinderExchangeQuantity",
                table: "SalesOrderItems",
                type: "int",
                nullable: true);
        }
    }
}
