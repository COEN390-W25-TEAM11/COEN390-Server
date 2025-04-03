using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Lights", true);
            migrationBuilder.Sql("DELETE FROM Motion", true);

            migrationBuilder.DropForeignKey(
                name: "FK_Motion_Lights_LightId",
                table: "Motion");

            migrationBuilder.RenameColumn(
                name: "LightId",
                table: "Motion",
                newName: "SensorId");

            migrationBuilder.RenameIndex(
                name: "IX_Motion_LightId",
                table: "Motion",
                newName: "IX_Motion_SensorId");

            migrationBuilder.AddColumn<int>(
                name: "Brightness",
                table: "Lights",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EspId",
                table: "Lights",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Pin",
                table: "Lights",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EspId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Pin = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Sensitivity = table.Column<int>(type: "INTEGER", nullable: false),
                    Timeout = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assigned",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LightId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SensorId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assigned", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assigned_Lights_LightId",
                        column: x => x.LightId,
                        principalTable: "Lights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assigned_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assigned_LightId",
                table: "Assigned",
                column: "LightId");

            migrationBuilder.CreateIndex(
                name: "IX_Assigned_SensorId",
                table: "Assigned",
                column: "SensorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Motion_Sensors_SensorId",
                table: "Motion",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Motion_Sensors_SensorId",
                table: "Motion");

            migrationBuilder.DropTable(
                name: "Assigned");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropColumn(
                name: "Brightness",
                table: "Lights");

            migrationBuilder.DropColumn(
                name: "EspId",
                table: "Lights");

            migrationBuilder.DropColumn(
                name: "Pin",
                table: "Lights");

            migrationBuilder.RenameColumn(
                name: "SensorId",
                table: "Motion",
                newName: "LightId");

            migrationBuilder.RenameIndex(
                name: "IX_Motion_SensorId",
                table: "Motion",
                newName: "IX_Motion_LightId");

            migrationBuilder.AddForeignKey(
                name: "FK_Motion_Lights_LightId",
                table: "Motion",
                column: "LightId",
                principalTable: "Lights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
