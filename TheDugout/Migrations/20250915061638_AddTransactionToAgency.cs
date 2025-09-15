using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionToAgency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FromAgencyId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToAgencyId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Focus",
                table: "Agencies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Popularity",
                table: "Agencies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FromAgencyId",
                table: "FinancialTransactions",
                column: "FromAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ToAgencyId",
                table: "FinancialTransactions",
                column: "ToAgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Agencies_FromAgencyId",
                table: "FinancialTransactions",
                column: "FromAgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Agencies_ToAgencyId",
                table: "FinancialTransactions",
                column: "ToAgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Agencies_FromAgencyId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Agencies_ToAgencyId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_FromAgencyId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_ToAgencyId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "FromAgencyId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "ToAgencyId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "Focus",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Agencies");
        }
    }
}
