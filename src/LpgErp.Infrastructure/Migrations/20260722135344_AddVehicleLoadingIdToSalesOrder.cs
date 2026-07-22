using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleLoadingIdToSalesOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VehicleLoadingId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_VehicleLoadingId",
                table: "SalesOrders",
                column: "VehicleLoadingId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_VehicleLoadings_VehicleLoadingId",
                table: "SalesOrders",
                column: "VehicleLoadingId",
                principalTable: "VehicleLoadings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_VehicleLoadings_VehicleLoadingId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_VehicleLoadingId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "VehicleLoadingId",
                table: "SalesOrders");
        }
    }
}
