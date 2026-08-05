using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluentPOS.Modules.Reporting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Reporting");

            migrationBuilder.CreateTable(
                name: "DailyStoreSales",
                schema: "Reporting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationName = table.Column<string>(type: "text", nullable: true),
                    RoyaltyRatePercent = table.Column<decimal>(type: "numeric", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrdersCount = table.Column<int>(type: "integer", nullable: false),
                    GrossSales = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    RefundsTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    NetSales = table.Column<decimal>(type: "numeric", nullable: false),
                    RoyaltyAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyStoreSales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyStoreSales_OrganizationId",
                schema: "Reporting",
                table: "DailyStoreSales",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyStoreSales_StoreId_Date",
                schema: "Reporting",
                table: "DailyStoreSales",
                columns: new[] { "StoreId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyStoreSales",
                schema: "Reporting");
        }
    }
}
