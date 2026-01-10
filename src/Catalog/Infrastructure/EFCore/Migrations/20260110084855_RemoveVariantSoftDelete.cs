using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVariantSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Variants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Variants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Variants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Variants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Variants",
                type: "text",
                nullable: true);
        }
    }
}
