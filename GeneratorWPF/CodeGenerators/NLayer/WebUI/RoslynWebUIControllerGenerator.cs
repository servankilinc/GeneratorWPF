using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using GeneratorWPF.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorWPF.CodeGenerators.NLayer.WebUI;

public class RoslynWebUIControllerGenerator
{
    private readonly AppSetting _appSetting;
    private readonly DtoRepository _dtoRepository;
    private readonly FieldRepository _fieldRepository;
    private readonly RelationRepository _relationRepository;
    public RoslynWebUIControllerGenerator(AppSetting appSetting)
    {
        _appSetting = appSetting;
        _dtoRepository = new();
        _fieldRepository = new();
        _relationRepository = new();
    }

    public string GeneraterController(Entity entity, List<Dto> dtos)
    {
        string controllerName = $"{entity.Name}Controller";
        string serviceTypeName = $"I{entity.Name}Service";
        string serviceArgName = $"{entity.Name.ToCamelCase()}Service";
        string serviceName = $"_{entity.Name.ToCamelCase()}Service";

        Dictionary<string, string> selectableRelations = new Dictionary<string, string>();  // fieldName, primaryRelationEntityName
        List<Entity> entitiesToInjection = new List<Entity>();
        List<Field> filterableFields = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id && f.Filterable, include: i => i.Include(x => x.FieldType));
        foreach (var field in filterableFields)
        {
            Relation? relation = _relationRepository.Get(
              filter: f => f.ForeignFieldId == field.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
              include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

            if (relation == null) continue;

            selectableRelations.Add(field.Name, relation.PrimaryField.Entity.Name);
            entitiesToInjection.Add(relation.PrimaryField.Entity);
        }


        // 1) Attribute List
        List<AttributeSyntax> attributeList = new List<AttributeSyntax>();
        if (_appSetting.IsThereIdentiy) attributeList.Add(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Authorize")));


        // 2) Field List
        var fieldList = new List<FieldDeclarationSyntax>
        {
            CreateReadOnlyField(serviceTypeName, serviceName)
        };

        foreach (var entityToInj in entitiesToInjection)
        {
            fieldList.Add(CreateReadOnlyField($"I{entityToInj.Name}Service", $"_{entityToInj.Name.ToCamelCase()}Service"));
        }


        // 3) Constructor args
        List<ParameterSyntax> parameterSyntaxes = new List<ParameterSyntax>
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(serviceArgName)).WithType(SyntaxFactory.ParseTypeName(serviceTypeName))
        };

        foreach (var entityToInj in entitiesToInjection)
        {
            parameterSyntaxes.Add(SyntaxFactory.Parameter(
                SyntaxFactory.Identifier($"{entityToInj.Name.ToCamelCase()}Service"))
                    .WithType(SyntaxFactory.ParseTypeName($"I{entityToInj.Name}Service")));
        }

        // 4) Constructor Statements
        List<ExpressionStatementSyntax> statements = new List<ExpressionStatementSyntax>()
        {
            SyntaxFactory.ExpressionStatement(
               SyntaxFactory.AssignmentExpression(
                   SyntaxKind.SimpleAssignmentExpression,
                   SyntaxFactory.IdentifierName(serviceName),
                   SyntaxFactory.IdentifierName(serviceArgName)
               )
           )
        };

        foreach (var entityToInj in entitiesToInjection)
        {
            statements.Add(
                SyntaxFactory.ExpressionStatement(
                   SyntaxFactory.AssignmentExpression(
                       SyntaxKind.SimpleAssignmentExpression,
                       SyntaxFactory.IdentifierName($"_{entityToInj.Name.ToCamelCase()}Service"),
                       SyntaxFactory.IdentifierName($"{entityToInj.Name.ToCamelCase()}Service")
                   )
               )
            );
        }

        // 5) Constructor
        var constructor = SyntaxFactory.ConstructorDeclaration(controllerName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(parameterSyntaxes.ToArray())
            .WithBody(SyntaxFactory.Block(statements.ToArray())
        );


        // 6) Method List
        var methodList = new List<MethodDeclarationSyntax>();

        methodList.Add(GenerateActionIndex(entity, selectableRelations));
        methodList.Add(GenerateActionCreateGet(entity, selectableRelations));
        methodList.Add(GenerateActionCreateForm(entity));

        // ************ ... Metodlar Eksik Kaldı


        // 7) Class
        var classDeclaration = SyntaxFactory
            .ClassDeclaration(controllerName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("Controller")))
            .AddMembers(fieldList.ToArray())
            .AddMembers(constructor)
            .AddMembers(methodList.ToArray());

        if (attributeList.Count > 0)
            classDeclaration = classDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeList)));

        // 8) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("WebUI.Controllers"))
            .AddMembers(classDeclaration);

        // 9) Compilation Unit
        var usings = new List<UsingDirectiveSyntax>()
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Business.Abstract")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.BaseRequestModels")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.Datatable")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("WebUI.Utils.ActionFilters")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"WebUI.Models.ViewModels.{entity.Name}"))
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


    private MethodDeclarationSyntax GenerateActionIndex(Entity entity, Dictionary<string, string> selectableRelations)
    {
        // 1) Attribute List
        List<AttributeSyntax> attributeList = new List<AttributeSyntax>()
        {
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpGet"))
        };


        // 2) Selecting Call List
        var selectListCallings = new SyntaxNodeOrToken[] { };

        foreach (var relation in selectableRelations)
        {
            string fieldName = relation.Key;
            string entityName = relation.Value;

            if (fieldName.Trim().ToLowerInvariant().EndsWith("id"))
            {
                fieldName = fieldName.Trim().Substring(0, fieldName.Length - 2) + "List";
            }
            else
            {
                fieldName = $"{entityName}List";
            }

            selectListCallings.Append(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(fieldName),
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName($"_{entityName.Trim().ToCamelCase()}Service"),
                                SyntaxFactory.IdentifierName("GetSelectListAsync")
                            )
                        )
                    )
                )
            );
        }

        // 3) Instance Decleration
        var objectCreationExpression =
            SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName($"{entity.Name}ViewModel"))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(selectListCallings)
                    )
                );

        // 4) ViewModel Instance Decleration by Result
        var viewModelInstanceDecleration =
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("viewModel"))
                                .WithInitializer(SyntaxFactory.EqualsValueClause(objectCreationExpression))
                        )
                    )
            );

        // 5) Return Statement
        var returnViewStatement =
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("View"))
                    .AddArgumentListArguments(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("viewModel"))
                    )
            );


        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName("Task<IActionResult>"),
                SyntaxFactory.Identifier("Index")
            )
            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeList)))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .WithBody(
                SyntaxFactory.Block(
                    viewModelInstanceDecleration,
                    returnViewStatement
                )
            );
    }

    private MethodDeclarationSyntax GenerateActionCreateGet(Entity entity, Dictionary<string, string> selectableRelations)
    {
        Dto? createDto = entity.CreateDto != default ? _dtoRepository.Get(f => f.Id == entity.CreateDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(x => x.SourceField)) : default;
        bool isThereCreateDto = createDto != default;

        // 1) Attribute List
        List<AttributeSyntax> attributeList = new List<AttributeSyntax>()
        {
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpGet"))
        };


        // 2) Selecting Call List
        var selectListCallings = new SyntaxNodeOrToken[] { };

        foreach (var relation in selectableRelations)
        {
            string fieldName = relation.Key;
            string entityName = relation.Value;

            // dto içerisnde yoksa boşuna eklenmemeli
            if (isThereCreateDto && !createDto!.DtoFields.Any(f => f.SourceField.Name.Trim().ToLowerInvariant() == fieldName.Trim().ToLowerInvariant()))
                continue;

            if (fieldName.Trim().ToLowerInvariant().EndsWith("id"))
            {
                fieldName = fieldName.Trim().Substring(0, fieldName.Length - 2) + "List";
            }
            else
            {
                fieldName = $"{entityName}List";
            }

            selectListCallings.Append(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(fieldName),
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName($"_{entityName.Trim().ToCamelCase()}Service"),
                                SyntaxFactory.IdentifierName("GetSelectListAsync")
                            )
                        )
                    )
                )
            );
        }

        // 3) Instance Decleration
        var objectCreationExpression =
            SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName($"{entity.Name}CreateViewModel"))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(selectListCallings)
                    )
                );

        // 4) ViewModel Instance Decleration by Result
        var viewModelInstanceDecleration =
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("viewModel"))
                                .WithInitializer(SyntaxFactory.EqualsValueClause(objectCreationExpression))
                        )
                    )
            );

        // 5) Return Statement
        var returnViewStatement =
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("View"))
                    .AddArgumentListArguments(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("viewModel"))
                    )
            );


        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName("Task<IActionResult>"),
                SyntaxFactory.Identifier("Create")
            )
            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeList)))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .WithBody(
                SyntaxFactory.Block(
                    viewModelInstanceDecleration,
                    returnViewStatement
                )
            );
    }

    private MethodDeclarationSyntax GenerateActionCreateForm(Entity entity)
    {
        Dto? createDto = entity.CreateDto != default ? _dtoRepository.Get(f => f.Id == entity.CreateDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(x => x.SourceField)) : default;
        bool isThereCreateDto = createDto != default;

        string requstModelType = isThereCreateDto ? createDto!.Name : entity.Name;

        // 1) Attribute List
        List<AttributeSyntax> attributeList = new List<AttributeSyntax>()
        {
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpGet"))
        };

        if (isThereCreateDto) attributeList.Add(
            SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName("ServiceFilter"),
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.TypeOfExpression(
                                SyntaxFactory.IdentifierName($"ValidationFilter<{createDto!.Name}>")
                            )
                        )
                    )
                )
            )
        );

        // 2) Parameters
        var paramList = new List<ParameterSyntax>
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("createModel"))
                .WithType(SyntaxFactory.IdentifierName(requstModelType))
        };

        // 3) Arguments of method call
        var argumentsOfMethod = new List<ArgumentSyntax>()
        {
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("createModel"))
        };
        

        // 4) Method Call
        var awaitInvocation =
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName($"|{entity.Name.ToCamelCase()}Service.CreateAsync")
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

        // 5) Return Statement
        var returnViewStatement =
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("Ok"))
                    .AddArgumentListArguments(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("result"))
                    )
            );


        // 6) Method Decleration
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName("Task<IActionResult>"),
                SyntaxFactory.Identifier("Create")
            )
            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeList)))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
            )
            .WithBody(
                SyntaxFactory.Block(
                    methodCallDecleration,
                    returnViewStatement
                )
            );
    }








    #region Helpers
    private FieldDeclarationSyntax CreateReadOnlyField(string type, string name)
    {
        return SyntaxFactory
            .FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type)
            )
            .AddVariables(SyntaxFactory.VariableDeclarator(name)))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
            );
    }
    #endregion
}
