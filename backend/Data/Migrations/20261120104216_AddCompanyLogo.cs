using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajFlow.Api.Data.Migrations
{
    public partial class AddCompanyLogo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "CompanyProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LogoData",
                table: "CompanyProfiles",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoVersion",
                table: "CompanyProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompanyProfiles_Logo",
                table: "CompanyProfiles",
                sql: "([LogoData] IS NULL AND [LogoContentType] IS NULL AND [LogoVersion] IS NULL) OR ([LogoData] IS NOT NULL AND DATALENGTH([LogoData]) BETWEEN 1 AND 2097152 AND [LogoContentType] IS NOT NULL AND [LogoContentType] IN ('image/png','image/jpeg') AND [LogoVersion] IS NOT NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CompanyProfiles_Logo",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "LogoData",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "LogoVersion",
                table: "CompanyProfiles");
        }
    }
}
