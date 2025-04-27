﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GeneratorWPF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldTypeSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldTypeSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceLayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceLayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValidatorTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidatorTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dtos_Entities_RelatedEntityId",
                        column: x => x.RelatedEntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FieldTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceTypeId = table.Column<int>(type: "int", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldTypes_FieldTypeSources_SourceTypeId",
                        column: x => x.SourceTypeId,
                        principalTable: "FieldTypeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceLayerId = table.Column<int>(type: "int", nullable: false),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Entities_RelatedEntityId",
                        column: x => x.RelatedEntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Services_ServiceLayers_ServiceLayerId",
                        column: x => x.ServiceLayerId,
                        principalTable: "ServiceLayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidatorTypeParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValidatorTypeId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidatorTypeParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidatorTypeParams_ValidatorTypes_ValidatorTypeId",
                        column: x => x.ValidatorTypeId,
                        principalTable: "ValidatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    FieldTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsUnique = table.Column<bool>(type: "bit", nullable: false),
                    IsList = table.Column<bool>(type: "bit", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fields_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fields_FieldTypes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "FieldTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Methods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MethodReturnFieldId = table.Column<int>(type: "int", nullable: true),
                    IsVoid = table.Column<bool>(type: "bit", nullable: false),
                    IsAsync = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Methods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Methods_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DtoFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DtoId = table.Column<int>(type: "int", nullable: false),
                    SourceFieldId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DtoFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DtoFields_Dtos_DtoId",
                        column: x => x.DtoId,
                        principalTable: "Dtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DtoFields_Fields_SourceFieldId",
                        column: x => x.SourceFieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrimaryFieldId = table.Column<int>(type: "int", nullable: false),
                    ForeignFieldId = table.Column<int>(type: "int", nullable: false),
                    RelationTypeId = table.Column<int>(type: "int", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relations_Fields_ForeignFieldId",
                        column: x => x.ForeignFieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relations_Fields_PrimaryFieldId",
                        column: x => x.PrimaryFieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relations_RelationTypes_RelationTypeId",
                        column: x => x.RelationTypeId,
                        principalTable: "RelationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MethodArgumentFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodId = table.Column<int>(type: "int", nullable: false),
                    FieldTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsList = table.Column<bool>(type: "bit", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodArgumentFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodArgumentFields_FieldTypes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "FieldTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MethodArgumentFields_Methods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "Methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MethodReturnFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodId = table.Column<int>(type: "int", nullable: false),
                    FieldTypeId = table.Column<int>(type: "int", nullable: false),
                    IsList = table.Column<bool>(type: "bit", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodReturnFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodReturnFields_FieldTypes_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "FieldTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MethodReturnFields_Methods_FieldTypeId",
                        column: x => x.FieldTypeId,
                        principalTable: "Methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Validations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DtoFieldId = table.Column<int>(type: "int", nullable: false),
                    ValidatorTypeId = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Validations_DtoFields_DtoFieldId",
                        column: x => x.DtoFieldId,
                        principalTable: "DtoFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Validations_ValidatorTypes_ValidatorTypeId",
                        column: x => x.ValidatorTypeId,
                        principalTable: "ValidatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidationParams",
                columns: table => new
                {
                    ValidationId = table.Column<int>(type: "int", nullable: false),
                    ValidatorTypeParamId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Control = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationParams", x => new { x.ValidationId, x.ValidatorTypeParamId });
                    table.ForeignKey(
                        name: "FK_ValidationParams_Validations_ValidationId",
                        column: x => x.ValidationId,
                        principalTable: "Validations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ValidationParams_ValidatorTypeParams_ValidatorTypeParamId",
                        column: x => x.ValidatorTypeParamId,
                        principalTable: "ValidatorTypeParams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "Control", "Path", "ProjectName", "SolutionName" },
                values: new object[] { 1, false, "C:\\Generator", "MyProject", "MyProject" });

            migrationBuilder.InsertData(
                table: "FieldTypeSources",
                columns: new[] { "Id", "Control", "Name" },
                values: new object[,]
                {
                    { 1, false, "Base" },
                    { 2, false, "Entity" },
                    { 3, false, "Dto" }
                });

            migrationBuilder.InsertData(
                table: "RelationTypes",
                columns: new[] { "Id", "Control", "Name" },
                values: new object[,]
                {
                    { 1, false, "OnoToOne" },
                    { 2, false, "OnoToMany" }
                });

            migrationBuilder.InsertData(
                table: "ServiceLayers",
                columns: new[] { "Id", "Control", "Name" },
                values: new object[,]
                {
                    { 1, false, "Core" },
                    { 2, false, "Model" },
                    { 3, false, "DataAccess" },
                    { 4, false, "Business" },
                    { 5, false, "Presentation" }
                });

            migrationBuilder.InsertData(
                table: "ValidatorTypes",
                columns: new[] { "Id", "Control", "Description", "Name" },
                values: new object[,]
                {
                    { 1, false, "Field cannot be empty", "NotEmpty" },
                    { 2, false, "Field cannot be null", "NotNull" },
                    { 3, false, "Field cannot be ...", "NotEqual" },
                    { 4, false, "Field cannot exceed maximum length", "MaxLength" },
                    { 5, false, "Value must be within a specific range", "Range" },
                    { 6, false, "Field must have a minimum number of characters", "MinLength" },
                    { 7, false, "Field must match a regular expression", "Regex" },
                    { 8, false, "Value must be greater than a specific number", "GreaterThan" },
                    { 9, false, "Value must be less than a specific number", "LessThan" },
                    { 10, false, "Field must be a valid email address", "EmailAddress" },
                    { 11, false, "Field must be a valid credit card number", "CreditCard" },
                    { 12, false, "Field must be a valid phone number", "Phone" },
                    { 13, false, "Field must be a valid URL", "Url" },
                    { 14, false, "Field must be a valid date", "Date" },
                    { 15, false, "Field must be a valid number", "Number" },
                    { 16, false, "Field mus be a valid guid value", "GuidNotEmpty" }
                });

            migrationBuilder.InsertData(
                table: "FieldTypes",
                columns: new[] { "Id", "Control", "Name", "SourceTypeId" },
                values: new object[,]
                {
                    { 1, false, "Int", 1 },
                    { 2, false, "String", 1 },
                    { 3, false, "Long", 1 },
                    { 4, false, "Float", 1 },
                    { 5, false, "Double", 1 },
                    { 6, false, "Bool", 1 },
                    { 7, false, "Char", 1 },
                    { 8, false, "Byte", 1 },
                    { 9, false, "DateTime", 1 },
                    { 10, false, "DateOnly", 1 },
                    { 11, false, "Guid", 1 }
                });

            migrationBuilder.InsertData(
                table: "ValidatorTypeParams",
                columns: new[] { "Id", "Control", "Key", "ValidatorTypeId" },
                values: new object[,]
                {
                    { 1, false, "Value", 3 },
                    { 2, false, "Max", 4 },
                    { 3, false, "Min", 5 },
                    { 4, false, "Max", 5 },
                    { 5, false, "Min", 6 },
                    { 6, false, "Pattern", 7 },
                    { 7, false, "Value", 8 },
                    { 8, false, "Value", 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DtoFields_DtoId",
                table: "DtoFields",
                column: "DtoId");

            migrationBuilder.CreateIndex(
                name: "IX_DtoFields_SourceFieldId",
                table: "DtoFields",
                column: "SourceFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Dtos_RelatedEntityId",
                table: "Dtos",
                column: "RelatedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_EntityId",
                table: "Fields",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_FieldTypeId",
                table: "Fields",
                column: "FieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldTypes_SourceTypeId",
                table: "FieldTypes",
                column: "SourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodArgumentFields_FieldTypeId",
                table: "MethodArgumentFields",
                column: "FieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodArgumentFields_MethodId",
                table: "MethodArgumentFields",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodReturnFields_FieldTypeId",
                table: "MethodReturnFields",
                column: "FieldTypeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MethodReturnFields_MethodId",
                table: "MethodReturnFields",
                column: "MethodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Methods_ServiceId",
                table: "Methods",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Relations_ForeignFieldId",
                table: "Relations",
                column: "ForeignFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Relations_PrimaryFieldId_ForeignFieldId",
                table: "Relations",
                columns: new[] { "PrimaryFieldId", "ForeignFieldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relations_RelationTypeId",
                table: "Relations",
                column: "RelationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_RelatedEntityId",
                table: "Services",
                column: "RelatedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceLayerId_RelatedEntityId",
                table: "Services",
                columns: new[] { "ServiceLayerId", "RelatedEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValidationParams_ValidatorTypeParamId",
                table: "ValidationParams",
                column: "ValidatorTypeParamId");

            migrationBuilder.CreateIndex(
                name: "IX_Validations_DtoFieldId",
                table: "Validations",
                column: "DtoFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Validations_ValidatorTypeId",
                table: "Validations",
                column: "ValidatorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidatorTypeParams_ValidatorTypeId",
                table: "ValidatorTypeParams",
                column: "ValidatorTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "MethodArgumentFields");

            migrationBuilder.DropTable(
                name: "MethodReturnFields");

            migrationBuilder.DropTable(
                name: "Relations");

            migrationBuilder.DropTable(
                name: "ValidationParams");

            migrationBuilder.DropTable(
                name: "Methods");

            migrationBuilder.DropTable(
                name: "RelationTypes");

            migrationBuilder.DropTable(
                name: "Validations");

            migrationBuilder.DropTable(
                name: "ValidatorTypeParams");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "DtoFields");

            migrationBuilder.DropTable(
                name: "ValidatorTypes");

            migrationBuilder.DropTable(
                name: "ServiceLayers");

            migrationBuilder.DropTable(
                name: "Dtos");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "FieldTypes");

            migrationBuilder.DropTable(
                name: "FieldTypeSources");
        }
    }
}
