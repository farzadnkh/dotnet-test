using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangeRateProvider.Infrastructure.Sql.Migrations.ExchangeRateProvider
{
    /// <inheritdoc />
    public partial class DataBaseInitializer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatorIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true),
                    DeleterUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consumer",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Apikey = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consumer_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consumer_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Consumer_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    DecimalPrecision = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Currency_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Currency_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRateProviders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRateProviders_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeRateProviders_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserBlockHistories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRefId = table.Column<int>(type: "int", nullable: false),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlockHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBlockHistories_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBlockHistories_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "dbo",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Market",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseCurrencyId = table.Column<int>(type: "int", nullable: false),
                    CalculationTerm = table.Column<byte>(type: "tinyint", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    LowerLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpreadEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpperLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Market", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Market_Currency_BaseCurrencyId",
                        column: x => x.BaseCurrencyId,
                        principalSchema: "dbo",
                        principalTable: "Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Market_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "dbo",
                        principalTable: "Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Market_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Market_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsumerProvider",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerProvider", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumerProvider_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalSchema: "dbo",
                        principalTable: "Consumer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerProvider_ExchangeRateProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "dbo",
                        principalTable: "ExchangeRateProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerProvider_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumerProvider_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRateProviderApiAccount",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    ProtocolType = table.Column<int>(type: "int", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Credentials = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateProviderApiAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRateProviderApiAccount_ExchangeRateProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "dbo",
                        principalTable: "ExchangeRateProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExchangeRateProviderApiAccount_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeRateProviderApiAccount_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProviderBusinessLogic",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderBusinessLogic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderBusinessLogic_ExchangeRateProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "dbo",
                        principalTable: "ExchangeRateProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProviderBusinessLogic_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderBusinessLogic_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsumerMarket",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerId = table.Column<int>(type: "int", nullable: false),
                    MarketId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    LowerLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpreadEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpperLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerMarket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumerMarket_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalSchema: "dbo",
                        principalTable: "Consumer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerMarket_Market_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Market",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerMarket_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumerMarket_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MarketCurrencies",
                schema: "dbo",
                columns: table => new
                {
                    MarketId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketCurrencies", x => new { x.MarketId, x.CurrencyId });
                    table.ForeignKey(
                        name: "FK_MarketCurrencies_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "dbo",
                        principalTable: "Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketCurrencies_Market_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Market",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MarketProviders",
                schema: "dbo",
                columns: table => new
                {
                    MarketId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRateProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketProviders", x => new { x.MarketId, x.ExchangeRateProviderId });
                    table.ForeignKey(
                        name: "FK_MarketProviders_ExchangeRateProviders_ExchangeRateProviderId",
                        column: x => x.ExchangeRateProviderId,
                        principalSchema: "dbo",
                        principalTable: "ExchangeRateProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketProviders_Market_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Market",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MarketTradingPair",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    LowerLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpreadEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpperLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketTradingPair", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketTradingPair_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "dbo",
                        principalTable: "Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketTradingPair_Market_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Market",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketTradingPair_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketTradingPair_Users_DeleterUserId",
                        column: x => x.DeleterUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketTradingPair_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsumerPair",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerId = table.Column<int>(type: "int", nullable: false),
                    MarketId = table.Column<int>(type: "int", nullable: false),
                    PairId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    LowerLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpreadEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpperLimitPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifierUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerPair", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumerPair_Consumer_ConsumerId",
                        column: x => x.ConsumerId,
                        principalSchema: "dbo",
                        principalTable: "Consumer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerPair_MarketTradingPair_PairId",
                        column: x => x.PairId,
                        principalSchema: "dbo",
                        principalTable: "MarketTradingPair",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerPair_Market_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Market",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumerPair_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumerPair_Users_LastModifierUserId",
                        column: x => x.LastModifierUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MarketTradingPairProviders",
                schema: "dbo",
                columns: table => new
                {
                    MarektTradingPairId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRateProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketTradingPairProviders", x => new { x.MarektTradingPairId, x.ExchangeRateProviderId });
                    table.ForeignKey(
                        name: "FK_MarketTradingPairProviders_ExchangeRateProviders_ExchangeRateProviderId",
                        column: x => x.ExchangeRateProviderId,
                        principalSchema: "dbo",
                        principalTable: "ExchangeRateProviders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketTradingPairProviders_MarketTradingPair_MarektTradingPairId",
                        column: x => x.MarektTradingPairId,
                        principalSchema: "dbo",
                        principalTable: "MarketTradingPair",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurrencyExchangeRate",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerPairId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Buy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BuyRateChange = table.Column<int>(type: "int", nullable: false),
                    Sell = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellRateChange = table.Column<int>(type: "int", nullable: false),
                    UpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyExchangeRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyExchangeRate_ConsumerPair_ConsumerPairId",
                        column: x => x.ConsumerPairId,
                        principalSchema: "dbo",
                        principalTable: "ConsumerPair",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_CreatorUserId",
                schema: "dbo",
                table: "Consumer",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_LastModifierUserId",
                schema: "dbo",
                table: "Consumer",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumer_UserId",
                schema: "dbo",
                table: "Consumer",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerMarket_ConsumerId",
                schema: "dbo",
                table: "ConsumerMarket",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerMarket_CreatorUserId",
                schema: "dbo",
                table: "ConsumerMarket",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerMarket_LastModifierUserId",
                schema: "dbo",
                table: "ConsumerMarket",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerMarket_MarketId",
                schema: "dbo",
                table: "ConsumerMarket",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerPair_ConsumerId",
                schema: "dbo",
                table: "ConsumerPair",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerPair_CreatorUserId",
                schema: "dbo",
                table: "ConsumerPair",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerPair_LastModifierUserId",
                schema: "dbo",
                table: "ConsumerPair",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerPair_MarketId",
                schema: "dbo",
                table: "ConsumerPair",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerPair_PairId",
                schema: "dbo",
                table: "ConsumerPair",
                column: "PairId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerProvider_ConsumerId",
                schema: "dbo",
                table: "ConsumerProvider",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerProvider_CreatorUserId",
                schema: "dbo",
                table: "ConsumerProvider",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerProvider_LastModifierUserId",
                schema: "dbo",
                table: "ConsumerProvider",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerProvider_ProviderId",
                schema: "dbo",
                table: "ConsumerProvider",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Currency_Code",
                schema: "dbo",
                table: "Currency",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currency_CreatorUserId",
                schema: "dbo",
                table: "Currency",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Currency_LastModifierUserId",
                schema: "dbo",
                table: "Currency",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRate_ConsumerPairId",
                schema: "dbo",
                table: "CurrencyExchangeRate",
                column: "ConsumerPairId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateProviderApiAccount_CreatorUserId",
                schema: "dbo",
                table: "ExchangeRateProviderApiAccount",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateProviderApiAccount_LastModifierUserId",
                schema: "dbo",
                table: "ExchangeRateProviderApiAccount",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateProviderApiAccount_ProviderId",
                schema: "dbo",
                table: "ExchangeRateProviderApiAccount",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateProviders_CreatorUserId",
                schema: "dbo",
                table: "ExchangeRateProviders",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateProviders_LastModifierUserId",
                schema: "dbo",
                table: "ExchangeRateProviders",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Market_BaseCurrencyId",
                schema: "dbo",
                table: "Market",
                column: "BaseCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Market_CreatorUserId",
                schema: "dbo",
                table: "Market",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Market_CurrencyId",
                schema: "dbo",
                table: "Market",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Market_LastModifierUserId",
                schema: "dbo",
                table: "Market",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketCurrencies_CurrencyId",
                schema: "dbo",
                table: "MarketCurrencies",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketProviders_ExchangeRateProviderId",
                schema: "dbo",
                table: "MarketProviders",
                column: "ExchangeRateProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTradingPair_CreatorUserId",
                schema: "dbo",
                table: "MarketTradingPair",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTradingPair_CurrencyId",
                schema: "dbo",
                table: "MarketTradingPair",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTradingPair_DeleterUserId",
                schema: "dbo",
                table: "MarketTradingPair",
                column: "DeleterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTradingPair_LastModifierUserId",
                schema: "dbo",
                table: "MarketTradingPair",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTradingPair_MarketId",
                schema: "dbo",
                table: "MarketTradingPair",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTradingPairProviders_ExchangeRateProviderId",
                schema: "dbo",
                table: "MarketTradingPairProviders",
                column: "ExchangeRateProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBusinessLogic_CreatorUserId",
                schema: "dbo",
                table: "ProviderBusinessLogic",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBusinessLogic_LastModifierUserId",
                schema: "dbo",
                table: "ProviderBusinessLogic",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderBusinessLogic_ProviderId",
                schema: "dbo",
                table: "ProviderBusinessLogic",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "dbo",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "dbo",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_UserId",
                schema: "dbo",
                table: "Settings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlockHistories_CreatorUserId",
                schema: "dbo",
                table: "UserBlockHistories",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlockHistories_UserId",
                schema: "dbo",
                table: "UserBlockHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "dbo",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "dbo",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "dbo",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "dbo",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "dbo",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumerMarket",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConsumerProvider",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CurrencyExchangeRate",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ExchangeRateProviderApiAccount",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MarketCurrencies",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MarketProviders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MarketTradingPairProviders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProviderBusinessLogic",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserBlockHistories",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConsumerPair",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ExchangeRateProviders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Consumer",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MarketTradingPair",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Market",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Currency",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");
        }
    }
}
