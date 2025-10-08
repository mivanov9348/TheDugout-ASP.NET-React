using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgencyTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => x.Id);
                    table.UniqueConstraint("AK_Attributes_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "CupTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinTeams = table.Column<int>(type: "int", nullable: true),
                    MaxTeams = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CupTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EuropeanCupTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TeamsCount = table.Column<int>(type: "int", nullable: false),
                    LeaguePhaseMatchesPerTeam = table.Column<int>(type: "int", nullable: false),
                    Ranking = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeanCupTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.Id);
                    table.UniqueConstraint("AK_EventTypes_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderType = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false)
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
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.UniqueConstraint("AK_Regions_Code", x => x.Code);
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
                name: "EuropeanCupPhaseTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsKnockout = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTwoLegged = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EuropeanCupTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeanCupPhaseTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EuropeanCupPhaseTemplates_EuropeanCupTemplates_EuropeanCupTemplateId",
                        column: x => x.EuropeanCupTemplateId,
                        principalTable: "EuropeanCupTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventAttributeWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTypeCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    AttributeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAttributeWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAttributeWeights_Attributes_AttributeCode",
                        column: x => x.AttributeCode,
                        principalTable: "Attributes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAttributeWeights_EventTypes_EventTypeCode",
                        column: x => x.EventTypeCode,
                        principalTable: "EventTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventOutcomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangesPossession = table.Column<bool>(type: "bit", nullable: false),
                    RangeMin = table.Column<int>(type: "int", nullable: false),
                    RangeMax = table.Column<int>(type: "int", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventOutcomes_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
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
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Countries_Regions_RegionCode",
                        column: x => x.RegionCode,
                        principalTable: "Regions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FirstNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirstNames_Regions_RegionCode",
                        column: x => x.RegionCode,
                        principalTable: "Regions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LastNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LastNames_Regions_RegionCode",
                        column: x => x.RegionCode,
                        principalTable: "Regions",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentaryTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OutcomeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Template = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventOutcomeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentaryTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentaryTemplates_EventOutcomes_EventOutcomeId",
                        column: x => x.EventOutcomeId,
                        principalTable: "EventOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "TeamTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeagueId = table.Column<int>(type: "int", nullable: true)
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
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyTemplateId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Popularity = table.Column<int>(type: "int", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Logo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agencies_AgencyTemplates_AgencyTemplateId",
                        column: x => x.AgencyTemplateId,
                        principalTable: "AgencyTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agencies_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
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
                name: "Competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CupRounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CupId = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CupRounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: true),
                    TeamsCount = table.Column<int>(type: "int", nullable: false),
                    RoundsCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogoFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompetitionId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cups_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cups_Competitions_CompetitionId1",
                        column: x => x.CompetitionId1,
                        principalTable: "Competitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cups_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cups_CupTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CupTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CupTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CupId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    IsEliminated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CupTeams_Cups_CupId",
                        column: x => x.CupId,
                        principalTable: "Cups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EuropeanCupPhases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EuropeanCupId = table.Column<int>(type: "int", nullable: false),
                    PhaseTemplateId = table.Column<int>(type: "int", nullable: false),
                    IsQualificationPhase = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeanCupPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EuropeanCupPhases_EuropeanCupPhaseTemplates_PhaseTemplateId",
                        column: x => x.PhaseTemplateId,
                        principalTable: "EuropeanCupPhaseTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EuropeanCups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: true),
                    Ranking = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogoFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    CompetitionId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeanCups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EuropeanCups_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EuropeanCups_Competitions_CompetitionId1",
                        column: x => x.CompetitionId1,
                        principalTable: "Competitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EuropeanCups_EuropeanCupTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "EuropeanCupTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EuropeanCupStandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EuropeanCupId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Matches = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Draws = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GoalsFor = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GoalDifference = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Ranking = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeanCupStandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EuropeanCupStandings_EuropeanCups_EuropeanCupId",
                        column: x => x.EuropeanCupId,
                        principalTable: "EuropeanCups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EuropeanCupTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EuropeanCupId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CurrentPhaseOrder = table.Column<int>(type: "int", nullable: false),
                    IsEliminated = table.Column<bool>(type: "bit", nullable: false),
                    IsPlayoffParticipant = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeanCupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EuropeanCupTeams_EuropeanCups_EuropeanCupId",
                        column: x => x.EuropeanCupId,
                        principalTable: "EuropeanCups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromTeamId = table.Column<int>(type: "int", nullable: true),
                    ToTeamId = table.Column<int>(type: "int", nullable: true),
                    FromAgencyId = table.Column<int>(type: "int", nullable: true),
                    ToAgencyId = table.Column<int>(type: "int", nullable: true),
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
                        name: "FK_FinancialTransactions_Agencies_FromAgencyId",
                        column: x => x.FromAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Agencies_ToAgencyId",
                        column: x => x.ToAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    CompetitionType = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: true),
                    CupRoundId = table.Column<int>(type: "int", nullable: true),
                    EuropeanCupPhaseId = table.Column<int>(type: "int", nullable: true),
                    HomeTeamId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamId = table.Column<int>(type: "int", nullable: false),
                    HomeTeamGoals = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AwayTeamGoals = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    WinnerTeamId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    IsElimination = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fixtures_CupRounds_CupRoundId",
                        column: x => x.CupRoundId,
                        principalTable: "CupRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fixtures_EuropeanCupPhases_EuropeanCupPhaseId",
                        column: x => x.EuropeanCupPhaseId,
                        principalTable: "EuropeanCupPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    BankId = table.Column<int>(type: "int", nullable: false),
                    NextDayActionLabel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSaves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: false),
                    CurrentMinute = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentTurn = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: true),
                    MessageTemplateId = table.Column<int>(type: "int", nullable: true),
                    GameSaveId1 = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_Messages_GameSaves_GameSaveId1",
                        column: x => x.GameSaveId1,
                        principalTable: "GameSaves",
                        principalColumn: "Id");
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
                    CurrentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: true),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    TeamsCount = table.Column<int>(type: "int", nullable: false),
                    RelegationSpots = table.Column<int>(type: "int", nullable: false),
                    PromotionSpots = table.Column<int>(type: "int", nullable: false),
                    CompetitionId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leagues_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leagues_Competitions_CompetitionId1",
                        column: x => x.CompetitionId1,
                        principalTable: "Competitions",
                        principalColumn: "Id");
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
                    table.ForeignKey(
                        name: "FK_Leagues_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
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
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false)
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
                    CountryId = table.Column<int>(type: "int", nullable: true),
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
                name: "LeagueStandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Matches = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Draws = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GoalsFor = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GoalDifference = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Ranking = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueStandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_GameSaves_GameSaveId",
                        column: x => x.GameSaveId,
                        principalTable: "GameSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    AgencyId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    KitNumber = table.Column<int>(type: "int", nullable: false),
                    HeightCm = table.Column<double>(type: "float", nullable: false),
                    WeightKg = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    GameSaveId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvatarFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentAbility = table.Column<int>(type: "int", nullable: false),
                    PotentialAbility = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Stadiums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    TicketPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stadiums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stadiums_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    SubstitutesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "TrainingFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    TrainingQuality = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingFacilities_Teams_TeamId",
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
                name: "YouthAcademies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouthAcademies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YouthAcademies_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    OutcomeId = table.Column<int>(type: "int", nullable: false),
                    Commentary = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchEvents_EventOutcomes_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "EventOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchEvents_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchEvents_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchEvents_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Penalties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsScored = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalties_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Penalties_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Penalties_Teams_TeamId",
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
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    CompetitionId = table.Column<int>(type: "int", nullable: false),
                    Goals = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Goals = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompetitionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonStats_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                        principalColumn: "Id");
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
                name: "IX_Agencies_AgencyTemplateId",
                table: "Agencies",
                column: "AgencyTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_GameSaveId",
                table: "Agencies",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_RegionId",
                table: "Agencies",
                column: "RegionId");

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
                name: "IX_CommentaryTemplates_EventOutcomeId",
                table: "CommentaryTemplates",
                column: "EventOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_SeasonId",
                table: "Competitions",
                column: "SeasonId");

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
                name: "IX_Countries_RegionCode",
                table: "Countries",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_CupRounds_CupId",
                table: "CupRounds",
                column: "CupId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_CompetitionId",
                table: "Cups",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_CompetitionId1",
                table: "Cups",
                column: "CompetitionId1",
                unique: true,
                filter: "[CompetitionId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_CountryId",
                table: "Cups",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_GameSaveId",
                table: "Cups",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_SeasonId",
                table: "Cups",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_TemplateId",
                table: "Cups",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId",
                table: "CupTeams",
                column: "CupId");

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_TeamId",
                table: "CupTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupPhases_EuropeanCupId",
                table: "EuropeanCupPhases",
                column: "EuropeanCupId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupPhases_PhaseTemplateId",
                table: "EuropeanCupPhases",
                column: "PhaseTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupPhaseTemplates_EuropeanCupTemplateId",
                table: "EuropeanCupPhaseTemplates",
                column: "EuropeanCupTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupPhaseTemplates_Order",
                table: "EuropeanCupPhaseTemplates",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCups_CompetitionId",
                table: "EuropeanCups",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCups_CompetitionId1",
                table: "EuropeanCups",
                column: "CompetitionId1",
                unique: true,
                filter: "[CompetitionId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCups_GameSaveId",
                table: "EuropeanCups",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCups_SeasonId",
                table: "EuropeanCups",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCups_TemplateId",
                table: "EuropeanCups",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupStandings_EuropeanCupId_TeamId",
                table: "EuropeanCupStandings",
                columns: new[] { "EuropeanCupId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupStandings_TeamId",
                table: "EuropeanCupStandings",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupTeams_EuropeanCupId_TeamId",
                table: "EuropeanCupTeams",
                columns: new[] { "EuropeanCupId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EuropeanCupTeams_TeamId",
                table: "EuropeanCupTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttributeWeights_AttributeCode",
                table: "EventAttributeWeights",
                column: "AttributeCode");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttributeWeights_EventTypeCode",
                table: "EventAttributeWeights",
                column: "EventTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_EventOutcomes_EventTypeId",
                table: "EventOutcomes",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Code",
                table: "EventTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BankId",
                table: "FinancialTransactions",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FromAgencyId",
                table: "FinancialTransactions",
                column: "FromAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FromTeamId",
                table: "FinancialTransactions",
                column: "FromTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ToAgencyId",
                table: "FinancialTransactions",
                column: "ToAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ToTeamId",
                table: "FinancialTransactions",
                column: "ToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FirstNames_RegionCode",
                table: "FirstNames",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_AwayTeamId",
                table: "Fixtures",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_CupRoundId",
                table: "Fixtures",
                column: "CupRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_EuropeanCupPhaseId",
                table: "Fixtures",
                column: "EuropeanCupPhaseId");

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
                name: "IX_Fixtures_WinnerTeamId",
                table: "Fixtures",
                column: "WinnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSaves_UserId",
                table: "GameSaves",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSaves_UserTeamId",
                table: "GameSaves",
                column: "UserTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_LastNames_RegionCode",
                table: "LastNames",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CompetitionId",
                table: "Leagues",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CompetitionId1",
                table: "Leagues",
                column: "CompetitionId1",
                unique: true,
                filter: "[CompetitionId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CountryId",
                table: "Leagues",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_GameSaveId",
                table: "Leagues",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_SeasonId",
                table: "Leagues",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_TemplateId",
                table: "Leagues",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_GameSaveId",
                table: "LeagueStandings",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_LeagueId_TeamId",
                table: "LeagueStandings",
                columns: new[] { "LeagueId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_SeasonId",
                table: "LeagueStandings",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_TeamId",
                table: "LeagueStandings",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueTemplates_CountryId",
                table: "LeagueTemplates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CompetitionId",
                table: "Matches",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_FixtureId",
                table: "Matches",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_GameSaveId",
                table: "Matches",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_EventTypeId",
                table: "MatchEvents",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_MatchId",
                table: "MatchEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_OutcomeId",
                table: "MatchEvents",
                column: "OutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_PlayerId",
                table: "MatchEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_TeamId",
                table: "MatchEvents",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GameSaveId",
                table: "Messages",
                column: "GameSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GameSaveId1",
                table: "Messages",
                column: "GameSaveId1");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageTemplateId",
                table: "Messages",
                column: "MessageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_MatchId",
                table: "Penalties",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_PlayerId",
                table: "Penalties",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_TeamId",
                table: "Penalties",
                column: "TeamId");

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
                name: "IX_PlayerMatchStats_CompetitionId",
                table: "PlayerMatchStats",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_MatchId",
                table: "PlayerMatchStats",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_PlayerId",
                table: "PlayerMatchStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_AgencyId",
                table: "Players",
                column: "AgencyId");

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
                name: "IX_PlayerSeasonStats_CompetitionId",
                table: "PlayerSeasonStats",
                column: "CompetitionId");

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
                name: "IX_Regions_Code",
                table: "Regions",
                column: "Code",
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
                name: "IX_Stadiums_TeamId",
                table: "Stadiums",
                column: "TeamId",
                unique: true);

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
                name: "IX_TrainingFacilities_TeamId",
                table: "TrainingFacilities",
                column: "TeamId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_YouthAcademies_TeamId",
                table: "YouthAcademies",
                column: "TeamId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Agencies_GameSaves_GameSaveId",
                table: "Agencies",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_GameSaves_GameSaveId",
                table: "Banks",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Competitions_Seasons_SeasonId",
                table: "Competitions",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CupRounds_Cups_CupId",
                table: "CupRounds",
                column: "CupId",
                principalTable: "Cups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_GameSaves_GameSaveId",
                table: "Cups",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_Seasons_SeasonId",
                table: "Cups",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupTeams_Teams_TeamId",
                table: "CupTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EuropeanCupPhases_EuropeanCups_EuropeanCupId",
                table: "EuropeanCupPhases",
                column: "EuropeanCupId",
                principalTable: "EuropeanCups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EuropeanCups_GameSaves_GameSaveId",
                table: "EuropeanCups",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EuropeanCups_Seasons_SeasonId",
                table: "EuropeanCups",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EuropeanCupStandings_Teams_TeamId",
                table: "EuropeanCupStandings",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EuropeanCupTeams_Teams_TeamId",
                table: "EuropeanCupTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Fixtures_Teams_WinnerTeamId",
                table: "Fixtures",
                column: "WinnerTeamId",
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
                name: "FK_Seasons_GameSaves_GameSaveId",
                table: "Seasons");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_GameSaves_GameSaveId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_GameSaves_CurrentSaveId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CommentaryTemplates");

            migrationBuilder.DropTable(
                name: "CupTeams");

            migrationBuilder.DropTable(
                name: "EuropeanCupStandings");

            migrationBuilder.DropTable(
                name: "EuropeanCupTeams");

            migrationBuilder.DropTable(
                name: "EventAttributeWeights");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "FirstNames");

            migrationBuilder.DropTable(
                name: "LastNames");

            migrationBuilder.DropTable(
                name: "LeagueStandings");

            migrationBuilder.DropTable(
                name: "MatchEvents");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Penalties");

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
                name: "Stadiums");

            migrationBuilder.DropTable(
                name: "TeamTactics");

            migrationBuilder.DropTable(
                name: "TrainingFacilities");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "YouthAcademies");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "EventOutcomes");

            migrationBuilder.DropTable(
                name: "MessageTemplates");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropTable(
                name: "Tactics");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "Fixtures");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "CupRounds");

            migrationBuilder.DropTable(
                name: "EuropeanCupPhases");

            migrationBuilder.DropTable(
                name: "AgencyTemplates");

            migrationBuilder.DropTable(
                name: "Cups");

            migrationBuilder.DropTable(
                name: "EuropeanCupPhaseTemplates");

            migrationBuilder.DropTable(
                name: "EuropeanCups");

            migrationBuilder.DropTable(
                name: "CupTemplates");

            migrationBuilder.DropTable(
                name: "EuropeanCupTemplates");

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
                name: "Competitions");

            migrationBuilder.DropTable(
                name: "LeagueTemplates");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
