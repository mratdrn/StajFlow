using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class AddAnalysisDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Applications_Id_ResumeId_InternshipPostingId",
                table: "Applications",
                columns: new[] { "Id", "ResumeId", "InternshipPostingId" });

            migrationBuilder.CreateTable(
                name: "PostingAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternshipPostingId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SourceRowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingAnalyses", x => x.Id);
                    table.UniqueConstraint("AK_PostingAnalyses_Id_InternshipPostingId", x => new { x.Id, x.InternshipPostingId });
                    table.CheckConstraint("CK_PostingAnalyses_ErrorCode", "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled')");
                    table.CheckConstraint("CK_PostingAnalyses_Json", "[DataJson] IS NULL OR ISJSON([DataJson]) = 1");
                    table.CheckConstraint("CK_PostingAnalyses_SourceRowVersion", "DATALENGTH([SourceRowVersion]) = 8");
                    table.CheckConstraint("CK_PostingAnalyses_State", "([Status] = 'Pending' AND [StartedAt] IS NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR ([Status] = 'Processing' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR ([Status] = 'Completed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NOT NULL AND [ErrorCode] IS NULL) OR ([Status] = 'Failed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NULL AND [ErrorCode] IS NOT NULL)");
                    table.CheckConstraint("CK_PostingAnalyses_Timestamps", "([StartedAt] IS NULL OR [StartedAt] >= [CreatedAt]) AND ([CompletedAt] IS NULL OR [CompletedAt] >= [StartedAt])");
                    table.CheckConstraint("CK_PostingAnalyses_Version", "[Version] > 0 AND LEN(LTRIM(RTRIM([SchemaVersion]))) > 0");
                    table.ForeignKey(
                        name: "FK_PostingAnalyses_InternshipPostings_InternshipPostingId",
                        column: x => x.InternshipPostingId,
                        principalTable: "InternshipPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResumeAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SourceContentHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumeAnalyses", x => x.Id);
                    table.UniqueConstraint("AK_ResumeAnalyses_Id_ResumeId", x => new { x.Id, x.ResumeId });
                    table.CheckConstraint("CK_ResumeAnalyses_ErrorCode", "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled')");
                    table.CheckConstraint("CK_ResumeAnalyses_Json", "[DataJson] IS NULL OR ISJSON([DataJson]) = 1");
                    table.CheckConstraint("CK_ResumeAnalyses_SourceContentHash", "[SourceContentHash] IS NULL OR LEN([SourceContentHash]) = 64");
                    table.CheckConstraint("CK_ResumeAnalyses_State", "([Status] = 'Pending' AND [StartedAt] IS NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR ([Status] = 'Processing' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR ([Status] = 'Completed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NOT NULL AND [ErrorCode] IS NULL) OR ([Status] = 'Failed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NULL AND [ErrorCode] IS NOT NULL)");
                    table.CheckConstraint("CK_ResumeAnalyses_Timestamps", "([StartedAt] IS NULL OR [StartedAt] >= [CreatedAt]) AND ([CompletedAt] IS NULL OR [CompletedAt] >= [StartedAt])");
                    table.CheckConstraint("CK_ResumeAnalyses_Version", "[Version] > 0 AND LEN(LTRIM(RTRIM([SchemaVersion]))) > 0");
                    table.ForeignKey(
                        name: "FK_ResumeAnalyses_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternshipApplicationId = table.Column<int>(type: "int", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    InternshipPostingId = table.Column<int>(type: "int", nullable: false),
                    ResumeAnalysisId = table.Column<int>(type: "int", nullable: true),
                    PostingAnalysisId = table.Column<int>(type: "int", nullable: true),
                    SchemaVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ScoringRuleVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    UnavailableReason = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationEvaluations", x => x.Id);
                    table.CheckConstraint("CK_ApplicationEvaluations_Json", "ISJSON([ResultJson]) = 1");
                    table.CheckConstraint("CK_ApplicationEvaluations_Score", "[Score] IS NULL OR [Score] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_ApplicationEvaluations_State", "([Status] = 'Calculated' AND [Score] IS NOT NULL AND [UnavailableReason] IS NULL AND [ResumeAnalysisId] IS NOT NULL AND [PostingAnalysisId] IS NOT NULL) OR ([Status] = 'NotCalculated' AND [Score] IS NULL AND [UnavailableReason] IS NOT NULL)");
                    table.CheckConstraint("CK_ApplicationEvaluations_UnavailableReason", "[UnavailableReason] IS NULL OR [UnavailableReason] IN ('AnalysisPending','AnalysisFailed','InsufficientData','NoMeasurableCriteria')");
                    table.CheckConstraint("CK_ApplicationEvaluations_Versions", "LEN(LTRIM(RTRIM([SchemaVersion]))) > 0 AND LEN(LTRIM(RTRIM([ScoringRuleVersion]))) > 0");
                    table.ForeignKey(
                        name: "FK_ApplicationEvaluations_Applications_InternshipApplicationId_ResumeId_InternshipPostingId",
                        columns: x => new { x.InternshipApplicationId, x.ResumeId, x.InternshipPostingId },
                        principalTable: "Applications",
                        principalColumns: new[] { "Id", "ResumeId", "InternshipPostingId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationEvaluations_PostingAnalyses_PostingAnalysisId_InternshipPostingId",
                        columns: x => new { x.PostingAnalysisId, x.InternshipPostingId },
                        principalTable: "PostingAnalyses",
                        principalColumns: new[] { "Id", "InternshipPostingId" });
                    table.ForeignKey(
                        name: "FK_ApplicationEvaluations_ResumeAnalyses_ResumeAnalysisId_ResumeId",
                        columns: x => new { x.ResumeAnalysisId, x.ResumeId },
                        principalTable: "ResumeAnalyses",
                        principalColumns: new[] { "Id", "ResumeId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEvaluations_InternshipApplicationId_ResumeId_InternshipPostingId",
                table: "ApplicationEvaluations",
                columns: new[] { "InternshipApplicationId", "ResumeId", "InternshipPostingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEvaluations_PostingAnalysisId_InternshipPostingId",
                table: "ApplicationEvaluations",
                columns: new[] { "PostingAnalysisId", "InternshipPostingId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEvaluations_ResumeAnalysisId_ResumeId",
                table: "ApplicationEvaluations",
                columns: new[] { "ResumeAnalysisId", "ResumeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PostingAnalyses_ActiveSource",
                table: "PostingAnalyses",
                columns: new[] { "InternshipPostingId", "SourceRowVersion" },
                unique: true,
                filter: "[Status] IN ('Pending','Processing')");

            migrationBuilder.CreateIndex(
                name: "IX_PostingAnalyses_InternshipPostingId_Version",
                table: "PostingAnalyses",
                columns: new[] { "InternshipPostingId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResumeAnalyses_Active",
                table: "ResumeAnalyses",
                column: "ResumeId",
                unique: true,
                filter: "[Status] IN ('Pending','Processing')");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeAnalyses_ResumeId_Version",
                table: "ResumeAnalyses",
                columns: new[] { "ResumeId", "Version" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationEvaluations");

            migrationBuilder.DropTable(
                name: "PostingAnalyses");

            migrationBuilder.DropTable(
                name: "ResumeAnalyses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Applications_Id_ResumeId_InternshipPostingId",
                table: "Applications");
        }
    }
}
