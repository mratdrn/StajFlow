using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations;

public partial class AddPostingWorkArrangement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "WorkArrangement",
            table: "InternshipPostings",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.Sql("UPDATE [InternshipPostings] SET [WorkArrangement] = CASE WHEN [IsRemote] = 1 THEN 'Remote' ELSE 'OnSite' END;");

        migrationBuilder.AlterColumn<string>(
            name: "WorkArrangement",
            table: "InternshipPostings",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldNullable: true);

        migrationBuilder.DropColumn(name: "IsRemote", table: "InternshipPostings");

        migrationBuilder.AddCheckConstraint(
            name: "CK_InternshipPostings_WorkArrangement",
            table: "InternshipPostings",
            sql: "[WorkArrangement] IN ('OnSite','Hybrid','Remote')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(name: "CK_InternshipPostings_WorkArrangement", table: "InternshipPostings");
        migrationBuilder.AddColumn<bool>(name: "IsRemote", table: "InternshipPostings", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.Sql("UPDATE [InternshipPostings] SET [IsRemote] = CASE WHEN [WorkArrangement] = 'Remote' THEN 1 ELSE 0 END;");
        migrationBuilder.DropColumn(name: "WorkArrangement", table: "InternshipPostings");
    }
}
