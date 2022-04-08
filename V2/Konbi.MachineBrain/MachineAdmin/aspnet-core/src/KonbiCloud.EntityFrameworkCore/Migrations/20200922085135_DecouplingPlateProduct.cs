using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class DecouplingPlateProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceStrategies_ProductMenus_ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Plates_PlateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PlateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PriceStrategies_ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Products",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SelectedDate",
                table: "ProductMenus",
                type: "Date",
                nullable: false,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProductMenus",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ContractorPrice",
                table: "ProductMenus",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlateId",
                table: "ProductMenus",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMenus_PlateId",
                table: "ProductMenus",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMenus_SelectedDate",
                table: "ProductMenus",
                column: "SelectedDate");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMenus_Plates_PlateId",
                table: "ProductMenus",
                column: "PlateId",
                principalTable: "Plates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMenus_Plates_PlateId",
                table: "ProductMenus");

            migrationBuilder.DropIndex(
                name: "IX_Products_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductMenus_PlateId",
                table: "ProductMenus");

            migrationBuilder.DropIndex(
                name: "IX_ProductMenus_SelectedDate",
                table: "ProductMenus");

            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "ProductMenus");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Products",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlateId",
                table: "Products",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SelectedDate",
                table: "ProductMenus",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "Date");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProductMenus",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<decimal>(
                name: "ContractorPrice",
                table: "ProductMenus",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.CreateIndex(
                name: "IX_Products_PlateId",
                table: "Products",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceStrategies_ProductMenuId",
                table: "PriceStrategies",
                column: "ProductMenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceStrategies_ProductMenus_ProductMenuId",
                table: "PriceStrategies",
                column: "ProductMenuId",
                principalTable: "ProductMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Plates_PlateId",
                table: "Products",
                column: "PlateId",
                principalTable: "Plates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
