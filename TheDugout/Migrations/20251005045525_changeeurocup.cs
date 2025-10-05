using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class changeeurocup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentPhaseOrder",
                table: "EuropeanCupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEliminated",
                table: "EuropeanCupTeams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlayoffParticipant",
                table: "EuropeanCupTeams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "EuropeanCups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsQualificationPhase",
                table: "EuropeanCupPhases",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPhaseOrder",
                table: "EuropeanCupTeams");

            migrationBuilder.DropColumn(
                name: "IsEliminated",
                table: "EuropeanCupTeams");

            migrationBuilder.DropColumn(
                name: "IsPlayoffParticipant",
                table: "EuropeanCupTeams");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "EuropeanCups");

            migrationBuilder.DropColumn(
                name: "IsQualificationPhase",
                table: "EuropeanCupPhases");
        }
    }
}
