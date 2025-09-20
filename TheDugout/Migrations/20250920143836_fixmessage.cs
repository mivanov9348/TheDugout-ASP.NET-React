using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class fixmessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageTemplatePlaceholders_MessageTemplates_MessageTemplateId",
                table: "MessageTemplatePlaceholders");

            migrationBuilder.DropIndex(
                name: "IX_MessageTemplatePlaceholders_MessageTemplateId",
                table: "MessageTemplatePlaceholders");

            migrationBuilder.DropColumn(
                name: "Sender",
                table: "MessageTemplates");

            migrationBuilder.DropColumn(
                name: "MessageTemplateId",
                table: "MessageTemplatePlaceholders");

            migrationBuilder.DropColumn(
                name: "Sender",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Messages",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<int>(
                name: "SenderType",
                table: "MessageTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "MessageTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SenderType",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderType",
                table: "MessageTemplates");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "MessageTemplates");

            migrationBuilder.DropColumn(
                name: "SenderType",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Messages",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "Sender",
                table: "MessageTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MessageTemplateId",
                table: "MessageTemplatePlaceholders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sender",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplatePlaceholders_MessageTemplateId",
                table: "MessageTemplatePlaceholders",
                column: "MessageTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageTemplatePlaceholders_MessageTemplates_MessageTemplateId",
                table: "MessageTemplatePlaceholders",
                column: "MessageTemplateId",
                principalTable: "MessageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
