using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GeneratorWPF.CodeGenerators.NLayer.DataAccess;

public partial class RoslynUOWGenerator
{
    private readonly AppSetting _appSetting;
    public RoslynUOWGenerator(AppSetting appSetting)
    {
        _appSetting = appSetting;
    }

    public string GeneraterAbstract(List<Entity> entities)
    {
        // 1) Property List
        var propertyList = new List<PropertyDeclarationSyntax>();

        foreach (var entity in entities)
        {
            propertyList.Add(GeneratorPropertyOfAbstract($"I{entity.Name}Repository", entity.Name.Pluralize()));
        }
        if (_appSetting.IsThereIdentiy)
        {
            propertyList.Add(GeneratorPropertyOfAbstract("IRefreshTokenRepository", "RefreshTokens"));
        }

        // 2) Method List
        var methodList = new List<MethodDeclarationSyntax>()
        {
            GeneratorMethodOfAbstract("int", "SaveChanges"),
            GeneratorMethodOfAbstract("void", "BeginTransaction"),
            GeneratorMethodOfAbstract("void", "CommitTransaction"),
            GeneratorMethodOfAbstract("void", "RollbackTransaction"),

            GeneratorAsyncMethodOfAbstract("Task<int>", "SaveChangesAsync"),
            GeneratorAsyncMethodOfAbstract("Task", "BeginTransactionAsync"),
            GeneratorAsyncMethodOfAbstract("Task", "CommitTransactionAsync"),
            GeneratorAsyncMethodOfAbstract("Task", "RollbackTransactionAsync"),
        };

        var members = new List<MemberDeclarationSyntax>();
        members.AddRange(propertyList);
        members.AddRange(methodList);

        // 3) Interface
        var InterfaceDeclaration = SyntaxFactory
            .InterfaceDeclaration("IUnitOfWork")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IDisposable")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IAsyncDisposable")))
            .AddMembers(members.ToArray());

        // 4) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("DataAccess.UoW"))
            .AddMembers(InterfaceDeclaration);

        // 5) Compilation Unit
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract")))
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterConcrete(List<Entity> entities)
    {
        // 1) Field List
        var fieldList = new List<FieldDeclarationSyntax>
        {
            GeneratorFieldOfConcrete("AppDbContext", "_context", isReadOnly: true, isNullable: false),
            GeneratorFieldOfConcrete("IDbContextTransaction", "_transaction", isReadOnly: false, isNullable: true)
        };

        // 2) Property List
        var propertyList = new List<PropertyDeclarationSyntax>();

        foreach (var entity in entities)
        {
            propertyList.Add(GeneratorPropertyOfConcrete($"I{entity.Name}Repository", entity.Name.Pluralize()));
        }
        if (_appSetting.IsThereIdentiy)
        {
            propertyList.Add(GeneratorPropertyOfConcrete("IRefreshTokenRepository", "RefreshTokens"));
        }

        // 3) Method List
        var methodList = new List<MethodDeclarationSyntax>();

        #region Method Bodies
        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("int"), "SaveChanges")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("_context"),
                            SyntaxFactory.IdentifierName("SaveChanges")
                        )
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "BeginTransaction")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.ThrowStatement(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.IdentifierName("InvalidOperationException"))
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal("Transaction already started for begin transaction.")
                                    )
                                )
                            )
                        ))
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("_context"),
                                    SyntaxFactory.IdentifierName("Database")),
                                SyntaxFactory.IdentifierName("BeginTransaction"))
                        )
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "CommitTransaction")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.ThrowStatement(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.IdentifierName("InvalidOperationException"))
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal("Transaction has not been started for commit transaction.")
                                    )
                                )
                            )
                        ))
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("_transaction"),
                            SyntaxFactory.IdentifierName("Commit")
                        )
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("_transaction"),
                            SyntaxFactory.IdentifierName("Dispose")
                        )
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "RollbackTransaction")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("_transaction"),
                                    SyntaxFactory.IdentifierName("Rollback")
                                )
                            )
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("_transaction"),
                                    SyntaxFactory.IdentifierName("Dispose")
                                )
                            )
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("_transaction"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            )
                        )
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task<int>"), "SaveChangesAsync")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                            .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                    )
                )
            )
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_context"),
                                SyntaxFactory.IdentifierName("SaveChangesAsync")
                            )
                        )
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                            )
                        ))
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "BeginTransactionAsync")
             .AddModifiers(
                 SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                 SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
             .WithParameterList(
                 SyntaxFactory.ParameterList(
                     SyntaxFactory.SingletonSeparatedList(
                         SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                             .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                     )
                 )
             )
             .WithBody(SyntaxFactory.Block(
                 SyntaxFactory.IfStatement(
                     SyntaxFactory.BinaryExpression(
                         SyntaxKind.NotEqualsExpression,
                         SyntaxFactory.IdentifierName("_transaction"),
                         SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                     ),
                     SyntaxFactory.ThrowStatement(
                         SyntaxFactory.ObjectCreationExpression(
                             SyntaxFactory.IdentifierName("InvalidOperationException"))
                         .WithArgumentList(SyntaxFactory.ArgumentList(
                             SyntaxFactory.SingletonSeparatedList(
                                 SyntaxFactory.Argument(
                                     SyntaxFactory.LiteralExpression(
                                         SyntaxKind.StringLiteralExpression,
                                         SyntaxFactory.Literal("Transaction already started for begin transaction.")
                                     )
                                 )
                             ))
                         )
                     )
                 ),
                 SyntaxFactory.ExpressionStatement(
                     SyntaxFactory.AssignmentExpression(
                         SyntaxKind.SimpleAssignmentExpression,
                         SyntaxFactory.IdentifierName("_transaction"),
                         SyntaxFactory.AwaitExpression(
                             SyntaxFactory.InvocationExpression(
                                 SyntaxFactory.MemberAccessExpression(
                                     SyntaxKind.SimpleMemberAccessExpression,
                                     SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                         SyntaxFactory.IdentifierName("_context"),
                                         SyntaxFactory.IdentifierName("Database")),
                                     SyntaxFactory.IdentifierName("BeginTransactionAsync"))
                             )
                             .WithArgumentList(SyntaxFactory.ArgumentList(
                                 SyntaxFactory.SingletonSeparatedList(
                                     SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                                 )
                             ))
                         )
                     )
                 )
             )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "CommitTransactionAsync")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                            .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                    )
                )
            )
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.ThrowStatement(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.IdentifierName("InvalidOperationException"))
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal("Transaction has not been started for commit.")
                                    )
                                )
                            ))
                        )
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_transaction"),
                                SyntaxFactory.IdentifierName("CommitAsync")
                            )
                        )
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                            )
                        ))
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_transaction"),
                                SyntaxFactory.IdentifierName("DisposeAsync")
                            )
                        )
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "RollbackTransactionAsync")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                            .WithType(SyntaxFactory.IdentifierName("CancellationToken"))
                    )
                )
            )
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("_transaction"),
                                        SyntaxFactory.IdentifierName("RollbackAsync")
                                    )
                                )
                                .WithArgumentList(SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("cancellationToken"))
                                    )
                                ))
                            )
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("_transaction"),
                                        SyntaxFactory.IdentifierName("DisposeAsync")
                                    )
                                )
                            )
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("_transaction"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            )
                        )
                    )
                )
            )));


        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Dispose")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("_transaction"),
                                    SyntaxFactory.IdentifierName("Dispose")
                                )
                            )
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("_transaction"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            )
                        )
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("_context"),
                            SyntaxFactory.IdentifierName("Dispose")
                        )
                    )
                )
            )));

        methodList.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("ValueTask"), "DisposeAsync")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        SyntaxFactory.IdentifierName("_transaction"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AwaitExpression(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("_transaction"),
                                        SyntaxFactory.IdentifierName("DisposeAsync")
                                    )
                                )
                            )
                        ),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("_transaction"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            )
                        )
                    )
                ),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_context"),
                                SyntaxFactory.IdentifierName("DisposeAsync")
                            )
                        )
                    )
                )
            )));
        #endregion


        // 4) Constructor
        var parameterList = new List<ParameterSyntax>()
        {
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("context")).WithType(SyntaxFactory.ParseTypeName("AppDbContext"))
        };
        var statementList = new List<StatementSyntax>()
        {
            SyntaxFactory.ParseStatement("_context = context;")
        };
        foreach (var entity in entities)
        {
            string argType = $"I{entity.Name}Repository";
            string argName = $"{entity.Name}Repository".ToCamelCase();

            parameterList.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(argName)).WithType(SyntaxFactory.ParseTypeName(argType)));

            string statementFieldName = entity.Name.Pluralize();
            statementList.Add(SyntaxFactory.ParseStatement($"{statementFieldName} = {argName};"));
        }
        if (_appSetting.IsThereIdentiy)
        {
            parameterList.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("refreshTokens")).WithType(SyntaxFactory.ParseTypeName("IRefreshTokenRepository")));

            statementList.Add(SyntaxFactory.ParseStatement("RefreshTokens = refreshTokens;"));
        }

        var constructor = SyntaxFactory.ConstructorDeclaration("UnitOfWork")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(parameterList.ToArray())
            .WithBody(SyntaxFactory.Block(statementList.ToArray()));

        // 3) Class
        var classDeclaration = SyntaxFactory
            .ClassDeclaration("UnitOfWork")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IUnitOfWork")))
            .AddMembers(fieldList.ToArray())
            .AddMembers(propertyList.ToArray())
            .AddMembers(constructor)
            .AddMembers(methodList.ToArray());

        // 4) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("DataAccess.UoW"))
            .AddMembers(classDeclaration);

        // 5) Compilation Unit
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Contexts")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore.Storage"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    #region Helpers Abstract
    private PropertyDeclarationSyntax GeneratorPropertyOfAbstract(string type, string name)
    {
        return SyntaxFactory
            .PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type),
                SyntaxFactory.Identifier(name)
            )
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
    }

    private MethodDeclarationSyntax GeneratorMethodOfAbstract(string returnType, string name)
    {
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName(returnType),
                SyntaxFactory.Identifier(name)
            )
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private MethodDeclarationSyntax GeneratorAsyncMethodOfAbstract(string returnType, string name)
    {
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName(returnType),
                SyntaxFactory.Identifier(name)
            )
            .AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                    .WithType(SyntaxFactory.ParseTypeName("CancellationToken"))
                    .WithDefault(SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.DefaultLiteralExpression,
                            SyntaxFactory.Token(SyntaxKind.DefaultKeyword)
                        )
                    )
                )
            )
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
    #endregion

    #region Helpers Concrete
    private FieldDeclarationSyntax GeneratorFieldOfConcrete(string type, string name, bool? isReadOnly = false, bool? isNullable = false)
    {
        if (isReadOnly == true)
        {
            return SyntaxFactory
            .FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type + (isNullable  == true ? "?" : string.Empty))
            )
            .AddVariables(SyntaxFactory.VariableDeclarator(name)))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
            );
        }
        return SyntaxFactory
            .FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type + (isNullable == true ? "?" : string.Empty))
            )
            .AddVariables(SyntaxFactory.VariableDeclarator(name)))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword)
            );
    }

    private PropertyDeclarationSyntax GeneratorPropertyOfConcrete(string type, string name)
    {
        return SyntaxFactory
            .PropertyDeclaration(
                SyntaxFactory.ParseTypeName(type),
                SyntaxFactory.Identifier(name)
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
            )
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
    }
    #endregion
}
