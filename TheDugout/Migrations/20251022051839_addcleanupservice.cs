using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addcleanupservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "PlayerMatchStats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_SeasonId",
                table: "PlayerMatchStats",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerMatchStats_Seasons_SeasonId",
                table: "PlayerMatchStats",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerMatchStats_Seasons_SeasonId",
                table: "PlayerMatchStats");

            migrationBuilder.DropIndex(
                name: "IX_PlayerMatchStats_SeasonId",
                table: "PlayerMatchStats");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "PlayerMatchStats");
        }
    }
}
