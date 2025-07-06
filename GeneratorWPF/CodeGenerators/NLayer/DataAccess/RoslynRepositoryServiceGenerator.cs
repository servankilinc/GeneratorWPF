using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GeneratorWPF.CodeGenerators.NLayer.DataAccess;

public partial class RoslynRepositoryServiceGenerator
{
    public string GeneraterAbstract(string entityName)
    {
        // 1) Interface
        var InterfaceDeclaration = SyntaxFactory
            .InterfaceDeclaration($"I{entityName}Repository")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IRepository<{entityName}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IRepositoryAsync<{entityName}>")));

        // 2) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("DataAccess.Abstract"))
            .AddMembers(InterfaceDeclaration);

        // 3) Compilation Unit
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Repository")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities")))
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GeneraterConcrete(string entityName)
    {
        string repoName = $"{entityName}Repository";

        // 1) Attribute List
        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("DataAccessException"))
            )
        );

        // 2) Constructor 
        var constructor = SyntaxFactory.ConstructorDeclaration(repoName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("context"))
                        .WithType(SyntaxFactory.ParseTypeName("AppDbContext"))
            )
            .WithInitializer(
                SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("context")))
            )
            .WithBody(SyntaxFactory.Block());

        // 3) Class
        var classDeclaration = SyntaxFactory
            .ClassDeclaration(repoName)
            .AddAttributeLists(attributeList)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"RepositoryBase<{entityName}, AppDbContext>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{entityName}Repository"))
            )
            .AddMembers(constructor);

        // 4) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("DataAccess.Concrete"))
            .AddMembers(classDeclaration);

        // 5) Compilation Unit
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Utils.CrossCuttingConcerns")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Contexts")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Repository")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }
}
