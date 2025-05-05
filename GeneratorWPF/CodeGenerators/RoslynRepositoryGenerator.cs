using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Text;

namespace GeneratorWPF.CodeGenerators;

public static class RoslynRepositoryGenerator
{
    public static void GeneraterRepositoryAbstrack(string entityName, string contextName, string outputDirectory)
    {
        var fileName = $"I{entityName}Repository";

        var interfaceDeclaration = SyntaxFactory
            .InterfaceDeclaration(fileName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IRepository<{entityName}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IRepositoryAsync{entityName}"))
            );

        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("DataAccess.Abstract"))
            .AddMembers(interfaceDeclaration);


        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract.RepositoryBase")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        File.WriteAllText(Path.Combine(outputDirectory, $"{fileName}.cs"), code, Encoding.UTF8);
    }



    public static void GeneraterRepositoryConcrete(string entityName, string contextName, string outputDirectory)
    {
        var fileName = $"{entityName}Repository";

        var constructor = SyntaxFactory
            .ConstructorDeclaration(fileName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("context"))
                        .WithType(SyntaxFactory.ParseTypeName(contextName))
            )
            .WithInitializer(
                SyntaxFactory.ConstructorInitializer(
                    SyntaxKind.BaseConstructorInitializer,
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("context"))
                        )
                    )
                )
            )
            .WithBody(SyntaxFactory.Block());


        var classDeclaration = SyntaxFactory
            .ClassDeclaration(fileName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"RepositoryBase<{entityName}, {contextName}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{fileName}"))
            )
            .AddMembers(constructor);

        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("DataAccess.Concrete"))
            .AddMembers(classDeclaration);

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Concrete.RepositoryBase")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Contexts")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Model.Entities"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        File.WriteAllText(Path.Combine(outputDirectory, $"{fileName}.cs"), code, Encoding.UTF8);
    }
}
