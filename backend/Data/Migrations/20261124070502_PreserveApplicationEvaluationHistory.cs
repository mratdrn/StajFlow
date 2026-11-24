using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class PreserveApplicationEvaluationHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_StudentProfileId_InternshipPostingId",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "RequiredCriteriaStatus",
                table: "ApplicationEvaluations",
                type: "varchar(24)",
                unicode: false,
                maxLength: 24,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StudentProfileId_InternshipPostingId",
                table: "Applications",
                columns: new[] { "StudentProfileId", "InternshipPostingId" },
                unique: true,
                filter: "[Status] <> 'Withdrawn'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ApplicationEvaluations_RequiredCriteriaStatus",
                table: "ApplicationEvaluations",
                sql: "[RequiredCriteriaStatus] IS NULL OR [RequiredCriteriaStatus] IN ('AllMatched','MissingEvidence','NoRequiredCriteria','Unavailable')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM [Applications]
                    GROUP BY [StudentProfileId], [InternshipPostingId]
                    HAVING COUNT_BIG(*) > 1
                )
                    THROW 51000, N'Başvuru geçmişi varken eski benzersizlik kuralı veri korunarak geri yüklenemez.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Applications_StudentProfileId_InternshipPostingId",
                table: "Applications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ApplicationEvaluations_RequiredCriteriaStatus",
                table: "ApplicationEvaluations");

            migrationBuilder.DropColumn(
                name: "RequiredCriteriaStatus",
                table: "ApplicationEvaluations");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StudentProfileId_InternshipPostingId",
                table: "Applications",
                columns: new[] { "StudentProfileId", "InternshipPostingId" },
                unique: true);
        }
    }
}
