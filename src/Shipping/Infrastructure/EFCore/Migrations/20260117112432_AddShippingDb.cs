using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shipping.Infrastructure.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingCarriers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupportedRegions = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingCarriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CarrierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShippingRecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShippingPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ShippingStreet = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ShippingWard = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ShippingFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingFeeCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "VND"),
                    CODAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CODCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "VND"),
                    PackageWeightInGrams = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PackageLengthInCm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PackageWidthInCm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PackageHeightInCm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PackageItemCount = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualDeliveryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_ShippingCarriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "ShippingCarriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ShippingCarrierSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ShopId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApiUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebhookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdditionalSettings = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true),
                    ShippingCarrierId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingCarrierSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingCarrierSettings_ShippingCarriers_ShippingCarrierId",
                        column: x => x.ShippingCarrierId,
                        principalTable: "ShippingCarriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackingEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RawData = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackingEvents_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ProductId",
                table: "ShipmentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShipmentId",
                table: "ShipmentItems",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_VariantId",
                table: "ShipmentItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CarrierId",
                table: "Shipments",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderId",
                table: "Shipments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderNumber",
                table: "Shipments",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipmentNumber",
                table: "Shipments",
                column: "ShipmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TrackingNumber",
                table: "Shipments",
                column: "TrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingCarriers_IsEnabled",
                table: "ShippingCarriers",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingCarriers_Name",
                table: "ShippingCarriers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingCarriers_Provider",
                table: "ShippingCarriers",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingCarrierSettings_ShippingCarrierId",
                table: "ShippingCarrierSettings",
                column: "ShippingCarrierId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackingEvents_EventType",
                table: "TrackingEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingEvents_ShipmentId",
                table: "TrackingEvents",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingEvents_Timestamp",
                table: "TrackingEvents",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentItems");

            migrationBuilder.DropTable(
                name: "ShippingCarrierSettings");

            migrationBuilder.DropTable(
                name: "TrackingEvents");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "ShippingCarriers");
        }
    }
}
