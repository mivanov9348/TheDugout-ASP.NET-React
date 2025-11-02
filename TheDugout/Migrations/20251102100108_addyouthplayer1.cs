using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addyouthplayer1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerTrainings_Players_PlayerId1",
                table: "PlayerTrainings");

            migrationBuilder.DropIndex(
                name: "IX_PlayerTrainings_PlayerId1",
                table: "PlayerTrainings");

            migrationBuilder.DropColumn(
                name: "PlayerId1",
                table: "PlayerTrainings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerId1",
                table: "PlayerTrainings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTrainings_PlayerId1",
                table: "PlayerTrainings",
                column: "PlayerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerTrainings_Players_PlayerId1",
                table: "PlayerTrainings",
                column: "PlayerId1",
                principalTable: "Players",
                principalColumn: "Id");
        }
    }
}
