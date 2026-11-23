using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class AddPostingAnalysisSourceContentHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceContentHash",
                table: "PostingAnalyses",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceContentHash",
                table: "PostingAnalyses");
        }
    }
}
