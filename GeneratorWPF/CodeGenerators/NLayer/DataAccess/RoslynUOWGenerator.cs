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
            GeneratorFieldOfConcrete("AppDbContext", "_context"),
            GeneratorFieldOfConcrete("IDbContextTransaction", "_transaction")
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
        #region Method Bodies
        string bodyOfBeginSaveChanges = @"return _context.SaveChanges();";

        string bodyOfBeginTransaction = @"
            if (_transaction != null) throw new InvalidOperationException(""Transaction already started for begin transaction."");
            _transaction = _context.Database.BeginTransaction()";

        string bodyOfCommitTransaction = @"
            if (_transaction == null) throw new InvalidOperationException(""Transaction has not been started for commit transaction."");

            _transaction.Commit();

            _transaction.Dispose();
            _transaction = null;";

        string bodyOfRollbackTransaction = @"
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }";

        string bodyOfBeginSaveChangesAsync = @"return await _context.SaveChangesAsync(cancellationToken);";

        string bodyOfBeginTransactionAsync = @"
            if (_transaction != null) throw new InvalidOperationException(""Transaction already started for begin transaction."");

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);";

        string bodyOfCommitTransactionAsync = @"
            if (_transaction == null) throw new InvalidOperationException(""Transaction has not been started for commit."");

            await _transaction.CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();
            _transaction = null;";

        string bodyOfRollbackTransactionAsync = @"
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }";

        #endregion

        var methodList = new List<MethodDeclarationSyntax>()
        {
            GeneratorMethodOfConcrete("int", "SaveChanges", bodyOfBeginSaveChanges),
            GeneratorMethodOfConcrete("void", "BeginTransaction", bodyOfBeginTransaction),
            GeneratorMethodOfConcrete("void", "CommitTransaction", bodyOfCommitTransaction),
            GeneratorMethodOfConcrete("void", "RollbackTransaction", bodyOfRollbackTransaction),

            GeneratorAsyncMethodOfConcrete("Task<int>", "SaveChangesAsync", bodyOfBeginSaveChangesAsync),
            GeneratorAsyncMethodOfConcrete("Task", "BeginTransactionAsync", bodyOfBeginTransactionAsync),
            GeneratorAsyncMethodOfConcrete("Task", "CommitTransactionAsync", bodyOfCommitTransactionAsync),
            GeneratorAsyncMethodOfConcrete("Task", "RollbackTransactionAsync", bodyOfRollbackTransactionAsync),
        };

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
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
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
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
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
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
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
    private FieldDeclarationSyntax GeneratorFieldOfConcrete(string type, string name)
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

    private MethodDeclarationSyntax GeneratorMethodOfConcrete(string returnType, string name, string body)
    {
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName(returnType),
                SyntaxFactory.Identifier(name)
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)
            )
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.ParseStatement(body)
            ));
    }

    private MethodDeclarationSyntax GeneratorAsyncMethodOfConcrete(string returnType, string name, string body)
    {
        return SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.ParseTypeName(returnType),
                SyntaxFactory.Identifier(name)
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
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
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.ParseStatement(body)
            ));
    }
    #endregion
}
