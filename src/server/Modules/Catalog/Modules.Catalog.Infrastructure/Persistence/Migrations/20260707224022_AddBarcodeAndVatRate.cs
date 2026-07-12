using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FluentPOS.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeAndVatRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                schema: "Catalog",
                table: "Products",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VatRateId",
                schema: "Catalog",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VatRates",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatRates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                schema: "Catalog",
                table: "Products",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_VatRateId",
                schema: "Catalog",
                table: "Products",
                column: "VatRateId");

            migrationBuilder.CreateIndex(
                name: "IX_VatRates_Name",
                schema: "Catalog",
                table: "VatRates",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_VatRates_VatRateId",
                schema: "Catalog",
                table: "Products",
                column: "VatRateId",
                principalSchema: "Catalog",
                principalTable: "VatRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_VatRates_VatRateId",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropTable(
                name: "VatRates",
                schema: "Catalog");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_VatRateId",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Barcode",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VatRateId",
                schema: "Catalog",
                table: "Products");
        }
    }
}
