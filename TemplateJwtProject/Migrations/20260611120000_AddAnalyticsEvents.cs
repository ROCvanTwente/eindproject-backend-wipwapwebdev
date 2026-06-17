using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TemplateJwtProject.Data;

#nullable disable

namespace TemplateJwtProject.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260611120000_AddAnalyticsEvents")]
    public partial class AddAnalyticsEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RouteId = table.Column<int>(type: "int", nullable: true),
                    RouteName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VisitorId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_CreatedAt",
                table: "AnalyticsEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType",
                table: "AnalyticsEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_RouteId",
                table: "AnalyticsEvents",
                column: "RouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");
        }
    }
}
