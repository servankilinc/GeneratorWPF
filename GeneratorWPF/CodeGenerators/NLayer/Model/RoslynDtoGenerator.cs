using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using GeneratorWPF.Models.Statics;
using GeneratorWPF.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace GeneratorWPF.CodeGenerators.NLayer.Model;

public partial class RoslynDtoGenerator
{
    private readonly DtoFieldRepository _dtoFieldRepository;
    private readonly ValidationRepository _validationRepository;
    public RoslynDtoGenerator()
    {
        _dtoFieldRepository = new DtoFieldRepository();
        _validationRepository = new ValidationRepository();
    }

    public string GeneraterDto(Dto dto, AppSetting appSettings)
    {
        List<DtoField> dtoFieldList = _dtoFieldRepository.GetAll(
            filter: f => f.DtoId == dto.Id,
            include: i => i
                .Include(x => x.SourceField)
                    .ThenInclude(x => x.FieldType)
                .Include(x => x.SourceField)
                    .ThenInclude(x => x.Entity));

        var dtoFieldIdList = dtoFieldList.Select(x => x.Id);
        bool isExistValidation = _validationRepository.IsExist(f => dtoFieldIdList.Contains(f.DtoFieldId));


        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>();

        foreach (var dtoField in dtoFieldList)
        {
            var propertyDeclaration = GeneratorProperty(dtoField);
            propertyList.Add(propertyDeclaration);
        }
        // *** Check kind of this Dto is Create and Related Entity of dto is User Entity then add password property
        if (dto.CrudTypeId == (int)CrudTypeEnums.Create && dto.RelatedEntityId == appSettings.UserEntityId)
            propertyList.Add(GeneratorPropertyByName("string", "Password", true));


        // 2) Usings
        List<UsingDirectiveSyntax> usingsList = new List<UsingDirectiveSyntax>(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Model"))
        };

        if (dtoFieldList.Any(f => f.SourceField.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Dto))
        {
            List<int> addedSourceEntites = new() { dto.RelatedEntityId };
            foreach (var dtoField in dtoFieldList.Where(f => f.SourceField.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Dto))
            {
                if (addedSourceEntites.Any(f => f == dtoField.SourceField.EntityId)) continue;

                addedSourceEntites.Add(dtoField.SourceField.EntityId);
                usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{dtoField.SourceField.Entity.Name}_")));
            }
        }

        if (isExistValidation)
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("FluentValidation")));

        if (dtoFieldList.Any(f => f.SourceField.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Entity))
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities")));



        List<ClassDeclarationSyntax> classes = new List<ClassDeclarationSyntax>();

        // 3) Class of Dto
        classes.Add(SyntaxFactory
            .ClassDeclaration(dto.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IDto")))
            .AddMembers(propertyList.ToArray()));


        // 4) Class of Validator
        if (isExistValidation)
        {
            var dtoFieldsByValidation = _dtoFieldRepository.GetAll(
                filter: f => f.DtoId == dto.Id && f.Validations != null,
                include: i => i
                    .Include(x => x.Validations)!
                        .ThenInclude(x => x.ValidatorType)!
                    .Include(x => x.Validations)!
                        .ThenInclude(x => x.ValidationParams)!);

            classes.Add(GenerateValidatorCode(dtoFieldsByValidation, dto, appSettings));
        }

        // 5) Namespace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName($"Model.Dtos.{dto.RelatedEntity.Name}_"))
            .AddMembers(classes.ToArray());



        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterReportDto(Dto dto, AppSetting appSettings)
    {
        List<DtoField> dtoFieldList = _dtoFieldRepository.GetAll(
            filter: f => f.DtoId == dto.Id,
            include: i => i
                .Include(x => x.SourceField)
                    .ThenInclude(x => x.FieldType)
                .Include(x => x.SourceField)
                    .ThenInclude(x => x.Entity));


        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>();

        foreach (var dtoField in dtoFieldList)
        {
            var propertyDeclaration = GeneratorProperty(dtoField);
            propertyList.Add(propertyDeclaration);
        }

        if (dto.RelatedEntity.Auditable)
        {
            propertyList.Add(GeneratorPropertyByName("string", "CreatedBy", false));
            propertyList.Add(GeneratorPropertyByName("string", "UpdatedBy", false));
            propertyList.Add(GeneratorPropertyByName("DateTime", "CreateDateUtc", false));
            propertyList.Add(GeneratorPropertyByName("DateTime", "UpdateDateUtc", false));
        }
        if (dto.RelatedEntity.SoftDeletable)
        {
            propertyList.Add(GeneratorPropertyByName("string", "DeletedBy", false));
            propertyList.Add(GeneratorPropertyByName("bool", "IsDeleted", true));
            propertyList.Add(GeneratorPropertyByName("DateTime", "DeletedDateUtc", false));
        }


        // 2) Usings
        List<UsingDirectiveSyntax> usingsList = new List<UsingDirectiveSyntax>(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Model"))
        };

        if (dtoFieldList.Any(f => f.SourceField.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Dto))
        {
            List<int> addedSourceEntites = new() { dto.RelatedEntityId };
            foreach (var dtoField in dtoFieldList.Where(f => f.SourceField.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Dto))
            {
                if (addedSourceEntites.Any(f => f == dtoField.SourceField.EntityId)) continue;

                addedSourceEntites.Add(dtoField.SourceField.EntityId);
                usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{dtoField.SourceField.Entity.Name}_")));
            }
        }

        if (dtoFieldList.Any(f => f.SourceField.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Entity))
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities")));



        // 3) Class of Dto
        var classDecleration = SyntaxFactory
            .ClassDeclaration(dto.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IDto")))
            .AddMembers(propertyList.ToArray());


        // 5) Namespace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName($"Model.Dtos.{dto.RelatedEntity.Name}_"))
            .AddMembers(classDecleration);

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }



    #region Helpers
    private MemberDeclarationSyntax GeneratorProperty(DtoField dtoField)
    {
        Field field = dtoField.SourceField;
        string fieldTypeName =
            field.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Base ?
            field.MapFieldTypeName() : field.FieldType.Name;

        string mappedFieldTypeName = fieldTypeName;
        if (field.IsList)
        {
            mappedFieldTypeName = $"List<{mappedFieldTypeName}>";
        }
        if (!field.IsRequired && !dtoField.IsList)
        {
            mappedFieldTypeName = $"{mappedFieldTypeName}?";
        }

        string mappedDtoFieldTypeName = mappedFieldTypeName;
        if (dtoField.IsList)
        {
            mappedDtoFieldTypeName = $"List<{mappedDtoFieldTypeName}>";
        }
        if (!dtoField.IsRequired && dtoField.IsList && !mappedDtoFieldTypeName.EndsWith("?"))
        {
            mappedDtoFieldTypeName = $"{mappedDtoFieldTypeName}?";
        }

        var property = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(mappedDtoFieldTypeName), SyntaxFactory.Identifier(dtoField.Name))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        bool isRequiredReferanceType = dtoField.IsRequired && !Statics.nonReferanceTypes.Contains(fieldTypeName);
        if (isRequiredReferanceType)
        {
            property = property
                .WithInitializer(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.Token(SyntaxKind.EqualsToken),
                        SyntaxFactory.ParseExpression("null!")
                    )
                )
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        return property;
    }
    private MemberDeclarationSyntax GeneratorPropertyByName(string type, string name, bool isRequired)
    {
        if (!isRequired) type = $"{type}?";

        var property = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(type), SyntaxFactory.Identifier(name))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        bool isRequiredReferanceType = isRequired && !Statics.nonReferanceTypes.Contains(type);
        if (isRequiredReferanceType)
        {
            property = property
                .WithInitializer(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.Token(SyntaxKind.EqualsToken),
                        SyntaxFactory.ParseExpression("null!")
                    )
                )
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        return property;
    }

    private ClassDeclarationSyntax GenerateValidatorCode(List<DtoField> dtoFields, Dto dto, AppSetting appSettings)
    {
        var rules = new List<StatementSyntax>();

        foreach (var dtoField in dtoFields)
        {
            if (dtoField.Validations == null || !dtoField.Validations.Any()) continue;

            foreach (var validation in dtoField.Validations)
            {
                string rule = CreateRule(validation, validation.ErrorMessage);
                if (string.IsNullOrWhiteSpace(rule)) continue;

                var expression = SyntaxFactory.ParseStatement(
                    $"RuleFor(v => v.{dtoField.Name}){rule};"
                );

                rules.Add(expression);
            }

            rules.Add(SyntaxFactory.ParseStatement(""));
        }

        if (dto.CrudTypeId == (int)CrudTypeEnums.Create && dto.RelatedEntityId == appSettings.UserEntityId)
        {
            rules.Add(SyntaxFactory.ParseStatement(@"RuleFor(v => v.Password).NotNull().WithMessage(""Password cannot be null."");"));
            rules.Add(SyntaxFactory.ParseStatement(@"RuleFor(v => v.Password).MinimumLength(6).WithMessage(""Password must be at least 6 characters long."");"));
        }

        var constructor = SyntaxFactory.ConstructorDeclaration($"{dto.Name}Validator")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(rules));

        return SyntaxFactory.ClassDeclaration($"{dto.Name}Validator")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.GenericName("AbstractValidator")
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(dto.Name)
                                )
                            )
                        )
                )
            )
            .AddMembers(constructor);
    }

    private string CreateRule(Validation validation, string? message)
    {
        string rule = string.Empty;

        switch (validation.ValidatorTypeId)
        {
            case (int)ValidatorTypes.NotEmpty:
                if (string.IsNullOrEmpty(message)) message = "This field is required.";
                rule = $".NotEmpty().WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.NotNull:
                if (string.IsNullOrEmpty(message)) message = "This field cannot be null.";
                rule = $".NotNull().WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.NotEqual:
                var neqValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.NotEqual_Value)?.Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"The value cannot be {neqValue ?? "?"}.";
                rule = $".NotEqual({neqValue}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.MaxLength:
                var mxLValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.MaxLength_Max)?.Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"This field must be at most {mxLValue ?? "?"} characters long.";
                rule = $".MaximumLength({mxLValue}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.MinLength:
                var minLValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.MinLength_Min)?.Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"This field must be at least {minLValue ?? "?"} characters long.";
                rule = $".MinimumLength({minLValue}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Range:
                var rngMinValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.Range_Min)?.Value : "?";
                var rngMaxValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.Range_Max)?.Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"Value must be between {rngMinValue ?? "?"} and {rngMaxValue ?? "?"}";
                rule = $".InclusiveBetween({rngMinValue}, {rngMaxValue}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Regex:
                if (string.IsNullOrEmpty(message)) message = "The format of this field is invalid.";
                var rgPattern = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.Regex_Pattern)?.Value : "?";
                rule = $".Matches({rgPattern}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.GreaterThan:
                var gtValue = validation.ValidationParams != null ? validation.ValidationParams.First(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.GreaterThan_Value).Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"Value must be greater than {gtValue ?? "?"}";
                rule = $".GreaterThan({gtValue}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.LessThan:
                var ltValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.LessThan_Value)?.Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"Value must be less than {ltValue ?? "?"}";
                rule = $".LessThan({ltValue}).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.EmailAddress:
                if (string.IsNullOrEmpty(message)) message = "Please enter a valid email address.";
                rule = $".EmailAddress().WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.CreditCard:
                if (string.IsNullOrEmpty(message)) message = "Please enter a valid credit card number.";
                rule = $".CreditCard().WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Phone:
                if (string.IsNullOrEmpty(message)) message = "Please enter a valid phone number.";
                rule = $".Matches(@\"^\\+?\\d{{10,15}}$\").WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Url:
                if (string.IsNullOrEmpty(message)) message = "Please enter a valid URL.";
                rule = $".Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Date:
                if (string.IsNullOrEmpty(message)) message = "Please enter a valid date.";
                rule = $".Must(date => date != default).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Number:
                if (string.IsNullOrEmpty(message)) message = "Please enter a valid number.";
                rule = $".Must(amount => decimal.TryParse(amount.ToString(), out _)).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.GuidNotEmpty:
                if (string.IsNullOrEmpty(message)) message = "Field must be a valid guid value";
                rule = $".NotEqual(Guid.Empty).WithMessage(\"{message}\")";
                break;
            case (int)ValidatorTypes.Length:
                var lengthValue = validation.ValidationParams != null ? validation.ValidationParams.FirstOrDefault(f => f.ValidatorTypeParamId == (int)ValidatorTypeParams.Length_Value)?.Value : "?";
                if (string.IsNullOrEmpty(message)) message = $"This field must be {lengthValue ?? "?"} characters long.";
                rule = $".Length({lengthValue}).WithMessage(\"{message}\")";
                break;
            default:
                break;
        }

        return rule;
    }
    #endregion
}