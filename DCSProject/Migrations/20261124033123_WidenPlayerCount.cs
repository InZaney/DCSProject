using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCSProject.Migrations;

/// <inheritdoc />
public partial class _20261124033123_WidenPlayerCount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "PlayerCount",
            table: "Games",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(75)",
            oldMaxLength: 75);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "PlayerCount",
            table: "Games",
            type: "nvarchar(75)",
            maxLength: 75,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);
    }
}
