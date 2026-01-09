using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class InitInventoryDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ThresholdWarning = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => x.Id);
                    table.CheckConstraint("CK_StockItem_Quantity", "\"Quantity\" >= 0");
                    table.CheckConstraint("CK_StockItem_ThresholdWarning", "\"ThresholdWarning\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "StockLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    SnapshotTotalQuantity = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLogs", x => x.Id);
                    table.CheckConstraint("CK_StockLog_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_StockLog_SnapshotTotalQuantity", "\"SnapshotTotalQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_StockLogs_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.CheckConstraint("CK_StockReservation_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_StockReservations_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_Name",
                table: "StockItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_Quantity",
                table: "StockItems",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_Sku",
                table: "StockItems",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_OrderId_CreatedAt",
                table: "StockLogs",
                columns: new[] { "OrderId", "CreatedAt" },
                filter: "\"OrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockLog_StockItemId_CreatedAt",
                table: "StockLogs",
                columns: new[] { "StockItemId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockLogs_OrderId",
                table: "StockLogs",
                column: "OrderId",
                filter: "\"OrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockLogs_StockItemId",
                table: "StockLogs",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLogs_Type",
                table: "StockLogs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservation_OrderId_StockItemId_Unique",
                table: "StockReservations",
                columns: new[] { "OrderId", "StockItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_CreatedAt",
                table: "StockReservations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_OrderId",
                table: "StockReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_StockItemId_CreatedAt",
                table: "StockReservations",
                columns: new[] { "StockItemId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLogs");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "StockItems");
        }
    }
}
