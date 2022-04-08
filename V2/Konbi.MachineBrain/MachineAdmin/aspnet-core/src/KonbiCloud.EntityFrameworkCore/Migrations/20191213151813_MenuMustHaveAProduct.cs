using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class MenuMustHaveAProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMenus_Products_ProductId",
                table: "ProductMenus");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductMenus",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMenus_Products_ProductId",
                table: "ProductMenus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMenus_Products_ProductId",
                table: "ProductMenus");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductMenus",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMenus_Products_ProductId",
                table: "ProductMenus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
