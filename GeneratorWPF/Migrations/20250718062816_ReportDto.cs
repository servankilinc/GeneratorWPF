using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorWPF.Migrations
{
    /// <inheritdoc />
    public partial class ReportDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportDtoId",
                table: "Entities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entities_ReportDtoId",
                table: "Entities",
                column: "ReportDtoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_Dtos_ReportDtoId",
                table: "Entities",
                column: "ReportDtoId",
                principalTable: "Dtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entities_Dtos_ReportDtoId",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_ReportDtoId",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "ReportDtoId",
                table: "Entities");
        }
    }
}
