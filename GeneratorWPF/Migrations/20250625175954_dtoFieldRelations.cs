using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorWPF.Migrations
{
    /// <inheritdoc />
    public partial class dtoFieldRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PrimaryEntityVirPropName",
                table: "Relations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ForeignEntityVirPropName",
                table: "Relations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DtoFieldRelations",
                columns: table => new
                {
                    DtoFieldId = table.Column<int>(type: "int", nullable: false),
                    RelationId = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DtoFieldRelations", x => new { x.DtoFieldId, x.RelationId, x.SequenceNo });
                    table.ForeignKey(
                        name: "FK_DtoFieldRelations_DtoFields_DtoFieldId",
                        column: x => x.DtoFieldId,
                        principalTable: "DtoFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DtoFieldRelations_Relations_RelationId",
                        column: x => x.RelationId,
                        principalTable: "Relations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DtoFieldRelations_RelationId",
                table: "DtoFieldRelations",
                column: "RelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DtoFieldRelations");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryEntityVirPropName",
                table: "Relations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ForeignEntityVirPropName",
                table: "Relations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
