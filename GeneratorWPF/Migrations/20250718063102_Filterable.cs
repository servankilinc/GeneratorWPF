﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorWPF.Migrations
{
    /// <inheritdoc />
    public partial class Filterable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Filterable",
                table: "Fields",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Filterable",
                table: "Fields");
        }
    }
}
