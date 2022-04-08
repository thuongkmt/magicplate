using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class RemoveVendingDiagnostic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackVMCDiagnostic");

            migrationBuilder.DropTable(
                name: "MachineStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlackVMCDiagnostic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LevelA = table.Column<bool>(nullable: false),
                    LevelB = table.Column<bool>(nullable: false),
                    LevelC = table.Column<bool>(nullable: false),
                    LevelD = table.Column<bool>(nullable: false),
                    LevelE = table.Column<bool>(nullable: false),
                    LevelF = table.Column<bool>(nullable: false),
                    LogTime = table.Column<DateTime>(nullable: false),
                    MachineId = table.Column<string>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackVMCDiagnostic", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    A0 = table.Column<string>(nullable: true),
                    A1 = table.Column<string>(nullable: true),
                    A2 = table.Column<string>(nullable: true),
                    A3 = table.Column<string>(nullable: true),
                    A4 = table.Column<string>(nullable: true),
                    A5 = table.Column<string>(nullable: true),
                    A6 = table.Column<string>(nullable: true),
                    A7 = table.Column<string>(nullable: true),
                    A8 = table.Column<string>(nullable: true),
                    A9 = table.Column<string>(nullable: true),
                    B0 = table.Column<string>(nullable: true),
                    B1 = table.Column<string>(nullable: true),
                    B2 = table.Column<string>(nullable: true),
                    B3 = table.Column<string>(nullable: true),
                    B4 = table.Column<string>(nullable: true),
                    B5 = table.Column<string>(nullable: true),
                    B6 = table.Column<string>(nullable: true),
                    B7 = table.Column<string>(nullable: true),
                    B8 = table.Column<string>(nullable: true),
                    B9 = table.Column<string>(nullable: true),
                    C0 = table.Column<string>(nullable: true),
                    C1 = table.Column<string>(nullable: true),
                    C2 = table.Column<string>(nullable: true),
                    C3 = table.Column<string>(nullable: true),
                    C4 = table.Column<string>(nullable: true),
                    C5 = table.Column<string>(nullable: true),
                    C6 = table.Column<string>(nullable: true),
                    C7 = table.Column<string>(nullable: true),
                    C8 = table.Column<string>(nullable: true),
                    C9 = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    D0 = table.Column<string>(nullable: true),
                    D1 = table.Column<string>(nullable: true),
                    D2 = table.Column<string>(nullable: true),
                    D3 = table.Column<string>(nullable: true),
                    D4 = table.Column<string>(nullable: true),
                    D5 = table.Column<string>(nullable: true),
                    D6 = table.Column<string>(nullable: true),
                    D7 = table.Column<string>(nullable: true),
                    D8 = table.Column<string>(nullable: true),
                    D9 = table.Column<string>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    E0 = table.Column<string>(nullable: true),
                    E1 = table.Column<string>(nullable: true),
                    E2 = table.Column<string>(nullable: true),
                    E3 = table.Column<string>(nullable: true),
                    E4 = table.Column<string>(nullable: true),
                    E5 = table.Column<string>(nullable: true),
                    E6 = table.Column<string>(nullable: true),
                    E7 = table.Column<string>(nullable: true),
                    E8 = table.Column<string>(nullable: true),
                    E9 = table.Column<string>(nullable: true),
                    F0 = table.Column<string>(nullable: true),
                    F1 = table.Column<string>(nullable: true),
                    F2 = table.Column<string>(nullable: true),
                    F3 = table.Column<string>(nullable: true),
                    F4 = table.Column<string>(nullable: true),
                    F5 = table.Column<string>(nullable: true),
                    F6 = table.Column<string>(nullable: true),
                    F7 = table.Column<string>(nullable: true),
                    F8 = table.Column<string>(nullable: true),
                    F9 = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    LogTime = table.Column<DateTime>(nullable: false),
                    MachineId = table.Column<string>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStatus", x => x.Id);
                });
        }
    }
}
