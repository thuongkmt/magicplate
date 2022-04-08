using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KonbiCloud.Migrations
{
    public partial class RefactorDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlateMenuMachineSyncStatus_PlateMenus_PlateMenuId",
                table: "PlateMenuMachineSyncStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_PlateMenus_Plates_PlateId",
                table: "PlateMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceStrategies_PlateMenus_PlateMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropTable(
                name: "DishTransaction");

            migrationBuilder.DropTable(
                name: "TakeAwayTrays");

            migrationBuilder.DropTable(
                name: "Trays");

            migrationBuilder.DropIndex(
                name: "IX_PriceStrategies_PlateMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropIndex(
                name: "IX_PlateMenus_PlateId",
                table: "PlateMenus");

            migrationBuilder.DropIndex(
                name: "IX_PlateMenuMachineSyncStatus_PlateMenuId",
                table: "PlateMenuMachineSyncStatus");

            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "PlateMenus");

            migrationBuilder.AddColumn<Guid>(
                name: "DiscId",
                table: "ProductTransaction",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductMenuId",
                table: "PriceStrategies",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Plates",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductMenuId",
                table: "PlateMenuMachineSyncStatus",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTransaction_DiscId",
                table: "ProductTransaction",
                column: "DiscId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PlateId",
                table: "Products",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceStrategies_ProductMenuId",
                table: "PriceStrategies",
                column: "ProductMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenuMachineSyncStatus_ProductMenuId",
                table: "PlateMenuMachineSyncStatus",
                column: "ProductMenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlateMenuMachineSyncStatus_PlateMenus_ProductMenuId",
                table: "PlateMenuMachineSyncStatus",
                column: "ProductMenuId",
                principalTable: "PlateMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceStrategies_PlateMenus_ProductMenuId",
                table: "PriceStrategies",
                column: "ProductMenuId",
                principalTable: "PlateMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Plates_PlateId",
                table: "Products",
                column: "PlateId",
                principalTable: "Plates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransaction_Discs_DiscId",
                table: "ProductTransaction",
                column: "DiscId",
                principalTable: "Discs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlateMenuMachineSyncStatus_PlateMenus_ProductMenuId",
                table: "PlateMenuMachineSyncStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceStrategies_PlateMenus_ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Plates_PlateId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransaction_Discs_DiscId",
                table: "ProductTransaction");

            migrationBuilder.DropIndex(
                name: "IX_ProductTransaction_DiscId",
                table: "ProductTransaction");

            migrationBuilder.DropIndex(
                name: "IX_Products_PlateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PriceStrategies_ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropIndex(
                name: "IX_PlateMenuMachineSyncStatus_ProductMenuId",
                table: "PlateMenuMachineSyncStatus");

            migrationBuilder.DropColumn(
                name: "DiscId",
                table: "ProductTransaction");

            migrationBuilder.DropColumn(
                name: "ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Plates");

            migrationBuilder.DropColumn(
                name: "ProductMenuId",
                table: "PlateMenuMachineSyncStatus");

            migrationBuilder.AddColumn<Guid>(
                name: "PlateId",
                table: "PlateMenus",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DishTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DiscId = table.Column<Guid>(nullable: true),
                    TransactionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishTransaction_Discs_DiscId",
                        column: x => x.DiscId,
                        principalTable: "Discs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DishTransaction_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TakeAwayTrays",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TakeAwayTrays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trays",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsSynced = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    SyncDate = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trays", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceStrategies_PlateMenuId",
                table: "PriceStrategies",
                column: "PlateMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_PlateId",
                table: "PlateMenus",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenuMachineSyncStatus_PlateMenuId",
                table: "PlateMenuMachineSyncStatus",
                column: "PlateMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTransaction_DiscId",
                table: "DishTransaction",
                column: "DiscId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTransaction_TransactionId",
                table: "DishTransaction",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlateMenuMachineSyncStatus_PlateMenus_PlateMenuId",
                table: "PlateMenuMachineSyncStatus",
                column: "PlateMenuId",
                principalTable: "PlateMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlateMenus_Plates_PlateId",
                table: "PlateMenus",
                column: "PlateId",
                principalTable: "Plates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceStrategies_PlateMenus_PlateMenuId",
                table: "PriceStrategies",
                column: "PlateMenuId",
                principalTable: "PlateMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
