using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class adddb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PotsCount",
                table: "EuropeanCupTemplates");

            migrationBuilder.DropColumn(
                name: "TeamsPerPot",
                table: "EuropeanCupTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PotsCount",
                table: "EuropeanCupTemplates",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "TeamsPerPot",
                table: "EuropeanCupTemplates",
                type: "int",
                nullable: false,
                defaultValue: 9);
        }
    }
}
