using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class AddDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceholdersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "en")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tactics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Defenders = table.Column<int>(type: "int", nullable: false),
                    Midfielders = table.Column<int>(type: "int", nullable: false),
                    Forwards = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tactics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeagueTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LeagueCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    TeamsCount = table.Column<int>(type: "int", nullable: false),
                    RelegationSpots = table.Column<int>(type: "int", nullable: false),
                    PromotionSpots = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueTemplates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageTemplatePlaceholders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageTemplateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplatePlaceholders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageTemplatePlaceholders_MessageTemplates_MessageTemplateId",
                        column: x => x.MessageTemplateId,
                        principalTable: "MessageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float(4)", precision: 4, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionWeights_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PositionWeights_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamTemplates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamTemplates_LeagueTemplates_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "LeagueTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromTeamId = table.Column<int>(type: "int", nullable: true),
                    ToTeamId = table.Column<int>(type: "int", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fixtures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamGoals = table.Column<int>(type: "int", nullable: false),
                    AwayTeamGoals = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixtures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserTeamId = table.Column<int>(type: "int", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSaves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    TeamsCount = table.Column<int>(type: "int", nullable: false),
                    RelegationSpots = table.Column<int>(type: "int", nullable: false),
                    PromotionSpots = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leagues_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leagues_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leagues_LeagueTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "LeagueTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    GameSaveId = table.Column<int>(type: "int", nullable: true),
                    MessageTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_MessageTemplates_MessageTemplateId",
                        column: x => x.MessageTemplateId,
                        principalTable: "MessageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentSaveId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_GameSaves_CurrentSaveId",
                        column: x => x.CurrentSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LogoFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Matches = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Draws = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    GoalsFor = table.Column<int>(type: "int", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    GoalDifference = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Popularity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teams_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teams_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teams_TeamTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TeamTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonEvents_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    KitNumber = table.Column<int>(type: "int", nullable: false),
                    HeightCm = table.Column<double>(type: "float", nullable: false),
                    WeightKg = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Players_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamTactics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    TacticId = table.Column<int>(type: "int", nullable: false),
                    LineupJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTactics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamTactics_Tactics_TacticId",
                        column: x => x.TacticId,
                        principalTable: "Tactics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamTactics_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAttributes_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAttributes_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMatchStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    YellowCards = table.Column<int>(type: "int", nullable: false),
                    RedCards = table.Column<int>(type: "int", nullable: false),
                    MinutesPlayed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "int", nullable: false),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    YellowCards = table.Column<int>(type: "int", nullable: false),
                    RedCards = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonStats_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    FromTeamId = table.Column<int>(type: "int", nullable: true),
                    ToTeamId = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsFreeAgent = table.Column<bool>(type: "bit", nullable: false),
                    GameDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Teams_FromTeamId",
                        column: x => x.FromTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Teams_ToTeamId",
                        column: x => x.ToTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTrainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingSessionId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    ChangeValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTrainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerTrainings_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerTrainings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerTrainings_TrainingSessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalTable: "TrainingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_Code",
                table: "Attributes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_GameSaveId",
                table: "Banks",
                column: "GameSaveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BankId",
                table: "FinancialTransactions",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FromTeamId",
                table: "FinancialTransactions",
                column: "FromTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ToTeamId",
                table: "FinancialTransactions",
                column: "ToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_AwayTeamId",
                table: "Fixtures",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_GameSaveId",
                table: "Fixtures",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_HomeTeamId",
                table: "Fixtures",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_LeagueId",
                table: "Fixtures",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_SeasonId",
                table: "Fixtures",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSaves_UserId",
                table: "GameSaves",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSaves_UserTeamId",
                table: "GameSaves",
                column: "UserTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CountryId",
                table: "Leagues",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_GameSaveId",
                table: "Leagues",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_TemplateId",
                table: "Leagues",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueTemplates_CountryId",
                table: "LeagueTemplates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GameSaveId_Date",
                table: "Messages",
                columns: new[] { "GameSaveId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IsRead",
                table: "Messages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageTemplateId",
                table: "Messages",
                column: "MessageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplatePlaceholders_MessageTemplateId_Name",
                table: "MessageTemplatePlaceholders",
                columns: new[] { "MessageTemplateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_Category_IsActive",
                table: "MessageTemplates",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAttributes_AttributeId",
                table: "PlayerAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAttributes_PlayerId_AttributeId",
                table: "PlayerAttributes",
                columns: new[] { "PlayerId", "AttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_PlayerId",
                table: "PlayerMatchStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CountryId",
                table: "Players",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameSaveId",
                table: "Players",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PositionId",
                table: "Players",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamId",
                table: "Players",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonStats_PlayerId",
                table: "PlayerSeasonStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonStats_SeasonId",
                table: "PlayerSeasonStats",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTrainings_AttributeId",
                table: "PlayerTrainings",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTrainings_PlayerId",
                table: "PlayerTrainings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTrainings_TrainingSessionId",
                table: "PlayerTrainings",
                column: "TrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Code",
                table: "Positions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionWeights_AttributeId",
                table: "PositionWeights",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionWeights_PositionId_AttributeId",
                table: "PositionWeights",
                columns: new[] { "PositionId", "AttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonEvents_SeasonId",
                table: "SeasonEvents",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_GameSaveId",
                table: "Seasons",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CountryId",
                table: "Teams",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_GameSaveId",
                table: "Teams",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeagueId",
                table: "Teams",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TemplateId",
                table: "Teams",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTactics_TacticId",
                table: "TeamTactics",
                column: "TacticId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTactics_TeamId",
                table: "TeamTactics",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamTemplates_CountryId",
                table: "TeamTemplates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTemplates_LeagueId",
                table: "TeamTemplates",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_GameSaveId",
                table: "TrainingSessions",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_SeasonId",
                table: "TrainingSessions",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_TeamId",
                table: "TrainingSessions",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FromTeamId",
                table: "Transfers",
                column: "FromTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_GameSaveId",
                table: "Transfers",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_PlayerId",
                table: "Transfers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SeasonId",
                table: "Transfers",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ToTeamId",
                table: "Transfers",
                column: "ToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentSaveId",
                table: "Users",
                column: "CurrentSaveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_GameSaves_GameSaveId",
                table: "Banks",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Teams_FromTeamId",
                table: "FinancialTransactions",
                column: "FromTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Teams_ToTeamId",
                table: "FinancialTransactions",
                column: "ToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fixtures_GameSaves_GameSaveId",
                table: "Fixtures",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fixtures_Leagues_LeagueId",
                table: "Fixtures",
                column: "LeagueId",
                principalTable: "Leagues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fixtures_Seasons_SeasonId",
                table: "Fixtures",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fixtures_Teams_AwayTeamId",
                table: "Fixtures",
                column: "AwayTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fixtures_Teams_HomeTeamId",
                table: "Fixtures",
                column: "HomeTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSaves_Teams_UserTeamId",
                table: "GameSaves",
                column: "UserTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSaves_Users_UserId",
                table: "GameSaves",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leagues_GameSaves_GameSaveId",
                table: "Leagues");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_GameSaves_GameSaveId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_GameSaves_CurrentSaveId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "Fixtures");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "MessageTemplatePlaceholders");

            migrationBuilder.DropTable(
                name: "PlayerAttributes");

            migrationBuilder.DropTable(
                name: "PlayerMatchStats");

            migrationBuilder.DropTable(
                name: "PlayerSeasonStats");

            migrationBuilder.DropTable(
                name: "PlayerTrainings");

            migrationBuilder.DropTable(
                name: "PositionWeights");

            migrationBuilder.DropTable(
                name: "SeasonEvents");

            migrationBuilder.DropTable(
                name: "TeamTactics");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "MessageTemplates");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropTable(
                name: "Tactics");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "GameSaves");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "TeamTemplates");

            migrationBuilder.DropTable(
                name: "LeagueTemplates");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
