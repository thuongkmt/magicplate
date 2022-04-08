using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class AddServiceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ServiceName = table.Column<string>(nullable: true),
                    IsArchived = table.Column<bool>(nullable: false),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsArchived", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Name", "ServiceName", "TenantId" },
                values: new object[,]
                {
                    { 1, new DateTime(2019, 12, 4, 11, 46, 15, 643, DateTimeKind.Local), null, null, null, true, false, null, null, "MagicPlate Server", "MagicPlateServer.Service", null },
                    { 2, new DateTime(2019, 12, 4, 11, 46, 15, 643, DateTimeKind.Local), null, null, null, true, false, null, null, "RabbitMq Server", "RabbitMqServer.Service", null },
                    { 3, new DateTime(2019, 12, 4, 11, 46, 15, 643, DateTimeKind.Local), null, null, null, true, false, null, null, "Tag reader", "TagReader.Service", null },
                    { 4, new DateTime(2019, 12, 4, 11, 46, 15, 643, DateTimeKind.Local), null, null, null, true, false, null, null, "LightAlarm", "LightAlarm.Service", null },
                    { 5, new DateTime(2019, 12, 4, 11, 46, 15, 643, DateTimeKind.Local), null, null, null, true, false, null, null, "Camera", "Camera.Service", null },
                    { 6, new DateTime(2019, 12, 4, 11, 46, 15, 643, DateTimeKind.Local), null, null, null, true, false, null, null, " Payment Controller", "PaymentController.Service", null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
