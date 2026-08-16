using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MindLink.Infrastructure.Persistence;

#nullable disable

namespace MindLink.Infrastructure.Persistence.Migrations;

[DbContext(typeof(MindLinkDbContext))]
[Migration("20260812090000_AddUserPlan")]
public partial class AddUserPlan : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Plan",
            table: "Users",
            type: "TEXT",
            maxLength: 20,
            nullable: false,
            defaultValue: "Free");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Plan", table: "Users");
    }
}
