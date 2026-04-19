using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangeRateProvider.Infrastructure.Sql.Migrations.ExchangeRateProvider
{
    /// <inheritdoc />
    public partial class Add_RatingMethod_To_Market_And_TraidingPair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "RatingMethod",
                schema: "dbo",
                table: "MarketTradingPair",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "RatingMethod",
                schema: "dbo",
                table: "Market",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingMethod",
                schema: "dbo",
                table: "MarketTradingPair");

            migrationBuilder.DropColumn(
                name: "RatingMethod",
                schema: "dbo",
                table: "Market");
        }
    }
}
