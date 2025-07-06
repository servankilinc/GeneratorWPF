using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using GeneratorWPF.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GeneratorWPF.CodeGenerators.NLayer.Business;

public partial class RoslynBusinessServiceGenerator
{
    private readonly AppSetting _appSetting;
    private readonly DtoFieldRepository _dtoFieldRepository;
    private readonly DtoFieldRelationsRepository _dtoFieldRelationsRepository;
    public RoslynBusinessServiceGenerator(AppSetting appSetting)
    {
        _appSetting = appSetting;
        _dtoFieldRepository = new DtoFieldRepository();
        _dtoFieldRelationsRepository = new DtoFieldRelationsRepository();
    }

    public string GeneraterAbstract(Entity entity, List<Dto> dtos)
    {
        List<Field> uniqueFields = entity.Fields.Where(f => f.IsUnique).ToList();

        // 1) Method List
        var methodList = new List<MethodDeclarationSyntax>();

        #region GetBasic
        var basicResponseDto = dtos.FirstOrDefault(f => f.Id == entity.BasicResponseDtoId);
        bool isThereBasicResponseDto = basicResponseDto != null;
        if (isThereBasicResponseDto)
        {
            methodList.Add(GeneratorGetMethodOfAbstract(basicResponseDto!.Name, "GetAsync", uniqueFields));
            methodList.Add(GeneratorGetAllMethodOfAbstract(basicResponseDto.Name, "GetAllAsync"));
            methodList.Add(GeneratorGetListMethodOfAbstract(basicResponseDto.Name, "GetListAsync"));
        }
        #endregion

        #region GetDetail
        var detailResponseDto = dtos.FirstOrDefault(f => f.Id == entity.DetailResponseDtoId);
        if (detailResponseDto != null)
        {
            methodList.Add(GeneratorGetMethodOfAbstract(detailResponseDto!.Name, "GetByDetailAsync", uniqueFields));
            methodList.Add(GeneratorGetAllMethodOfAbstract(detailResponseDto.Name, "GetAllByDetailAsync"));
            methodList.Add(GeneratorGetListMethodOfAbstract(detailResponseDto.Name, "GetListByDetailAsync"));
        }
        #endregion

        #region Other Read Dtos
        var readResponseDtos = dtos.Where(f => f.CrudTypeId == (int)CrudTypeEnums.Read && f.Id != entity.BasicResponseDtoId && f.Id != entity.DetailResponseDtoId);
        if (readResponseDtos != null && readResponseDtos.Any())
        {
            foreach (var readDto in readResponseDtos)
            {
                methodList.Add(GeneratorGetMethodOfAbstract(readDto!.Name, $"Get{readDto.Name}Async", uniqueFields));
                methodList.Add(GeneratorGetAllMethodOfAbstract(readDto.Name, $"GetAll{readDto.Name}Async"));
                methodList.Add(GeneratorGetListMethodOfAbstract(readDto.Name, $"GetList{readDto.Name}Async"));
            }
        }
        #endregion

        #region Create
        var createDto = dtos.FirstOrDefault(f => f.Id == entity.CreateDtoId);

        string createArgType = createDto != null ? createDto.Name : entity.Name;
        string createReturnType = isThereBasicResponseDto ? basicResponseDto!.Name : entity.Name;

        methodList.Add(GeneratorCreateMethodOfAbstract(createReturnType, createArgType));
        #endregion

        #region Update
        var updateDto = dtos.FirstOrDefault(f => f.Id == entity.UpdateDtoId);

        string updateArgType = updateDto != null ? updateDto.Name : entity.Name;
        string updateReturnType = isThereBasicResponseDto ? basicResponseDto!.Name : entity.Name;

        methodList.Add(GeneratorUpdateMethodOfAbstract(updateReturnType, updateArgType));
        #endregion

        #region Delete
        var deleteDto = dtos.FirstOrDefault(f => f.Id == entity.DeleteDtoId);
        bool isThereDeleteDto = deleteDto != null;

        string deleteArgType = isThereDeleteDto ? deleteDto!.Name : string.Empty;

        methodList.Add(GeneratorDeleteMethodOfAbstract(deleteArgType, isThereDeleteDto, uniqueFields));
        #endregion

        #region Datatable Methods
        methodList.Add(GeneratorDatatableClientSideMethodOfAbstract(entity.Name));
        methodList.Add(GeneratorDatatableServerSideMethodOfAbstract(entity.Name));
        #endregion

        // 2) Interface
        var InterfaceDeclaration = SyntaxFactory
            .InterfaceDeclaration($"I{entity.Name}Service")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IServiceBase<{entity.Name}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IServiceBaseAsync<{entity.Name}>")))
            .AddMembers(methodList.ToArray());

        // 3) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Business.Abstract"))
            .AddMembers(InterfaceDeclaration);

        // 4) Compilation Unit
        var usings = new List<UsingDirectiveSyntax>()
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Business.ServiceBase")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.BaseRequestModels")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Datatable")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Pagination")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities"))
        };
        if (dtos != null && dtos.Any())
        {
            usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{entity.Name}_")));
        }

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usings.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterConcrete(Entity entity, List<Dto> dtos)
    {
        string serviceName = $"{entity.Name}Service";
        string repoTypeName = $"I{entity.Name}Repositoty";
        string repoArgName = $"{entity.Name}Repositoty".ToCamelCase();

        List<Field> uniqueFields = entity.Fields.Where(f => f.IsUnique).ToList();

        // 1) Attribute List
        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ExceptionHandler"))
            )
        );

        // 2) Method List
        var methodList = new List<MethodDeclarationSyntax>();

        #region GetBasic
        var basicResponseDto = dtos.FirstOrDefault(f => f.Id == entity.BasicResponseDtoId);
        bool isThereBasicResponseDto = basicResponseDto != null;
        if (isThereBasicResponseDto)
        {
            var dtoFieldIdsOfBasicResponse = basicResponseDto.DtoFields.Select(f => f.Id).ToList();
            var isThereIncludeOfBasicResponse = _dtoFieldRelationsRepository.IsExist(f => dtoFieldIdsOfBasicResponse.Contains(f.DtoFieldId));

            methodList.Add(GeneratorGetMethodOfConcrete("GetAsync", basicResponseDto!, uniqueFields, isThereIncludeOfBasicResponse));
            methodList.Add(GeneratorGetAllMethodOfConcrete("GetAllAsync", basicResponseDto, isThereIncludeOfBasicResponse));
            methodList.Add(GeneratorGetListMethodOfConcrete("GetListAsync", basicResponseDto, isThereIncludeOfBasicResponse));
        }
        #endregion

        #region GetDetail
        var detailResponseDto = dtos.FirstOrDefault(f => f.Id == entity.DetailResponseDtoId);
        if (detailResponseDto != null)
        {
            var dtoFieldIdsOfDetailResponse = detailResponseDto.DtoFields.Select(f => f.Id).ToList();
            var isThereIncludeOfDetailResponse = _dtoFieldRelationsRepository.IsExist(f => dtoFieldIdsOfDetailResponse.Contains(f.DtoFieldId));

            methodList.Add(GeneratorGetMethodOfConcrete("GetByDetailAsync", detailResponseDto!, uniqueFields, isThereIncludeOfDetailResponse));
            methodList.Add(GeneratorGetAllMethodOfConcrete("GetAllByDetailAsync", detailResponseDto, isThereIncludeOfDetailResponse));
            methodList.Add(GeneratorGetListMethodOfConcrete("GetListByDetailAsync", detailResponseDto, isThereIncludeOfDetailResponse));
        }
        #endregion

        #region Other Read Dtos
        var readResponseDtos = dtos.Where(f => f.CrudTypeId == (int)CrudTypeEnums.Read && f.Id != entity.BasicResponseDtoId && f.Id != entity.DetailResponseDtoId);
        if (readResponseDtos != null && readResponseDtos.Any())
        {
            foreach (var readDto in readResponseDtos)
            {
                var dtoFieldIdsOfReadDto = readDto.DtoFields.Select(f => f.Id).ToList();
                var isThereIncludeOfReadDto = _dtoFieldRelationsRepository.IsExist(f => dtoFieldIdsOfReadDto.Contains(f.DtoFieldId));

                methodList.Add(GeneratorGetMethodOfConcrete($"Get{readDto.Name}Async", readDto!, uniqueFields, isThereIncludeOfReadDto));
                methodList.Add(GeneratorGetAllMethodOfConcrete($"GetAll{readDto.Name}Async", readDto, isThereIncludeOfReadDto));
                methodList.Add(GeneratorGetListMethodOfConcrete($"GetList{readDto.Name}Async", readDto, isThereIncludeOfReadDto));
            }
        }
        #endregion

        #region Create
        methodList.Add(GeneratorCreateMethodOfConcrete(entity, dtos));
        #endregion

        #region Update
        methodList.Add(GeneratorUpdateMethodOfConcrete(entity, dtos, uniqueFields));
        #endregion

        #region Delete
        methodList.Add(GeneratorDeleteMethodOfConcrete(entity, dtos, uniqueFields));
        #endregion

        #region Datatable Methods
        methodList.Add(GeneratorDatatableClientSideMethodOfConcrete(entity));
        methodList.Add(GeneratorDatatableServerSideMethodOfConcrete(entity));
        #endregion


        // 3) Constructor
        var constructor = SyntaxFactory.ConstructorDeclaration(serviceName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
             .AddParameterListParameters(
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier(repoArgName)).WithType(SyntaxFactory.ParseTypeName(repoTypeName)),
                 SyntaxFactory.Parameter(SyntaxFactory.Identifier("IMapper")).WithType(SyntaxFactory.ParseTypeName("mapper"))
             )
             .WithInitializer(
                 SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                     .AddArgumentListArguments(
                         SyntaxFactory.Argument(SyntaxFactory.IdentifierName(repoArgName)),
                         SyntaxFactory.Argument(SyntaxFactory.IdentifierName("mapper")))
             )
             .WithBody(SyntaxFactory.Block());

        // 4) Class
        var classDeclaration = SyntaxFactory
            .ClassDeclaration(serviceName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"ServiceBase<{entity.Name}, {repoTypeName}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{serviceName}"))
            )
            .AddMembers(constructor)
            .AddMembers(methodList.ToArray());

        // 5) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Business.Concrete"))
            .AddMembers(classDeclaration);

        // 6) Compilation Unit
        var usings = new List<UsingDirectiveSyntax>()
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("AutoMapper")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Business.Abstract")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Business.ServiceBase")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.BaseRequestModels")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.CrossCuttingConcerns")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Datatable")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Pagination")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities"))
        };
        if (dtos != null && dtos.Any())
        {
            usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{entity.Name}_")));
        }

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usings.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }



    #region Helpers Abstract
    private MethodDeclarationSyntax GeneratorGetMethodOfAbstract(string dtoName, string name, List<Field> uniqueFields)
    {
        var paramList = new List<ParameterSyntax>();

        foreach (var field in uniqueFields)
        {
            paramList.Add(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(field.Name.ToCamelCase()))
                    .WithType(SyntaxFactory.IdentifierName(field.GetMapedTypeName()))
            );
        }

        paramList.Add(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        );

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{dtoName}?>"), SyntaxFactory.Identifier(name))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private MethodDeclarationSyntax GeneratorGetAllMethodOfAbstract(string dtoName, string name)
    {
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicRequest?")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<ICollection<{dtoName}>?>"), SyntaxFactory.Identifier(name))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private MethodDeclarationSyntax GeneratorGetListMethodOfAbstract(string dtoName, string name)
    {
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicPaginationRequest")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<PaginationResponse<{dtoName}>>"), SyntaxFactory.Identifier(name))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }


    private MethodDeclarationSyntax GeneratorCreateMethodOfAbstract(string retrunName, string argName)
    {
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName(argName)
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{retrunName}>"), SyntaxFactory.Identifier("CreateAsync"))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private MethodDeclarationSyntax GeneratorUpdateMethodOfAbstract(string retrunName, string argName)
    {
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName(argName)
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{retrunName}>"), SyntaxFactory.Identifier("UpdateAsync"))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private MethodDeclarationSyntax GeneratorDeleteMethodOfAbstract(string argName, bool isThereDto, List<Field> uniqueFields)
    {
        var paramList = new List<ParameterSyntax>();

        if (isThereDto)
        {
            paramList.Add(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                    .WithType(SyntaxFactory.IdentifierName(argName))
            );
        }
        else
        {
            foreach (var field in uniqueFields)
            {
                paramList.Add(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(field.Name.ToCamelCase()))
                        .WithType(SyntaxFactory.IdentifierName(field.GetMapedTypeName()))
                );
            }
        }
        paramList.Add(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
            .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
            .WithDefault(SyntaxFactory.EqualsValueClause(
                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
            )
        ));

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task"), SyntaxFactory.Identifier("DeleteAsync"))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }


    private MethodDeclarationSyntax GeneratorDatatableClientSideMethodOfAbstract(string entityName)
    {
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicRequest")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<DatatableResponseClientSide<{entityName}>>"), SyntaxFactory.Identifier("DatatableClientSideAsync"))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private MethodDeclarationSyntax GeneratorDatatableServerSideMethodOfAbstract(string entityName)
    {
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicDatatableServerSideRequest")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        return SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<DatatableResponseServerSide<{entityName}>>"), SyntaxFactory.Identifier("DatatableServerSideAsync"))
            .AddParameterListParameters(paramList.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
    #endregion

    #region Helpers Concrete
    private MethodDeclarationSyntax GeneratorGetMethodOfConcrete(string methodName, Dto dto, List<Field> uniqueFields, bool isThereInclude)
    {
        // 1) Parameters
        var paramList = new List<ParameterSyntax>();
        foreach (var field in uniqueFields)
        {
            paramList.Add(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(field.Name.ToCamelCase()))
                    .WithType(SyntaxFactory.IdentifierName(field.GetMapedTypeName()))
            );
        }
        paramList.Add(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        );

        // 2) If Statements
        List<IfStatementSyntax> ifStatements = new();
        foreach (var field in uniqueFields)
        {
            ifStatements.Add(CreateIfDefaultCheckCondition(field.Name.ToCamelCase()));
        }

        // 3) Arguments of method call
        var arguments = new List<ArgumentSyntax>
        {
            CreateWhereRule(uniqueFields)
        };

        if (isThereInclude) arguments.Add(CreateIncludeRule(dto));

        arguments.Add(
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("tracking")))
        );
        arguments.Add(
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        );

        // 4) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("_GetAsync"))
                        .AddTypeArgumentListArguments(SyntaxFactory.IdentifierName(dto.Name))
                )
                .AddArgumentListArguments(arguments.ToArray())
            );

        // 5) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 6) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));

        // 7) Method Decleration
        var bodyStatements = new List<StatementSyntax>();
        bodyStatements.AddRange(ifStatements);
        bodyStatements.Add(methodCallDecleration);
        bodyStatements.Add(returnStatement);

        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<{dto.Name}?>"),
                SyntaxFactory.Identifier(methodName)
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(SyntaxFactory.Block(bodyStatements));
    }

    private MethodDeclarationSyntax GeneratorGetAllMethodOfConcrete(string methodName, Dto dto, bool isThereInclude)
    {
        // 1) Parameters
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicRequest?")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        // 2) Arguments of method call
        var arguments = new List<ArgumentSyntax>
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request?.Filter"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("filter"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request?.Sorts"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("sorts")))
        };

        if (isThereInclude) arguments.Add(CreateIncludeRule(dto));

        arguments.Add(
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("tracking")))
        );
        arguments.Add(
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        );

        // 3) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("_GetListAsync"))
                        .AddTypeArgumentListArguments(SyntaxFactory.IdentifierName(dto.Name))
                )
                .AddArgumentListArguments(arguments.ToArray())
            );

        // 4) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 5) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));


        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<ICollection<{dto.Name}>?>"),
                SyntaxFactory.Identifier(methodName)
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnStatement
                )
            );
    }

    private MethodDeclarationSyntax GeneratorGetListMethodOfConcrete(string methodName, Dto dto, bool isThereInclude)
    {
        // 1) Parameters
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicPaginationRequest")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        // 2) Arguments of method call
        var arguments = new List<ArgumentSyntax>
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.PaginationRequest"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("paginationRequest"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.Filter"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("filter"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.Sorts"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("sorts")))
        };

        if (isThereInclude) arguments.Add(CreateIncludeRule(dto));

        arguments.Add(
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        );

        // 3) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("_PaginationAsync"))
                        .AddTypeArgumentListArguments(SyntaxFactory.IdentifierName(dto.Name))
                )
                .AddArgumentListArguments(arguments.ToArray())
            );

        // 4) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 5) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));

        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<PaginationResponse<{dto.Name}>>"),
                SyntaxFactory.Identifier(methodName)
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnStatement
                )
            );
    }


    private MethodDeclarationSyntax GeneratorCreateMethodOfConcrete(Entity entity, List<Dto> dtos)
    {
        var basicResponseDto = dtos.FirstOrDefault(f => f.Id == entity.BasicResponseDtoId);
        bool isThereBasicResponseDto = basicResponseDto != null;

        var createDto = dtos.FirstOrDefault(f => f.Id == entity.CreateDtoId);
        bool isThereCreateDto = createDto != null;

        string argType = isThereCreateDto ? createDto!.Name : entity.Name;
        string returnType = isThereBasicResponseDto ? basicResponseDto!.Name : entity.Name;

        // 1) Attribute List
        var attributeList = SyntaxFactory.AttributeList();
        if (isThereCreateDto)
        {
            attributeList = attributeList.AddAttributes(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("Validation"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.TypeOfExpression(
                                    SyntaxFactory.IdentifierName(createDto!.Name)
                                )
                            )
                        )
                    )
                )
            );
        }

        // 2) Parameter List
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName(argType)
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        // 3) Method Call
        List<IdentifierNameSyntax> identifierNameSyntaxes = new List<IdentifierNameSyntax>();
        if (isThereCreateDto)
            identifierNameSyntaxes.Add(SyntaxFactory.IdentifierName(createDto!.Name));

        if (isThereBasicResponseDto)
            identifierNameSyntaxes.Add(SyntaxFactory.IdentifierName(basicResponseDto!.Name));

        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("_AddAsync"))
                        .AddTypeArgumentListArguments(identifierNameSyntaxes.ToArray())
                )
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request"))
                        .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("insertModel"))),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                )
            );

        // 4) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 5) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));


        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<{returnType}>"),
                SyntaxFactory.Identifier("CreateAsync")
            )
            .AddAttributeLists(attributeList)
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnStatement
                )
            );
    }

    private MethodDeclarationSyntax GeneratorUpdateMethodOfConcrete(Entity entity, List<Dto> dtos, List<Field> uniqueFields)
    {
        var basicResponseDto = dtos.FirstOrDefault(f => f.Id == entity.BasicResponseDtoId);
        bool isThereBasicResponseDto = basicResponseDto != null;

        var updateDto = dtos.FirstOrDefault(f => f.Id == entity.UpdateDtoId);
        bool isThereUpdateDto = updateDto != null;

        string argType = isThereUpdateDto ? updateDto!.Name : entity.Name;
        string returnType = isThereBasicResponseDto ? basicResponseDto!.Name : entity.Name;

        // 1) Attribute List
        var attributeList = SyntaxFactory.AttributeList();
        if (isThereUpdateDto)
        {
            attributeList = attributeList.AddAttributes(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("Validation"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.TypeOfExpression(
                                    SyntaxFactory.IdentifierName(updateDto!.Name)
                                )
                            )
                        )
                    )
                )
            );
        }

        // 2) Parameter List
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName(argType)
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        // 3) Arguments of method call
        var argumentsOfMethod = new List<ArgumentSyntax>
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("updateModel"))),

            CreateWhereRule(uniqueFields, "request"),

            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        };

        // 4) Method Call
        List<IdentifierNameSyntax> identifierNameSyntaxes = new List<IdentifierNameSyntax>();
        if (isThereUpdateDto)
            identifierNameSyntaxes.Add(SyntaxFactory.IdentifierName(updateDto!.Name));

        if (isThereBasicResponseDto)
            identifierNameSyntaxes.Add(SyntaxFactory.IdentifierName(basicResponseDto!.Name));

        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("_UpdateAsync"))
                        .AddTypeArgumentListArguments(identifierNameSyntaxes.ToArray())
                )
                .AddArgumentListArguments(argumentsOfMethod.ToArray())
            );

        // 5) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 6) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));

        // 7) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<{returnType}>"),
                SyntaxFactory.Identifier("UpdateAsync")
            )
            .AddAttributeLists(attributeList)
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnStatement
                )
            );
    }

    private MethodDeclarationSyntax GeneratorDeleteMethodOfConcrete(Entity entity, List<Dto> dtos, List<Field> uniqueFields)
    {
        var deleteDto = dtos.FirstOrDefault(f => f.Id == entity.DeleteDtoId);
        bool isThereDeleteDto = deleteDto != null;

        // 1) Attribute List
        var attributeList = SyntaxFactory.AttributeList();
        if (isThereDeleteDto)
        {
            attributeList = attributeList.AddAttributes(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("Validation"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.TypeOfExpression(
                                    SyntaxFactory.IdentifierName(deleteDto!.Name)
                                )
                            )
                        )
                    )
                )
            );
        }

        // 2) Parameters
        var paramList = new List<ParameterSyntax>();

        if (isThereDeleteDto)
        {
            paramList.Add(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                    .WithType(SyntaxFactory.IdentifierName(deleteDto!.Name))
            );
        }
        else
        {
            foreach (var field in uniqueFields)
            {
                paramList.Add(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(field.Name.ToCamelCase()))
                        .WithType(SyntaxFactory.IdentifierName(field.GetMapedTypeName()))
                );
            }
        }

        paramList.Add(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        );


        // 3) If Statements
        List<IfStatementSyntax> ifStatements = new();
        if (isThereDeleteDto == false)
        {
            foreach (var field in uniqueFields)
            {
                ifStatements.Add(CreateIfDefaultCheckCondition(field.Name.ToCamelCase()));
            }
        }

        // 4) Arguments of method call
        var arguments = new List<ArgumentSyntax>
        {
            CreateWhereRule(uniqueFields, isThereDeleteDto ? "request" : null),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        };

        // 4) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("_DeleteAsync")
                )
                .AddArgumentListArguments(arguments.ToArray())
            );

        // 5) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 7) Method Decleration
        var bodyStatements = new List<StatementSyntax>();
        bodyStatements.AddRange(ifStatements);
        bodyStatements.Add(methodCallDecleration);

        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName("Task"),
                SyntaxFactory.Identifier("DeleteAsync")
            )
            .AddAttributeLists(attributeList)
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(SyntaxFactory.Block(bodyStatements));
    }


    private MethodDeclarationSyntax GeneratorDatatableClientSideMethodOfConcrete(Entity entity)
    {
        // 1) Parameters
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicRequest")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        // 2) Arguments of method call
        var arguments = new List<ArgumentSyntax>
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.Filter"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("filter"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.Sorts"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("sorts"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        };


        // 3) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("_DatatableClientSideAsync")
                )
                .AddArgumentListArguments(arguments.ToArray())
            );

        // 4) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 5) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));

        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<DatatableResponseClientSide<{entity.Name}>>"),
                SyntaxFactory.Identifier("DatatableClientSideAsync")
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnStatement
                )
            );
    }

    private MethodDeclarationSyntax GeneratorDatatableServerSideMethodOfConcrete(Entity entity)
    {
        // 1) Parameters
        var paramList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                .WithType(SyntaxFactory.IdentifierName("DynamicDatatableServerSideRequest")
            ),
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
                )
            )
        };

        // 2) Arguments of method call
        var arguments = new List<ArgumentSyntax>
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.DatatableRequest"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("datatableRequest"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request.Filter"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("filter"))),
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("cancellationToken")))
        };


        // 3) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("_DatatableServerSideAsync")
                )
                .AddArgumentListArguments(arguments.ToArray())
            );

        // 4) Method Call Decleration by Result
        var methodCallDecleration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("result"))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitInvocation))
                    )
                )
            );

        // 5) Return Statement
        var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result"));

        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName($"Task<DatatableResponseServerSide<{entity.Name}>>"),
                SyntaxFactory.Identifier("DatatableServerSideAsync")
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .AddParameterListParameters(paramList.ToArray())
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnStatement
                )
            );
    }



    private IfStatementSyntax CreateIfDefaultCheckCondition(string argName)
    {
        var condition = SyntaxFactory.BinaryExpression(
            SyntaxKind.EqualsExpression,
            SyntaxFactory.IdentifierName(argName),
            SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
        );

        var argumentNullException = SyntaxFactory.ThrowStatement(
            SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("ArgumentNullException"))
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("nameof"))
                        .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(argName)))
                    )
                )
            );

        return SyntaxFactory.IfStatement(
            condition,
            argumentNullException
        );
    }
    private ArgumentSyntax CreateWhereRule(List<Field> uniqueFields, string? sourceName = null)
    {
        if (string.IsNullOrEmpty(sourceName)) sourceName = "";
        else sourceName = $".{sourceName.Trim()}";

        var firstField = uniqueFields.First();

        BinaryExpressionSyntax combined = SyntaxFactory.BinaryExpression(
            SyntaxKind.EqualsExpression,
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("f"), SyntaxFactory.IdentifierName(firstField.Name)),
            SyntaxFactory.IdentifierName(sourceName + firstField.Name.ToCamelCase())
        );

        for (int i = 1; i < uniqueFields.Count; i++)
        {
            var nextField = uniqueFields[i];
            var next = SyntaxFactory.BinaryExpression(
                SyntaxKind.EqualsExpression,
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("f"), SyntaxFactory.IdentifierName(nextField.Name)),
                SyntaxFactory.IdentifierName(sourceName + nextField.Name.ToCamelCase())
            );
            combined = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, combined, next);
        }

        return SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(combined)
            .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("f"))))
            .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("where")));
    }
    private ArgumentSyntax CreateIncludeRule(Dto dto)
    {
        InvocationExpressionSyntax? chain = null;

        dto.DtoFields = dto.DtoFields.GroupBy(p => p.SourceField.EntityId).Select(g => g.First()).ToList();

        foreach (var dtoField in dto.DtoFields)
        {
            var dtoFieldRelations = _dtoFieldRepository.GetDtoFieldRelations(dtoField.Id);
            if (!dtoFieldRelations.Any()) continue;

            var dfrFirst = dtoFieldRelations.First();

            bool controlOfRelationFirst = dfrFirst.Relation.PrimaryField.EntityId == dto.RelatedEntityId;

            string destPropOfFirst = controlOfRelationFirst ? dfrFirst.Relation.PrimaryEntityVirPropName : dfrFirst.Relation.ForeignEntityVirPropName;

            ExpressionSyntax identifierNameSyntax = chain != null ? chain : SyntaxFactory.IdentifierName("i");
            chain = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, identifierNameSyntax, SyntaxFactory.IdentifierName("Include")))
                    .AddArgumentListArguments(
                        SyntaxFactory.Argument(
                            SyntaxFactory.SimpleLambdaExpression(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("x")),
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("x"), SyntaxFactory.IdentifierName(destPropOfFirst)))
                        )
                    );

            int lastDestEntityId = controlOfRelationFirst ? dfrFirst.Relation.ForeignField.EntityId : dfrFirst.Relation.PrimaryField.EntityId;

            for (int i = 1; i < dtoFieldRelations.Count; i++)
            {
                var dfr = dtoFieldRelations[i];

                bool controlOfRelation = dfr.Relation.PrimaryField.EntityId == lastDestEntityId;

                string destProp = controlOfRelation ? dfr.Relation.PrimaryEntityVirPropName : dfr.Relation.ForeignEntityVirPropName;

                chain = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, chain, SyntaxFactory.IdentifierName("ThenInclude")))
                        .AddArgumentListArguments(
                            SyntaxFactory.Argument(
                                SyntaxFactory.SimpleLambdaExpression(
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("x")),
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("x"), SyntaxFactory.IdentifierName(destProp)))
                            )
                        );

                lastDestEntityId = controlOfRelation ? dfr.Relation.ForeignField.EntityId : dfr.Relation.PrimaryField.EntityId;
            }
        }

        return SyntaxFactory.Argument(
            SyntaxFactory.ParenthesizedLambdaExpression(chain!)
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("i"))))
            .WithNameColon(SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("include")));
    }
    #endregion
}