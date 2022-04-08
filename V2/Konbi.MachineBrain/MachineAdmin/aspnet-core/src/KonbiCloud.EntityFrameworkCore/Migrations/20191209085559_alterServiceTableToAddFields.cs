using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class alterServiceTableToAddFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Services",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ErrotAt",
                table: "Services",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsError",
                table: "Services",
                nullable: false,
                defaultValue: false);
            migrationBuilder.InsertData(
             table: "Services",
             columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsArchived", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Name", "TenantId", "Type" },
             values: new object[,]
             {
                    { 1, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "MagicPlate Server", 1, "MagicPlateServer.Service" },
                    { 2, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "RabbitMQ Server", 1, "RabbitMqServer.Service" },
                    { 3, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "Tag reader", 1, "TagReader.Service" },
                    { 4, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "LightAlarm", 1, "LightAlarm.Service" },
                    { 5, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "Camera", 1, "Camera.Service" },
                    { 6, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, " Payment Controller", 1, "PaymentController.Service" }
             });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ErrotAt",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsError",
                table: "Services");

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsArchived", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Name", "TenantId", "Type" },
                values: new object[,]
                {
                    { 1, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "MagicPlate Server", 1, "MagicPlateServer.Service" },
                    { 2, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "RabbitMq Server", 1, "RabbitMqServer.Service" },
                    { 3, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "Tag reader", 1, "TagReader.Service" },
                    { 4, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "LightAlarm", 1, "LightAlarm.Service" },
                    { 5, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, "Camera", 1, "Camera.Service" },
                    { 6, new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), null, null, null, false, false, null, null, " Payment Controller", 1, "PaymentController.Service" }
                });
        }
    }
}
