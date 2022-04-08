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
                name: "FK_PriceStrategies_PlateMenus_PlateMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropTable(
                name: "DishTransactions");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "PlateMenus");

            migrationBuilder.DropTable(
                name: "TakeAwayTrays");

            migrationBuilder.DropTable(
                name: "Trays");

            migrationBuilder.DropIndex(
                name: "IX_PriceStrategies_PlateMenuId",
                table: "PriceStrategies");

            migrationBuilder.AddColumn<Guid>(
                name: "DiscId",
                table: "ProductTransactions",
                nullable: true);

            //migrationBuilder.AddColumn<Guid>(
            //    name: "PlateId",
            //    table: "Products",
            //    nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductMenuId",
                table: "PriceStrategies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Plates",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    SessionId = table.Column<Guid>(nullable: false),
                    SelectedDate = table.Column<DateTime>(nullable: false),
                    ContractorPrice = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductMenus_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductMenus_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTransactions_DiscId",
                table: "ProductTransactions",
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
                name: "IX_ProductMenus_ProductId",
                table: "ProductMenus",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMenus_SessionId",
                table: "ProductMenus",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceStrategies_ProductMenus_ProductMenuId",
                table: "PriceStrategies",
                column: "ProductMenuId",
                principalTable: "ProductMenus",
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
                name: "FK_ProductTransactions_Discs_DiscId",
                table: "ProductTransactions",
                column: "DiscId",
                principalTable: "Discs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceStrategies_ProductMenus_ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Plates_PlateId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransactions_Discs_DiscId",
                table: "ProductTransactions");

            migrationBuilder.DropTable(
                name: "ProductMenus");

            migrationBuilder.DropIndex(
                name: "IX_ProductTransactions_DiscId",
                table: "ProductTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Products_PlateId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PriceStrategies_ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropColumn(
                name: "DiscId",
                table: "ProductTransactions");

            migrationBuilder.DropColumn(
                name: "PlateId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductMenuId",
                table: "PriceStrategies");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Plates");

            migrationBuilder.CreateTable(
                name: "DishTransactions",
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
                    table.PrimaryKey("PK_DishTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishTransactions_Discs_DiscId",
                        column: x => x.DiscId,
                        principalTable: "Discs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DishTransactions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CardId = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Ordered = table.Column<double>(nullable: false),
                    Period = table.Column<string>(nullable: true),
                    Quota = table.Column<double>(nullable: false),
                    QuotaCash = table.Column<bool>(nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlateMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContractorPrice = table.Column<decimal>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    PlateId = table.Column<Guid>(nullable: false),
                    Price = table.Column<decimal>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: true),
                    SelectedDate = table.Column<DateTime>(nullable: false),
                    SessionId = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateMenus_Plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "Plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlateMenus_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlateMenus_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
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
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
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
                name: "IX_DishTransactions_DiscId",
                table: "DishTransactions",
                column: "DiscId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTransactions_TransactionId",
                table: "DishTransactions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_PlateId",
                table: "PlateMenus",
                column: "PlateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_ProductId",
                table: "PlateMenus",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateMenus_SessionId",
                table: "PlateMenus",
                column: "SessionId");

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
