using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addyouthplayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthPlayer_Players_PlayerId",
                table: "YouthPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthPlayer_YouthAcademies_YouthAcademyId",
                table: "YouthPlayer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YouthPlayer",
                table: "YouthPlayer");

            migrationBuilder.RenameTable(
                name: "YouthPlayer",
                newName: "YouthPlayers");

            migrationBuilder.RenameIndex(
                name: "IX_YouthPlayer_YouthAcademyId",
                table: "YouthPlayers",
                newName: "IX_YouthPlayers_YouthAcademyId");

            migrationBuilder.RenameIndex(
                name: "IX_YouthPlayer_PlayerId",
                table: "YouthPlayers",
                newName: "IX_YouthPlayers_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YouthPlayers",
                table: "YouthPlayers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthPlayers_Players_PlayerId",
                table: "YouthPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthPlayers_YouthAcademies_YouthAcademyId",
                table: "YouthPlayers",
                column: "YouthAcademyId",
                principalTable: "YouthAcademies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthPlayers_Players_PlayerId",
                table: "YouthPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthPlayers_YouthAcademies_YouthAcademyId",
                table: "YouthPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YouthPlayers",
                table: "YouthPlayers");

            migrationBuilder.RenameTable(
                name: "YouthPlayers",
                newName: "YouthPlayer");

            migrationBuilder.RenameIndex(
                name: "IX_YouthPlayers_YouthAcademyId",
                table: "YouthPlayer",
                newName: "IX_YouthPlayer_YouthAcademyId");

            migrationBuilder.RenameIndex(
                name: "IX_YouthPlayers_PlayerId",
                table: "YouthPlayer",
                newName: "IX_YouthPlayer_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YouthPlayer",
                table: "YouthPlayer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthPlayer_Players_PlayerId",
                table: "YouthPlayer",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthPlayer_YouthAcademies_YouthAcademyId",
                table: "YouthPlayer",
                column: "YouthAcademyId",
                principalTable: "YouthAcademies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
