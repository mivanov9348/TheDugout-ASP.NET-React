using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addEuropeanCupRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EuropeanCupTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Ranking",
                table: "EuropeanCupTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EuropeanCups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Ranking",
                table: "EuropeanCups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EuropeanCupTemplates");

            migrationBuilder.DropColumn(
                name: "Ranking",
                table: "EuropeanCupTemplates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EuropeanCups");

            migrationBuilder.DropColumn(
                name: "Ranking",
                table: "EuropeanCups");
        }
    }
}
