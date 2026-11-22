using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMailer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailOpenTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstOpenedAt",
                table: "Applications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOpenedAt",
                table: "Applications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OpenCount",
                table: "Applications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TrackingToken",
                table: "Applications",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TrackingToken",
                table: "Applications",
                column: "TrackingToken",
                unique: true,
                filter: "[TrackingToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_TrackingToken",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "FirstOpenedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "LastOpenedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OpenCount",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "TrackingToken",
                table: "Applications");
        }
    }
}
