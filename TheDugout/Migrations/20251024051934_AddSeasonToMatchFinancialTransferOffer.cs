using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDugout.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonToMatchFinancialTransferOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "TransferOffers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferOffers_SeasonId",
                table: "TransferOffers",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SeasonId",
                table: "Matches",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SeasonId",
                table: "FinancialTransactions",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Seasons_SeasonId",
                table: "FinancialTransactions",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Seasons_SeasonId",
                table: "Matches",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOffers_Seasons_SeasonId",
                table: "TransferOffers",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Seasons_SeasonId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Seasons_SeasonId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferOffers_Seasons_SeasonId",
                table: "TransferOffers");

            migrationBuilder.DropIndex(
                name: "IX_TransferOffers_SeasonId",
                table: "TransferOffers");

            migrationBuilder.DropIndex(
                name: "IX_Matches_SeasonId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_SeasonId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "TransferOffers");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "FinancialTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "GameSaveId",
                table: "FinancialTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
