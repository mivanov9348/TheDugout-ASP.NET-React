using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameSaves_GameSaveId1",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_GameSaveId1",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GameSaveId1",
                table: "Players");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameSaveId1",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameSaveId1",
                table: "Players",
                column: "GameSaveId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameSaves_GameSaveId1",
                table: "Players",
                column: "GameSaveId1",
                principalTable: "GameSaves",
                principalColumn: "Id");
        }
    }
}
