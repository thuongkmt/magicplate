using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AlterValueForSeedingServices_ChangeIsArchivedValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 5, 17, 48, 42, 837, DateTimeKind.Local), false });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), true });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), true });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), true });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), true });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), true });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreationTime", "IsArchived" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), true });
        }
    }
}
