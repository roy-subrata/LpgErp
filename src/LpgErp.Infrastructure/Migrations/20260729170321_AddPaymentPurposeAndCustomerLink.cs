using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPurposeAndCustomerLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CylinderDepositId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CylinderExchangeId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId_PaymentDate",
                table: "Payments",
                columns: new[] { "CustomerId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CylinderDepositId",
                table: "Payments",
                column: "CylinderDepositId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CylinderExchangeId",
                table: "Payments",
                column: "CylinderExchangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CylinderDeposits_CylinderDepositId",
                table: "Payments",
                column: "CylinderDepositId",
                principalTable: "CylinderDeposits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CylinderExchanges_CylinderExchangeId",
                table: "Payments",
                column: "CylinderExchangeId",
                principalTable: "CylinderExchanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Every existing payment reached its customer through the sales order. Fill the direct
            // link so the statement can read a customer's payments without that join, and so
            // on-account payments (which have no order) can use the same column.
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.CustomerId = so.CustomerId
                FROM Payments p
                INNER JOIN SalesOrders so ON so.Id = p.SalesOrderId
                WHERE p.CustomerId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CylinderDeposits_CylinderDepositId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CylinderExchanges_CylinderExchangeId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CustomerId_PaymentDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CylinderDepositId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CylinderExchangeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CylinderDepositId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CylinderExchangeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Payments");
        }
    }
}
