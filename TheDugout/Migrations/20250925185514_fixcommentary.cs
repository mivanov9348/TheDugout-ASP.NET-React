using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class fixcommentary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentaryTemplates_EventOutcomes_EventOutcomeId",
                table: "CommentaryTemplates");

            migrationBuilder.AlterColumn<int>(
                name: "EventOutcomeId",
                table: "CommentaryTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentaryTemplates_EventOutcomes_EventOutcomeId",
                table: "CommentaryTemplates",
                column: "EventOutcomeId",
                principalTable: "EventOutcomes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentaryTemplates_EventOutcomes_EventOutcomeId",
                table: "CommentaryTemplates");

            migrationBuilder.AlterColumn<int>(
                name: "EventOutcomeId",
                table: "CommentaryTemplates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentaryTemplates_EventOutcomes_EventOutcomeId",
                table: "CommentaryTemplates",
                column: "EventOutcomeId",
                principalTable: "EventOutcomes",
                principalColumn: "Id");
        }
    }
}
