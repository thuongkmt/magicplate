using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AlterTableServices_AddTenantIdForSeedingData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), 1 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), 1 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), 1 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), 1 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), 1 });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 15, 56, 34, 784, DateTimeKind.Local), 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 14, 3, 40, 888, DateTimeKind.Local), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 14, 3, 40, 889, DateTimeKind.Local), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 14, 3, 40, 889, DateTimeKind.Local), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 14, 3, 40, 889, DateTimeKind.Local), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 14, 3, 40, 889, DateTimeKind.Local), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreationTime", "TenantId" },
                values: new object[] { new DateTime(2019, 12, 4, 14, 3, 40, 889, DateTimeKind.Local), null });
        }
    }
}
