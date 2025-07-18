using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorWPF.Migrations
{
    /// <inheritdoc />
    public partial class validatorTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ValidatorTypes",
                columns: new[] { "Id", "Control", "Description", "Name" },
                values: new object[] { 17, false, "Field must have a exact number of characters", "Length" });

            migrationBuilder.InsertData(
                table: "ValidatorTypeParams",
                columns: new[] { "Id", "Control", "Key", "ValidatorTypeId" },
                values: new object[] { 10, false, "Value", 17 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ValidatorTypeParams",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ValidatorTypes",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
