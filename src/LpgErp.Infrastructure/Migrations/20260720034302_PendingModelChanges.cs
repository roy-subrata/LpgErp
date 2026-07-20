using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LpgErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "SalesOrders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "SalesOrders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransportCompanyId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisitTime",
                table: "SalesOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dealer",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransportCompanyId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportationCost",
                table: "PurchaseOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Distance",
                table: "DriverSettlements",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentDueDays",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TransportCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportCompanies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_TransportCompanyId",
                table: "SalesOrders",
                column: "TransportCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_TransportCompanyId",
                table: "PurchaseOrders",
                column: "TransportCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_TransportCompanies_TransportCompanyId",
                table: "PurchaseOrders",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_TransportCompanies_TransportCompanyId",
                table: "SalesOrders",
                column: "TransportCompanyId",
                principalTable: "TransportCompanies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_TransportCompanies_TransportCompanyId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_TransportCompanies_TransportCompanyId",
                table: "SalesOrders");

            migrationBuilder.DropTable(
                name: "TransportCompanies");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_TransportCompanyId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_TransportCompanyId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "TransportCompanyId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "VisitTime",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Dealer",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "TransportCompanyId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "TransportationCost",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "DriverSettlements");

            migrationBuilder.DropColumn(
                name: "PaymentDueDays",
                table: "Customers");
        }
    }
}
