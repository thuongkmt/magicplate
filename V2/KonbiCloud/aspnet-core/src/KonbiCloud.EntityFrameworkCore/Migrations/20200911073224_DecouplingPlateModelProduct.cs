using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class DecouplingPlateModelProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Plates_PlateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PlateId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "Products");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "PlateMenus",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlateId",
                table: "PlateMenus",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_PlateId",
                table: "PlateMenus",
                column: "PlateId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlateMenus_Plates_PlateId",
                table: "PlateMenus",
                column: "PlateId",
                principalTable: "Plates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlateMenus_Plates_PlateId",
                table: "PlateMenus");

            migrationBuilder.DropIndex(
                name: "IX_PlateMenus_PlateId",
                table: "PlateMenus");

            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "PlateMenus");

            migrationBuilder.AddColumn<Guid>(
                name: "PlateId",
                table: "Products",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "PlateMenus",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.CreateIndex(
                name: "IX_Products_PlateId",
                table: "Products",
                column: "PlateId");

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
