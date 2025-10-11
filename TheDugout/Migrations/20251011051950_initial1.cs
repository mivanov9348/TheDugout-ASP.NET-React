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
                name: "FK_SeasonEvents_GameSaves_GameSaveId",
                table: "SeasonEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonEvents_GameSaves_GameSaveId",
                table: "SeasonEvents",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeasonEvents_GameSaves_GameSaveId",
                table: "SeasonEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonEvents_GameSaves_GameSaveId",
                table: "SeasonEvents",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
