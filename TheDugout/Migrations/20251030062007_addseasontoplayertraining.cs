using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addseasontoplayertraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerId1",
                table: "PlayerTrainings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "PlayerTrainings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTrainings_PlayerId1",
                table: "PlayerTrainings",
                column: "PlayerId1");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTrainings_SeasonId",
                table: "PlayerTrainings",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTrainings_Players_PlayerId1",
                table: "PlayerTrainings",
                column: "PlayerId1",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTrainings_Seasons_SeasonId",
                table: "PlayerTrainings",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTrainings_Players_PlayerId1",
                table: "PlayerTrainings");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTrainings_Seasons_SeasonId",
                table: "PlayerTrainings");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTrainings_PlayerId1",
                table: "PlayerTrainings");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTrainings_SeasonId",
                table: "PlayerTrainings");

            migrationBuilder.DropColumn(
                name: "PlayerId1",
                table: "PlayerTrainings");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "PlayerTrainings");
        }
    }
}
