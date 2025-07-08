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

public partial class RoslynAuthModelsGenerator
{
    private readonly EntityRepository _entityRepository;
    private readonly FieldRepository _fieldRepository;
    private readonly AppSetting _appSetting;
    public RoslynAuthModelsGenerator(AppSetting appSetting)
    {
        _entityRepository = new();
        _fieldRepository = new();
        _appSetting = appSetting;
    }

    #region Login Models
    public string GeneraterLoginResponse()
    {
        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("IList<string>?", "Roles", false),
            GenerateProperty("AccessToken", "AccessToken", true)
        };

        // 2) Find user property model type
        Entity? userEntity = null;
        if (_appSetting.UserEntityId != null)
        {
            userEntity = _entityRepository.Get(f => f.Id == _appSetting.UserEntityId, include: i => i.Include(x => x.Dtos).ThenInclude(ti => ti.DtoFields));
            if (userEntity != null)
            {
                if (userEntity.Dtos == null)
                {
                    propertyList.Add(GenerateProperty(userEntity.Name, "User", true));
                }
                else
                {
                    var baseRespDto = userEntity.BasicResponseDtoId != default ? userEntity.Dtos.FirstOrDefault(f => f.Id == userEntity.BasicResponseDtoId) : default;
                    var detailRespDto = userEntity.DetailResponseDtoId != default ? userEntity.Dtos.FirstOrDefault(f => f.Id == userEntity.DetailResponseDtoId) : default;
                    var firstReadDto = userEntity.Dtos.FirstOrDefault(f => f.CrudTypeId == (int)CrudTypeEnums.Read);

                    if (baseRespDto != default)
                        propertyList.Add(GenerateProperty(baseRespDto.Name, "User", true));
                    else if (detailRespDto != default)
                        propertyList.Add(GenerateProperty(detailRespDto.Name, "User", true));
                    else if (firstReadDto != default)
                        propertyList.Add(GenerateProperty(firstReadDto.Name, "User", true));
                    else
                        propertyList.Add(GenerateProperty(userEntity.Name, "User", true));
                }
            }
        }


        // 3) Class-1 LoginResponse
        var classDeclaration_LoginResponse = SyntaxFactory
            .ClassDeclaration("LoginResponse")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 4) Class-2 LoginTrustedResponse
        var classDeclaration_LoginTrustedResponse = SyntaxFactory
            .ClassDeclaration("LoginTrustedResponse")
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("LoginResponse")))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(GenerateProperty("string", "RefreshToken", true));

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Auth.Login"))
            .AddMembers(classDeclaration_LoginResponse)
            .AddMembers(classDeclaration_LoginTrustedResponse);

        // 6) Usings
        List<UsingDirectiveSyntax> usingsList = new(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Auth"))
        };

        if (userEntity != null)
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{userEntity.Name}_")));

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterLoginRequest()
    {
        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("string", "Email", true),
            GenerateProperty("string", "Password", true)
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("CriticalData"))
                    ))
                )
        };


        // 2) Class-1 LoginRequest
        var classDeclaration = SyntaxFactory
            .ClassDeclaration("LoginRequest")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 3) Class-2 Validator
        var validatorClassDeclaration = GenerateValidatorCode("LoginRequest", new string[]
        {
            @"RuleFor(b => b.Email).NotNull().EmailAddress().NotEmpty().EmailAddress();",
            @"RuleFor(b => b.Password).NotNull().MinimumLength(6).NotEmpty();"
        });

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Auth.Login"))
            .AddMembers(classDeclaration)
            .AddMembers(validatorClassDeclaration);

        // 6) Usings
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Core.Utils.CriticalData")),
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("FluentValidation"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }
    #endregion

    #region Refresh Auth
    public string GeneraterRefreshAuthResponse()
    {
        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("AccessToken", "AccessToken", true)
        };

        // 2) Find user property model type
        Entity? userEntity = null;
        if (_appSetting.UserEntityId != null)
        {
            userEntity = _entityRepository.Get(f => f.Id == _appSetting.UserEntityId, include: i => i.Include(x => x.Dtos).ThenInclude(ti => ti.DtoFields));
            if (userEntity != null)
            {
                if (userEntity.Dtos == null)
                {
                    propertyList.Add(GenerateProperty(userEntity.Name, "User", true));
                }
                else
                {
                    var baseRespDto = userEntity.BasicResponseDtoId != default ? userEntity.Dtos.FirstOrDefault(f => f.Id == userEntity.BasicResponseDtoId) : default;
                    var detailRespDto = userEntity.DetailResponseDtoId != default ? userEntity.Dtos.FirstOrDefault(f => f.Id == userEntity.DetailResponseDtoId) : default;
                    var firstReadDto = userEntity.Dtos.FirstOrDefault(f => f.CrudTypeId == (int)CrudTypeEnums.Read);

                    if (baseRespDto != default)
                        propertyList.Add(GenerateProperty(baseRespDto.Name, "User", true));
                    else if (detailRespDto != default)
                        propertyList.Add(GenerateProperty(detailRespDto.Name, "User", true));
                    else if (firstReadDto != default)
                        propertyList.Add(GenerateProperty(firstReadDto.Name, "User", true));
                    else
                        propertyList.Add(GenerateProperty(userEntity.Name, "User", true));
                }
            }
        }


        // 3) Class-1 RefreshAuthResponse
        var classDeclaration_RefreshAuthResponse = SyntaxFactory
            .ClassDeclaration("RefreshAuthResponse")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 4) Class-2 RefreshAuthTrustedResponse
        var classDeclaration_RefreshAuthTrustedResponse = SyntaxFactory
            .ClassDeclaration("RefreshAuthTrustedResponse")
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("RefreshAuthResponse")))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(GenerateProperty("string", "RefreshToken", true));

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Auth.RefreshAuth"))
            .AddMembers(classDeclaration_RefreshAuthResponse)
            .AddMembers(classDeclaration_RefreshAuthTrustedResponse);

        // 6) Usings
        List<UsingDirectiveSyntax> usingsList = new(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Auth"))
        };

        if (userEntity != null)
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{userEntity.Name}_")));

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterRefreshAuthRequest()
    {
        var ruleListOfValidation = new List<string>()
        {
            @"When(b => b.IsTrusted, () => RuleFor(b => b.RefreshToken).NotNull().NotEmpty());"
        };

        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("bool", "IsTrusted", true),
            GenerateProperty("string", "RefreshToken", true)
        };
        if (_appSetting.UserEntityId != null)
        {
            var uniqueFields = _fieldRepository.GetAll(f => f.EntityId == _appSetting.UserEntityId && f.IsUnique);
            if (uniqueFields != null)
            {
                if (uniqueFields.Count == 1)
                {
                    ruleListOfValidation.Add(@"RuleFor(b => b.UserId).NotNull().NotEmpty();");
                    string fieldType = uniqueFields.First().MapFieldTypeName();
                    propertyList.Add(GenerateProperty(fieldType, "UserId", true));
                }
                else
                {
                    foreach (var uf in uniqueFields)
                    {
                        ruleListOfValidation.Add($"RuleFor(b => b.{uf.Name}).NotNull().NotEmpty();");
                        propertyList.Add(GenerateProperty(uf.MapFieldTypeName(), uf.Name, true));
                    }
                }
            }
        }


        // 2) Class-1 LoginRequest
        var classDeclaration = SyntaxFactory
            .ClassDeclaration("RefreshAuthRequest")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 3) Class-2 Validator
        var validatorClassDeclaration = GenerateValidatorCode("RefreshAuthRequest", ruleListOfValidation.ToArray());

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Auth.RefreshAuth"))
            .AddMembers(classDeclaration)
            .AddMembers(validatorClassDeclaration);

        // 6) Usings
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("FluentValidation"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }
    #endregion

    #region Signup Models
    public string GeneraterSignUpResponse()
    {
        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("IList<string>?", "Roles", false),
            GenerateProperty("AccessToken", "AccessToken", true)
        };

        // 2) Find user property model type
        Entity? userEntity = null;
        if (_appSetting.UserEntityId != null)
        {
            userEntity = _entityRepository.Get(f => f.Id == _appSetting.UserEntityId, include: i => i.Include(x => x.Dtos).ThenInclude(ti => ti.DtoFields));
            if (userEntity != null)
            {
                if (userEntity.Dtos == null)
                {
                    propertyList.Add(GenerateProperty(userEntity.Name, "User", true));
                }
                else
                {
                    var baseRespDto = userEntity.BasicResponseDtoId != default ? userEntity.Dtos.FirstOrDefault(f => f.Id == userEntity.BasicResponseDtoId) : default;
                    var detailRespDto = userEntity.DetailResponseDtoId != default ? userEntity.Dtos.FirstOrDefault(f => f.Id == userEntity.DetailResponseDtoId) : default;
                    var firstReadDto = userEntity.Dtos.FirstOrDefault(f => f.CrudTypeId == (int)CrudTypeEnums.Read);

                    if (baseRespDto != default)
                        propertyList.Add(GenerateProperty(baseRespDto.Name, "User", true));
                    else if (detailRespDto != default)
                        propertyList.Add(GenerateProperty(detailRespDto.Name, "User", true));
                    else if (firstReadDto != default)
                        propertyList.Add(GenerateProperty(firstReadDto.Name, "User", true));
                    else
                        propertyList.Add(GenerateProperty(userEntity.Name, "User", true));
                }
            }
        }


        // 3) Class-1 SignUpResponse
        var classDeclaration_SignUpResponse = SyntaxFactory
            .ClassDeclaration("SignUpResponse")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 4) Class-2 SignUpTrustedResponse
        var classDeclaration_SignUpTrustedResponse = SyntaxFactory
            .ClassDeclaration("SignUpTrustedResponse")
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("SignUpResponse")))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(GenerateProperty("string", "RefreshToken", true));

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Auth.SignUp"))
            .AddMembers(classDeclaration_SignUpResponse)
            .AddMembers(classDeclaration_SignUpTrustedResponse);

        // 6) Usings
        List<UsingDirectiveSyntax> usingsList = new(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Auth"))
        };

        if (userEntity != null)
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{userEntity.Name}_")));

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterSignUpRequest()
    {
        var ruleListOfValidation = new List<string>()
        {
            @"RuleFor(b => b.Email).NotNull().EmailAddress().NotEmpty().EmailAddress();",
            @"RuleFor(b => b.Password).NotNull().MinimumLength(6).NotEmpty();"
        };

        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("string", "Email", true),
            GenerateProperty("string", "Password", true)
        };
        if (_appSetting.UserEntityId != null)
        {
            var userFields = _fieldRepository.GetAll(f => f.EntityId == _appSetting.UserEntityId, include: i => i.Include(x => x.FieldType));
            if (userFields != null)
            {
                foreach (var uf in userFields.Where(f => !f.IsUnique && f.IsRequired && f.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Base))
                {
                    ruleListOfValidation.Add($"RuleFor(b => b.{uf.Name}).NotNull().NotEmpty();");
                    propertyList.Add(GenerateProperty(uf.MapFieldTypeName(), uf.Name, true));
                }
            }
        }


        // 2) Class-1 LoginRequest
        var classDeclaration = SyntaxFactory
            .ClassDeclaration("SignUpRequest")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 3) Class-2 Validator
        var validatorClassDeclaration = GenerateValidatorCode("SignUpRequest", ruleListOfValidation.ToArray());

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Auth.SignUp"))
            .AddMembers(classDeclaration)
            .AddMembers(validatorClassDeclaration);

        // 6) Usings
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("FluentValidation"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }
    #endregion

    public string GeneraterRefreshTokenEntity()
    {
        string typeOfUserKey = "int";
        string typeOfUser = "IdentityUser";
        Entity? userEntity = null;
        if (_appSetting.IsThereUser)
        {
            userEntity = _entityRepository.Get(f => f.Id == _appSetting.UserEntityId, include: i => i.Include(x => x.Fields).ThenInclude(ti => ti.FieldType));
            if (userEntity != null)
            {
                var uField = userEntity.Fields.FirstOrDefault(f => f.IsUnique);
                if (uField != null) typeOfUserKey = uField.MapFieldTypeName();
                typeOfUser = userEntity.Name; 
            }
        }

        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>()
        {
            GenerateProperty("Guid", "Id", true),
            GenerateProperty(typeOfUserKey, "UserId", true),
            GenerateProperty("string", "IpAddress", true),
            GenerateProperty("string", "Token", true),
            GenerateProperty("DateTime", "ExpirationUtc", true),
            GenerateProperty("DateTime", "CreateDateUtc", true),
            GenerateProperty("int", "TTL", true),
            GenerateProperty($"virtual {typeOfUser}?", "User", false),
        };

        // 3) Class-1 RefreshToken
        var classDeclaration = SyntaxFactory
            .ClassDeclaration("RefreshToken")
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("IEntity")))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Entities"))
            .AddMembers(classDeclaration);

        // 6) Usings
        List<UsingDirectiveSyntax> usingsList = new(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Model"))
        };

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }


    #region Helpers
    private MemberDeclarationSyntax GenerateProperty(string type, string name, bool? required = false)
    {
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

        bool isRequiredReferanceType = required == true && !Statics.nonReferanceTypes.Contains(type);
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

    private ClassDeclarationSyntax GenerateValidatorCode(string modelName, string[] ruleList)
    {
        var rules = new List<StatementSyntax>();

        foreach (var item in ruleList)
        {
            rules.Add(SyntaxFactory.ParseStatement(item));
        }

        var constructor = SyntaxFactory.ConstructorDeclaration($"{modelName}Validator")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(rules));

        return SyntaxFactory.ClassDeclaration($"{modelName}Validator")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.GenericName("AbstractValidator")
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(modelName)
                                )
                            )
                        )
                )
            )
            .AddMembers(constructor);
    }
    #endregion
}
