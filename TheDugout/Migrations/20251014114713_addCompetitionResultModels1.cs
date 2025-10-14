using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addCompetitionResultModels1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeam_GameSaves_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeam_Teams_TeamId",
                table: "CompetitionEuropeanQualifiedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPromotedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPromotedTeam_GameSaves_GameSaveId",
                table: "CompetitionPromotedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPromotedTeam_Teams_TeamId",
                table: "CompetitionPromotedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionRelegatedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionRelegatedTeam_GameSaves_GameSaveId",
                table: "CompetitionRelegatedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionRelegatedTeam_Teams_TeamId",
                table: "CompetitionRelegatedTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResult_Competitions_CompetitionId",
                table: "CompetitionSeasonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResult_GameSaves_GameSaveId",
                table: "CompetitionSeasonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResult_Seasons_SeasonId",
                table: "CompetitionSeasonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResult_Teams_ChampionTeamId",
                table: "CompetitionSeasonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResult_Teams_RunnerUpTeamId",
                table: "CompetitionSeasonResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionSeasonResult",
                table: "CompetitionSeasonResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionRelegatedTeam",
                table: "CompetitionRelegatedTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionPromotedTeam",
                table: "CompetitionPromotedTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionEuropeanQualifiedTeam",
                table: "CompetitionEuropeanQualifiedTeam");

            migrationBuilder.RenameTable(
                name: "CompetitionSeasonResult",
                newName: "CompetitionSeasonResults");

            migrationBuilder.RenameTable(
                name: "CompetitionRelegatedTeam",
                newName: "CompetitionRelegatedTeams");

            migrationBuilder.RenameTable(
                name: "CompetitionPromotedTeam",
                newName: "CompetitionPromotedTeams");

            migrationBuilder.RenameTable(
                name: "CompetitionEuropeanQualifiedTeam",
                newName: "CompetitionEuropeanQualifiedTeams");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResult_SeasonId",
                table: "CompetitionSeasonResults",
                newName: "IX_CompetitionSeasonResults_SeasonId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResult_RunnerUpTeamId",
                table: "CompetitionSeasonResults",
                newName: "IX_CompetitionSeasonResults_RunnerUpTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResult_GameSaveId",
                table: "CompetitionSeasonResults",
                newName: "IX_CompetitionSeasonResults_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResult_CompetitionId",
                table: "CompetitionSeasonResults",
                newName: "IX_CompetitionSeasonResults_CompetitionId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResult_ChampionTeamId",
                table: "CompetitionSeasonResults",
                newName: "IX_CompetitionSeasonResults_ChampionTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionRelegatedTeam_TeamId",
                table: "CompetitionRelegatedTeams",
                newName: "IX_CompetitionRelegatedTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionRelegatedTeam_GameSaveId",
                table: "CompetitionRelegatedTeams",
                newName: "IX_CompetitionRelegatedTeams_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionRelegatedTeam_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeams",
                newName: "IX_CompetitionRelegatedTeams_CompetitionSeasonResultId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionPromotedTeam_TeamId",
                table: "CompetitionPromotedTeams",
                newName: "IX_CompetitionPromotedTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionPromotedTeam_GameSaveId",
                table: "CompetitionPromotedTeams",
                newName: "IX_CompetitionPromotedTeams_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionPromotedTeam_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeams",
                newName: "IX_CompetitionPromotedTeams_CompetitionSeasonResultId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionEuropeanQualifiedTeam_TeamId",
                table: "CompetitionEuropeanQualifiedTeams",
                newName: "IX_CompetitionEuropeanQualifiedTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionEuropeanQualifiedTeam_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeams",
                newName: "IX_CompetitionEuropeanQualifiedTeams_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionEuropeanQualifiedTeam_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeams",
                newName: "IX_CompetitionEuropeanQualifiedTeams_CompetitionSeasonResultId");

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "CompetitionEuropeanQualifiedTeams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionSeasonResults",
                table: "CompetitionSeasonResults",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionRelegatedTeams",
                table: "CompetitionRelegatedTeams",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionPromotedTeams",
                table: "CompetitionPromotedTeams",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionEuropeanQualifiedTeams",
                table: "CompetitionEuropeanQualifiedTeams",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeams_CompetitionSeasonResults_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeams",
                column: "CompetitionSeasonResultId",
                principalTable: "CompetitionSeasonResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeams_GameSaves_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeams",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeams_Teams_TeamId",
                table: "CompetitionEuropeanQualifiedTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPromotedTeams_CompetitionSeasonResults_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeams",
                column: "CompetitionSeasonResultId",
                principalTable: "CompetitionSeasonResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPromotedTeams_GameSaves_GameSaveId",
                table: "CompetitionPromotedTeams",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPromotedTeams_Teams_TeamId",
                table: "CompetitionPromotedTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionRelegatedTeams_CompetitionSeasonResults_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeams",
                column: "CompetitionSeasonResultId",
                principalTable: "CompetitionSeasonResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionRelegatedTeams_GameSaves_GameSaveId",
                table: "CompetitionRelegatedTeams",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionRelegatedTeams_Teams_TeamId",
                table: "CompetitionRelegatedTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResults_Competitions_CompetitionId",
                table: "CompetitionSeasonResults",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResults_GameSaves_GameSaveId",
                table: "CompetitionSeasonResults",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResults_Seasons_SeasonId",
                table: "CompetitionSeasonResults",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResults_Teams_ChampionTeamId",
                table: "CompetitionSeasonResults",
                column: "ChampionTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResults_Teams_RunnerUpTeamId",
                table: "CompetitionSeasonResults",
                column: "RunnerUpTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeams_CompetitionSeasonResults_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeams_GameSaves_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeams_Teams_TeamId",
                table: "CompetitionEuropeanQualifiedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPromotedTeams_CompetitionSeasonResults_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPromotedTeams_GameSaves_GameSaveId",
                table: "CompetitionPromotedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionPromotedTeams_Teams_TeamId",
                table: "CompetitionPromotedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionRelegatedTeams_CompetitionSeasonResults_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionRelegatedTeams_GameSaves_GameSaveId",
                table: "CompetitionRelegatedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionRelegatedTeams_Teams_TeamId",
                table: "CompetitionRelegatedTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResults_Competitions_CompetitionId",
                table: "CompetitionSeasonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResults_GameSaves_GameSaveId",
                table: "CompetitionSeasonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResults_Seasons_SeasonId",
                table: "CompetitionSeasonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResults_Teams_ChampionTeamId",
                table: "CompetitionSeasonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionSeasonResults_Teams_RunnerUpTeamId",
                table: "CompetitionSeasonResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionSeasonResults",
                table: "CompetitionSeasonResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionRelegatedTeams",
                table: "CompetitionRelegatedTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionPromotedTeams",
                table: "CompetitionPromotedTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompetitionEuropeanQualifiedTeams",
                table: "CompetitionEuropeanQualifiedTeams");

            migrationBuilder.RenameTable(
                name: "CompetitionSeasonResults",
                newName: "CompetitionSeasonResult");

            migrationBuilder.RenameTable(
                name: "CompetitionRelegatedTeams",
                newName: "CompetitionRelegatedTeam");

            migrationBuilder.RenameTable(
                name: "CompetitionPromotedTeams",
                newName: "CompetitionPromotedTeam");

            migrationBuilder.RenameTable(
                name: "CompetitionEuropeanQualifiedTeams",
                newName: "CompetitionEuropeanQualifiedTeam");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResults_SeasonId",
                table: "CompetitionSeasonResult",
                newName: "IX_CompetitionSeasonResult_SeasonId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResults_RunnerUpTeamId",
                table: "CompetitionSeasonResult",
                newName: "IX_CompetitionSeasonResult_RunnerUpTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResults_GameSaveId",
                table: "CompetitionSeasonResult",
                newName: "IX_CompetitionSeasonResult_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResults_CompetitionId",
                table: "CompetitionSeasonResult",
                newName: "IX_CompetitionSeasonResult_CompetitionId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionSeasonResults_ChampionTeamId",
                table: "CompetitionSeasonResult",
                newName: "IX_CompetitionSeasonResult_ChampionTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionRelegatedTeams_TeamId",
                table: "CompetitionRelegatedTeam",
                newName: "IX_CompetitionRelegatedTeam_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionRelegatedTeams_GameSaveId",
                table: "CompetitionRelegatedTeam",
                newName: "IX_CompetitionRelegatedTeam_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionRelegatedTeams_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeam",
                newName: "IX_CompetitionRelegatedTeam_CompetitionSeasonResultId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionPromotedTeams_TeamId",
                table: "CompetitionPromotedTeam",
                newName: "IX_CompetitionPromotedTeam_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionPromotedTeams_GameSaveId",
                table: "CompetitionPromotedTeam",
                newName: "IX_CompetitionPromotedTeam_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionPromotedTeams_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeam",
                newName: "IX_CompetitionPromotedTeam_CompetitionSeasonResultId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionEuropeanQualifiedTeams_TeamId",
                table: "CompetitionEuropeanQualifiedTeam",
                newName: "IX_CompetitionEuropeanQualifiedTeam_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionEuropeanQualifiedTeams_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeam",
                newName: "IX_CompetitionEuropeanQualifiedTeam_GameSaveId");

            migrationBuilder.RenameIndex(
                name: "IX_CompetitionEuropeanQualifiedTeams_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeam",
                newName: "IX_CompetitionEuropeanQualifiedTeam_CompetitionSeasonResultId");

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "CompetitionEuropeanQualifiedTeam",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionSeasonResult",
                table: "CompetitionSeasonResult",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionRelegatedTeam",
                table: "CompetitionRelegatedTeam",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionPromotedTeam",
                table: "CompetitionPromotedTeam",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompetitionEuropeanQualifiedTeam",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "CompetitionSeasonResultId",
                principalTable: "CompetitionSeasonResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeam_GameSaves_GameSaveId",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEuropeanQualifiedTeam_Teams_TeamId",
                table: "CompetitionEuropeanQualifiedTeam",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPromotedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                table: "CompetitionPromotedTeam",
                column: "CompetitionSeasonResultId",
                principalTable: "CompetitionSeasonResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPromotedTeam_GameSaves_GameSaveId",
                table: "CompetitionPromotedTeam",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionPromotedTeam_Teams_TeamId",
                table: "CompetitionPromotedTeam",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionRelegatedTeam_CompetitionSeasonResult_CompetitionSeasonResultId",
                table: "CompetitionRelegatedTeam",
                column: "CompetitionSeasonResultId",
                principalTable: "CompetitionSeasonResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionRelegatedTeam_GameSaves_GameSaveId",
                table: "CompetitionRelegatedTeam",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionRelegatedTeam_Teams_TeamId",
                table: "CompetitionRelegatedTeam",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResult_Competitions_CompetitionId",
                table: "CompetitionSeasonResult",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResult_GameSaves_GameSaveId",
                table: "CompetitionSeasonResult",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResult_Seasons_SeasonId",
                table: "CompetitionSeasonResult",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResult_Teams_ChampionTeamId",
                table: "CompetitionSeasonResult",
                column: "ChampionTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionSeasonResult_Teams_RunnerUpTeamId",
                table: "CompetitionSeasonResult",
                column: "RunnerUpTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
