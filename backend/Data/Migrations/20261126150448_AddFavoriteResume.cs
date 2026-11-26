using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class AddFavoriteResume : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Resumes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                WITH RankedResumes AS (
                    SELECT [Id], ROW_NUMBER() OVER (
                        PARTITION BY [StudentProfileId] ORDER BY [UploadedAt], [Id]
                    ) AS [Position]
                    FROM [Resumes]
                )
                UPDATE resume SET [IsFavorite] = 1
                FROM [Resumes] AS resume
                INNER JOIN RankedResumes AS ranked ON ranked.[Id] = resume.[Id]
                WHERE ranked.[Position] = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_StudentProfileId_Favorite",
                table: "Resumes",
                column: "StudentProfileId",
                unique: true,
                filter: "[IsFavorite] = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resumes_StudentProfileId_Favorite",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Resumes");
        }
    }
}
