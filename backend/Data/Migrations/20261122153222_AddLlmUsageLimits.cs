using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class AddLlmUsageLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ResumeAnalyses_ErrorCode",
                table: "ResumeAnalyses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PostingAnalyses_ErrorCode",
                table: "PostingAnalyses");

            migrationBuilder.CreateTable(
                name: "LlmDailyUsage",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestCount = table.Column<int>(type: "int", nullable: false),
                    WindowStartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    WindowRequestCount = table.Column<int>(type: "int", nullable: false),
                    BlockedUntil = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    InputTokens = table.Column<long>(type: "bigint", nullable: false),
                    OutputTokens = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmDailyUsage", x => x.Date);
                    table.CheckConstraint("CK_LlmDailyUsage_Counts", "[RequestCount] >= 0 AND [WindowRequestCount] >= 0 AND [InputTokens] >= 0 AND [OutputTokens] >= 0");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ResumeAnalyses_ErrorCode",
                table: "ResumeAnalyses",
                sql: "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled','AuthenticationFailed','ModelUnavailable','QuotaExceeded','InputTooLong','ConfigurationError','ContentBlocked')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PostingAnalyses_ErrorCode",
                table: "PostingAnalyses",
                sql: "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled','AuthenticationFailed','ModelUnavailable','QuotaExceeded','InputTooLong','ConfigurationError','ContentBlocked')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmDailyUsage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ResumeAnalyses_ErrorCode",
                table: "ResumeAnalyses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PostingAnalyses_ErrorCode",
                table: "PostingAnalyses");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ResumeAnalyses_ErrorCode",
                table: "ResumeAnalyses",
                sql: "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PostingAnalyses_ErrorCode",
                table: "PostingAnalyses",
                sql: "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled')");
        }
    }
}
