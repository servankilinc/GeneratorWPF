using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorWPF.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ValidatorTypeParams",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.InsertData(
                table: "ValidatorTypeParams",
                columns: new[] { "Id", "Control", "Key", "ValidatorTypeId" },
                values: new object[] { 9, false, "Value", 17 });

            migrationBuilder.UpdateData(
                table: "ValidatorTypes",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "Field must have a exact number of characters\", \"Length");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ValidatorTypeParams",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.InsertData(
                table: "ValidatorTypeParams",
                columns: new[] { "Id", "Control", "Key", "ValidatorTypeId" },
                values: new object[] { 10, false, "Value", 17 });

            migrationBuilder.UpdateData(
                table: "ValidatorTypes",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "Field must have a exact number of characters");
        }
    }
}
