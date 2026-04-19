using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangeRateProvider.Infrastructure.Sql.Migrations.ExchangeRateProvider
{
    /// <inheritdoc />
    public partial class FixTableHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyExchangeRate_ConsumerPair_ConsumerPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyExchangeRate_ConsumerPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate");

            migrationBuilder.RenameColumn(
                name: "ConsumerPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                newName: "MarketTradingPairId");

            migrationBuilder.AddColumn<long>(
                name: "ConsumerId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRate_ConsumerId_MarketTradingPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                columns: new[] { "ConsumerId", "MarketTradingPairId" })
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurrencyExchangeRate_ConsumerId_MarketTradingPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate");

            migrationBuilder.DropColumn(
                name: "ConsumerId",
                schema: "dbo",
                table: "CurrencyExchangeRate");

            migrationBuilder.RenameColumn(
                name: "MarketTradingPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                newName: "ConsumerPairId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRate_ConsumerPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                column: "ConsumerPairId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyExchangeRate_ConsumerPair_ConsumerPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                column: "ConsumerPairId",
                principalSchema: "dbo",
                principalTable: "ConsumerPair",
                principalColumn: "Id");
        }
    }
}
