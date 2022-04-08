using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class MenuMustHasAproduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlateMenus_Products_ProductId",
                table: "PlateMenus");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "PlateMenus",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlateMenus_Products_ProductId",
                table: "PlateMenus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlateMenus_Products_ProductId",
                table: "PlateMenus");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "PlateMenus",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_PlateMenus_Products_ProductId",
                table: "PlateMenus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
