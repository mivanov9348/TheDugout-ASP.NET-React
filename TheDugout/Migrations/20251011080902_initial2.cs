using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_GameSaves_GameSaveId1",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_GameSaveId1",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "GameSaveId1",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "GameSaveId1",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GameSaveId1",
                table: "Messages",
                column: "GameSaveId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_GameSaves_GameSaveId1",
                table: "Messages",
                column: "GameSaveId1",
                principalTable: "GameSaves",
                principalColumn: "Id");
        }
    }
}
