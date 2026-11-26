using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class AddProviderTokenBudgets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LlmDailyUsage",
                table: "LlmDailyUsage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LlmDailyUsage_Counts",
                table: "LlmDailyUsage");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "LlmDailyUsage",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Gemini");

            migrationBuilder.AddColumn<long>(
                name: "ReservedTokens",
                table: "LlmDailyUsage",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "WindowReservedTokens",
                table: "LlmDailyUsage",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LlmDailyUsage",
                table: "LlmDailyUsage",
                columns: new[] { "Provider", "Date" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_LlmDailyUsage_Counts",
                table: "LlmDailyUsage",
                sql: "[RequestCount] >= 0 AND [WindowRequestCount] >= 0 AND [InputTokens] >= 0 AND [OutputTokens] >= 0 AND [ReservedTokens] >= 0 AND [WindowReservedTokens] >= 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [LlmDailyUsage] WHERE [Provider] <> N'Gemini') THROW 51000, N'Sağlayıcı kullanım kayıtları varken bu migration veri kaybı olmadan geri alınamaz.', 1;");
            migrationBuilder.DropPrimaryKey(
                name: "PK_LlmDailyUsage",
                table: "LlmDailyUsage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LlmDailyUsage_Counts",
                table: "LlmDailyUsage");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "LlmDailyUsage");

            migrationBuilder.DropColumn(
                name: "ReservedTokens",
                table: "LlmDailyUsage");

            migrationBuilder.DropColumn(
                name: "WindowReservedTokens",
                table: "LlmDailyUsage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LlmDailyUsage",
                table: "LlmDailyUsage",
                column: "Date");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LlmDailyUsage_Counts",
                table: "LlmDailyUsage",
                sql: "[RequestCount] >= 0 AND [WindowRequestCount] >= 0 AND [InputTokens] >= 0 AND [OutputTokens] >= 0");
        }
    }
}
