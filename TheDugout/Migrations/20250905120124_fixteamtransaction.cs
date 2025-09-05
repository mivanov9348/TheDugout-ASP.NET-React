using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class fixteamtransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransaction_Teams_TeamId",
                table: "FinancialTransaction");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "FinancialTransaction",
                newName: "ToTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransaction_TeamId",
                table: "FinancialTransaction",
                newName: "IX_FinancialTransaction_ToTeamId");

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "GameSaves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FinancialTransaction",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "FinancialTransaction",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FromTeamId",
                table: "FinancialTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FinancialTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GameSaveId",
                table: "Bank",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransaction_FromTeamId",
                table: "FinancialTransaction",
                column: "FromTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Bank_GameSaveId",
                table: "Bank",
                column: "GameSaveId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bank_GameSaves_GameSaveId",
                table: "Bank",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransaction_Teams_FromTeamId",
                table: "FinancialTransaction",
                column: "FromTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransaction_Teams_ToTeamId",
                table: "FinancialTransaction",
                column: "ToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bank_GameSaves_GameSaveId",
                table: "Bank");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransaction_Teams_FromTeamId",
                table: "FinancialTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransaction_Teams_ToTeamId",
                table: "FinancialTransaction");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransaction_FromTeamId",
                table: "FinancialTransaction");

            migrationBuilder.DropIndex(
                name: "IX_Bank_GameSaveId",
                table: "Bank");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "GameSaves");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "FinancialTransaction");

            migrationBuilder.DropColumn(
                name: "FromTeamId",
                table: "FinancialTransaction");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FinancialTransaction");

            migrationBuilder.DropColumn(
                name: "GameSaveId",
                table: "Bank");

            migrationBuilder.RenameColumn(
                name: "ToTeamId",
                table: "FinancialTransaction",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransaction_ToTeamId",
                table: "FinancialTransaction",
                newName: "IX_FinancialTransaction_TeamId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FinancialTransaction",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransaction_Teams_TeamId",
                table: "FinancialTransaction",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
