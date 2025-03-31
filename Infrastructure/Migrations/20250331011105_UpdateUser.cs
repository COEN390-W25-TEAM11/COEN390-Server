using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assigned_Lights_LightId",
                table: "Assigned");

            migrationBuilder.DropForeignKey(
                name: "FK_Assigned_Sensors_SensorId",
                table: "Assigned");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assigned",
                table: "Assigned");

            migrationBuilder.RenameTable(
                name: "Assigned",
                newName: "Assigneds");

            migrationBuilder.RenameIndex(
                name: "IX_Assigned_SensorId",
                table: "Assigneds",
                newName: "IX_Assigneds_SensorId");

            migrationBuilder.RenameIndex(
                name: "IX_Assigned_LightId",
                table: "Assigneds",
                newName: "IX_Assigneds_LightId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assigneds",
                table: "Assigneds",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assigneds_Lights_LightId",
                table: "Assigneds",
                column: "LightId",
                principalTable: "Lights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assigneds_Sensors_SensorId",
                table: "Assigneds",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assigneds_Lights_LightId",
                table: "Assigneds");

            migrationBuilder.DropForeignKey(
                name: "FK_Assigneds_Sensors_SensorId",
                table: "Assigneds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assigneds",
                table: "Assigneds");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Assigneds",
                newName: "Assigned");

            migrationBuilder.RenameIndex(
                name: "IX_Assigneds_SensorId",
                table: "Assigned",
                newName: "IX_Assigned_SensorId");

            migrationBuilder.RenameIndex(
                name: "IX_Assigneds_LightId",
                table: "Assigned",
                newName: "IX_Assigned_LightId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assigned",
                table: "Assigned",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assigned_Lights_LightId",
                table: "Assigned",
                column: "LightId",
                principalTable: "Lights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assigned_Sensors_SensorId",
                table: "Assigned",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
