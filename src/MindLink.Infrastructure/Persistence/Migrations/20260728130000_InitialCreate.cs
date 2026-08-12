using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MindLink.Infrastructure.Persistence;

#nullable disable

namespace MindLink.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MindLinkDbContext))]
[Migration("20260728130000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ActivityLogs",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(maxLength: 160, nullable: false),
                Detail = table.Column<string>(maxLength: 300, nullable: false),
                Category = table.Column<string>(maxLength: 40, nullable: false),
                OccurredAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ActivityLogs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                FullName = table.Column<string>(maxLength: 120, nullable: false),
                Email = table.Column<string>(maxLength: 160, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 128, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ActivityLogs");
        migrationBuilder.DropTable(name: "Users");
    }
}
