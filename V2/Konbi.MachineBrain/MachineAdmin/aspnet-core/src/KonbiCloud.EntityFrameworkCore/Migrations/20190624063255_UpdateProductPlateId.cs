using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace KonbiCloud.Migrations
{
    public partial class UpdateProductPlateId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlateId",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "Products");
        }
    }
}
