using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class initial3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_GameSaves_GameSaveId1",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_GameSaveId1",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "GameSaveId1",
                table: "Transfers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameSaveId1",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_GameSaveId1",
                table: "Transfers",
                column: "GameSaveId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_GameSaves_GameSaveId1",
                table: "Transfers",
                column: "GameSaveId1",
                principalTable: "GameSaves",
                principalColumn: "Id");
        }
    }
}
