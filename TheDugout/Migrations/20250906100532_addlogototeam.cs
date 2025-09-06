using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class addlogototeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoFileName",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoFileName",
                table: "Teams");
        }
    }
}
