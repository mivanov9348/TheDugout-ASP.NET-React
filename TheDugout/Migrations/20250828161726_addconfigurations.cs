using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addconfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Position_PositionId",
                table: "Players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Position",
                table: "Position");

            migrationBuilder.RenameTable(
                name: "Position",
                newName: "Positions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Positions_PositionId",
                table: "Players",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Positions_PositionId",
                table: "Players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "Position");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Position",
                table: "Position",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Position_PositionId",
                table: "Players",
                column: "PositionId",
                principalTable: "Position",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
