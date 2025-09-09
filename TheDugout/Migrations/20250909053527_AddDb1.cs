using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class AddDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Progress",
                table: "PlayerAttributes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progress",
                table: "PlayerAttributes");
        }
    }
}
