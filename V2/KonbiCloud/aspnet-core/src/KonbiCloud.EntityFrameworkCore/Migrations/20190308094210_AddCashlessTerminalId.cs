using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddCashlessTerminalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CashlessTerminalId",
                table: "Machines",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RegisteredAzureIoT",
                table: "Machines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StopSalesAfter",
                table: "Machines",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TemperatureStopSales",
                table: "Machines",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashlessTerminalId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "RegisteredAzureIoT",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "StopSalesAfter",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "TemperatureStopSales",
                table: "Machines");
        }
    }
}
