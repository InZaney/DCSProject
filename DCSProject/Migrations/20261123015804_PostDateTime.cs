using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCSProject.Migrations;

/// <inheritdoc />
public partial class _20261123015804_PostDateTime : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "DatePosted",
            table: "BlogPosts",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateOnly),
            oldType: "date");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateOnly>(
            name: "DatePosted",
            table: "BlogPosts",
            type: "date",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");
    }
}
