using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addCompetitionResultModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitionSeasonResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: true),
                    GameSaveId = table.Column<int>(type: "int", nullable: true),
                    CompetitionType = table.Column<int>(type: "int", nullable: false),
                    ChampionTeamId = table.Column<int>(type: "int", nullable: true),
                    RunnerUpTeamId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionSeasonResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionSeasonResult_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionSeasonResult_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionSeasonResult_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionSeasonResult_Teams_ChampionTeamId",
                        column: x => x.ChampionTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionSeasonResult_Teams_RunnerUpTeamId",
                        column: x => x.RunnerUpTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionEuropeanQualifiedTeam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitionSeasonResultId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionEuropeanQualifiedTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionEuropeanQualifiedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                        column: x => x.CompetitionSeasonResultId,
                        principalTable: "CompetitionSeasonResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitionEuropeanQualifiedTeam_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionEuropeanQualifiedTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionPromotedTeam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitionSeasonResultId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionPromotedTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionPromotedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                        column: x => x.CompetitionSeasonResultId,
                        principalTable: "CompetitionSeasonResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitionPromotedTeam_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionPromotedTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionRelegatedTeam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitionSeasonResultId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionRelegatedTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionRelegatedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                        column: x => x.CompetitionSeasonResultId,
                        principalTable: "CompetitionSeasonResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitionRelegatedTeam_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitionRelegatedTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEuropeanQualifiedTeam_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "CompetitionSeasonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEuropeanQualifiedTeam_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEuropeanQualifiedTeam_TeamId",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionPromotedTeam_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeam",
                column: "CompetitionSeasonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionPromotedTeam_GameSaveId",
                table: "CompetitionPromotedTeam",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionPromotedTeam_TeamId",
                table: "CompetitionPromotedTeam",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionRelegatedTeam_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeam",
                column: "CompetitionSeasonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionRelegatedTeam_GameSaveId",
                table: "CompetitionRelegatedTeam",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionRelegatedTeam_TeamId",
                table: "CompetitionRelegatedTeam",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionSeasonResult_ChampionTeamId",
                table: "CompetitionSeasonResult",
                column: "ChampionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionSeasonResult_CompetitionId",
                table: "CompetitionSeasonResult",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionSeasonResult_GameSaveId",
                table: "CompetitionSeasonResult",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionSeasonResult_RunnerUpTeamId",
                table: "CompetitionSeasonResult",
                column: "RunnerUpTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionSeasonResult_SeasonId",
                table: "CompetitionSeasonResult",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitionEuropeanQualifiedTeam");

            migrationBuilder.DropTable(
                name: "CompetitionPromotedTeam");

            migrationBuilder.DropTable(
                name: "CompetitionRelegatedTeam");

            migrationBuilder.DropTable(
                name: "CompetitionSeasonResult");
        }
    }
}
