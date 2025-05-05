using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Text;

namespace GeneratorWPF.CodeGenerators;

public static class RoslynBusinessGenerator
{
    public static void GeneraterBusinessAbstrack(string entityName, string outputDirectory)
    {
        var fileName = $"I{entityName}Service";

        var interfaceDeclaration = SyntaxFactory
            .InterfaceDeclaration(fileName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Business.Abstract"))
            .AddMembers(interfaceDeclaration);


        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.DataAccess.Pagination"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        File.WriteAllText(Path.Combine(outputDirectory, $"{fileName}.cs"), code, Encoding.UTF8);
    }

    public static void GeneraterBusinessConcrete(string entityName, string outputDirectory)
    {
        var fileName = $"{entityName}Manager";

        var typeIdentifierRepo = $"I{entityName}Service";
        var typeIdentifierMapper = "IMapper";
        var indentifierRepo = $"_{entityName.ToLower()}Repository";
        var indentifierConstRepo = $"{entityName.ToLower()}Repository";
        var identifierMapper = "_mapper";


        var repositoryField = SyntaxFactory
            .FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(typeIdentifierRepo)
                )
                .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(indentifierRepo)))
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

        var mapperField = SyntaxFactory
         .FieldDeclaration(
             SyntaxFactory.VariableDeclaration(
                 SyntaxFactory.ParseTypeName(typeIdentifierMapper)
             )
             .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(identifierMapper)))
         )
         .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));



        var constructorDeclaration = SyntaxFactory
            .ConstructorDeclaration(fileName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(indentifierConstRepo))
                        .WithType(SyntaxFactory.ParseTypeName(typeIdentifierRepo)),
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("mapper"))
                        .WithType(SyntaxFactory.ParseTypeName(typeIdentifierMapper))
            )
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(indentifierRepo),
                            SyntaxFactory.IdentifierName(indentifierConstRepo)
                        )
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(identifierMapper),
                            SyntaxFactory.IdentifierName("mapper")
                        )
                    )
                )
            );


        var classDeclaration = SyntaxFactory
            .ClassDeclaration(fileName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(typeIdentifierRepo))
            )
            .AddMembers(repositoryField, mapperField, constructorDeclaration);

        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Business.Concrete"))
            .AddMembers(classDeclaration);


        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Business.Abstract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DataAccess.Abstract")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("AutoMapper"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        File.WriteAllText(Path.Combine(outputDirectory, $"{fileName}.cs"), code, Encoding.UTF8);
    }
}
