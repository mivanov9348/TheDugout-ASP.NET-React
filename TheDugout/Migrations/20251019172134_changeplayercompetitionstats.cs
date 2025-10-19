using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class changeplayercompetitionstats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerSeasonStats_Competitions_CompetitionId",
                table: "PlayerSeasonStats");

            migrationBuilder.DropIndex(
                name: "IX_PlayerSeasonStats_CompetitionId",
                table: "PlayerSeasonStats");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "PlayerSeasonStats");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "PlayerSeasonStats",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "PlayerSeasonStats",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerCompetitionStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: true),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Goals = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCompetitionStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerCompetitionStats_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerCompetitionStats_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerCompetitionStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerCompetitionStats_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCompetitionStats_CompetitionId",
                table: "PlayerCompetitionStats",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCompetitionStats_GameSaveId",
                table: "PlayerCompetitionStats",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCompetitionStats_PlayerId",
                table: "PlayerCompetitionStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCompetitionStats_SeasonId",
                table: "PlayerCompetitionStats",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerCompetitionStats");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "PlayerSeasonStats",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "PlayerSeasonStats",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompetitionId",
                table: "PlayerSeasonStats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonStats_CompetitionId",
                table: "PlayerSeasonStats",
                column: "CompetitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerSeasonStats_Competitions_CompetitionId",
                table: "PlayerSeasonStats",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
