using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TemplateJwtProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolGuideEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PasswordChanged",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EstimatedTimeMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    XCoordinate = table.Column<double>(type: "float", nullable: false),
                    YCoordinate = table.Column<double>(type: "float", nullable: false),
                    BuildingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteLocations",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteLocations", x => new { x.RouteId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_RouteLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteLocations_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Buildings",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Centraal schoolgebouw met receptie en kantoren.", "Hoofdgebouw" },
                    { 2, "Gebouw met technieklokalen en praktijkruimtes.", "Techniekgebouw" }
                });

            migrationBuilder.InsertData(
                table: "Routes",
                columns: new[] { "Id", "Description", "EstimatedTimeMinutes", "Name" },
                values: new object[,]
                {
                    { 1, "Route langs de belangrijkste plekken voor bezoekers.", 20, "Open Dag Route" },
                    { 2, "Route door de techniekvleugel en praktijklokalen.", 15, "Techniek Route" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "BuildingId", "Description", "Floor", "Name", "XCoordinate", "YCoordinate" },
                values: new object[,]
                {
                    { 1, 1, "Hoofdreceptie voor bezoekers en studenten.", 0, "Receptie", 12.5, 8.1999999999999993 },
                    { 2, 1, "Grote ontmoetingsruimte voor evenementen en pauzes.", 0, "Aula", 24.100000000000001, 14.699999999999999 },
                    { 3, 2, "Praktijklokaal voor technieklessen.", 1, "Praktijklokaal T1", 7.4000000000000004, 29.300000000000001 }
                });

            migrationBuilder.InsertData(
                table: "RouteLocations",
                columns: new[] { "LocationId", "RouteId", "Notes", "Order" },
                values: new object[,]
                {
                    { 1, 1, "Start bij de ingang.", 1 },
                    { 2, 1, "Loop via de centrale hal.", 2 },
                    { 1, 2, "Startpunt voor bezoekers.", 1 },
                    { 3, 2, "Eindpunt in het techniekgebouw.", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Name",
                table: "Buildings",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_BuildingId",
                table: "Locations",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_LocationId",
                table: "RouteLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_RouteId_Order",
                table: "RouteLocations",
                columns: new[] { "RouteId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Name",
                table: "Routes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteLocations");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropColumn(
                name: "PasswordChanged",
                table: "AspNetUsers");
        }
    }
}
