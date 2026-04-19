using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangeRateProvider.Infrastructure.Sql.Migrations.ExchangeRateProvider
{
    /// <inheritdoc />
    public partial class AddWhiteListIp_to_Consumer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhiteListIps",
                schema: "dbo",
                table: "Consumer",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhiteListIps",
                schema: "dbo",
                table: "Consumer");
        }
    }
}
