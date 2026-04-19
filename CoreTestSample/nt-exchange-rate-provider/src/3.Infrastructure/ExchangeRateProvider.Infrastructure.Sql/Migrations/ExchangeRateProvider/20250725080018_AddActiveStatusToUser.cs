using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangeRateProvider.Infrastructure.Sql.Migrations.ExchangeRateProvider
{
    /// <inheritdoc />
    public partial class AddActiveStatusToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Users");
        }
    }
}
