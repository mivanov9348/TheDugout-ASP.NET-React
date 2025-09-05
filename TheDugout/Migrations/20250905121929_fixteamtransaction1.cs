using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class fixteamtransaction1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bank_GameSaves_GameSaveId",
                table: "Bank");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransaction_Bank_BankId",
                table: "FinancialTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransaction_Teams_FromTeamId",
                table: "FinancialTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransaction_Teams_ToTeamId",
                table: "FinancialTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinancialTransaction",
                table: "FinancialTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bank",
                table: "Bank");

            migrationBuilder.RenameTable(
                name: "FinancialTransaction",
                newName: "FinancialTransactions");

            migrationBuilder.RenameTable(
                name: "Bank",
                newName: "Banks");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransaction_ToTeamId",
                table: "FinancialTransactions",
                newName: "IX_FinancialTransactions_ToTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransaction_FromTeamId",
                table: "FinancialTransactions",
                newName: "IX_FinancialTransactions_FromTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransaction_BankId",
                table: "FinancialTransactions",
                newName: "IX_FinancialTransactions_BankId");

            migrationBuilder.RenameIndex(
                name: "IX_Bank_GameSaveId",
                table: "Banks",
                newName: "IX_Banks_GameSaveId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinancialTransactions",
                table: "FinancialTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Banks",
                table: "Banks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_GameSaves_GameSaveId",
                table: "Banks",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Banks_BankId",
                table: "FinancialTransactions",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banks_GameSaves_GameSaveId",
                table: "Banks");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Banks_BankId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Teams_FromTeamId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Teams_ToTeamId",
                table: "FinancialTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinancialTransactions",
                table: "FinancialTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Banks",
                table: "Banks");

            migrationBuilder.RenameTable(
                name: "FinancialTransactions",
                newName: "FinancialTransaction");

            migrationBuilder.RenameTable(
                name: "Banks",
                newName: "Bank");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransactions_ToTeamId",
                table: "FinancialTransaction",
                newName: "IX_FinancialTransaction_ToTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransactions_FromTeamId",
                table: "FinancialTransaction",
                newName: "IX_FinancialTransaction_FromTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransactions_BankId",
                table: "FinancialTransaction",
                newName: "IX_FinancialTransaction_BankId");

            migrationBuilder.RenameIndex(
                name: "IX_Banks_GameSaveId",
                table: "Bank",
                newName: "IX_Bank_GameSaveId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinancialTransaction",
                table: "FinancialTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bank",
                table: "Bank",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bank_GameSaves_GameSaveId",
                table: "Bank",
                column: "GameSaveId",
                principalTable: "GameSaves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransaction_Bank_BankId",
                table: "FinancialTransaction",
                column: "BankId",
                principalTable: "Bank",
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
    }
}
