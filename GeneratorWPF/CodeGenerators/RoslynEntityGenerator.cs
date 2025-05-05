using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Text;

namespace GeneratorWPF.CodeGenerators;

public static partial class RoslynGenerator
{
    public static void GeneraterEntity(Entity entity, List<Entity> entities, string outputDirectory)
    {
        var propertyList = new List<MemberDeclarationSyntax>();
        foreach (var field in entity.Fields)
        {
            if(field.FieldType == null || string.IsNullOrEmpty(field.FieldType.Name))
            {
                Console.WriteLine($"GEN-WARNING: Field type is null or empty, entity of {entity.Name}");
                continue;
            }
            var propertyDeclaration = PropertyGenerator(field.FieldType.Name, field.Name);
            propertyList.Add(propertyDeclaration);
        }

        HandleRelationProps(propertyList, entity, entities);

        var classDeclaration = SyntaxFactory
            .ClassDeclaration(entity.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IEntity")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("SoftDeletableEntity"))
            )
            .AddMembers(propertyList.ToArray());

        var namespaceDeclaration = SyntaxFactory.
            NamespaceDeclaration(SyntaxFactory.ParseName("Model.Entities"))
            .AddMembers(classDeclaration);

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Model")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
            )
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        File.WriteAllText(Path.Combine(outputDirectory, $"{entity.Name}.cs"), code, Encoding.UTF8);
    }


    private static void HandleRelationProps(List<MemberDeclarationSyntax> propertyList, Entity entity, List<Entity> entities)
    {
        if (entity.Fields == null) return;
        
        foreach (var field in entity.Fields)
        {
            if (field.RelationsPrimary != null)
            {
                foreach (var relation in field.RelationsPrimary)
                {
                    var foreignEntity = entities.FirstOrDefault(e => e.Fields.Any(f => f.Id == relation.ForeignFieldId));
                    if (foreignEntity == null || string.IsNullOrEmpty(foreignEntity.Name))
                    {
                        Console.WriteLine($"GEN-WARNING: Foreign entity is null or empty, entity of {entity.Name}");
                        continue;
                    }

                    if (relation.RelationType.Id == (int)RelationTypeEnums.OneToOne)
                    {
                        var navigationProperty = PropertyGenerator(foreignEntity.Name, foreignEntity.Name);

                        propertyList.Add(navigationProperty);
                    }
                    else
                    {
                        var navigationProperty = PropertyGenerator($"ICollection<{foreignEntity.Name}>", $"{foreignEntity.Name}List");

                        propertyList.Add(navigationProperty);
                    }
                }
            }
            if (field.RelationsPrimary != null)
            {
                foreach (var relation in field.RelationsForeign)
                {
                    var primaryEntity = entities.FirstOrDefault(e => e.Fields.Any(f => f.Id == relation.PrimaryFieldId));
                    if (primaryEntity == null || string.IsNullOrEmpty(primaryEntity.Name))
                    {
                        Console.WriteLine($"GEN-WARNING: Primary entity is null or empty, entity of {entity.Name}");
                        continue;
                    }

                    if (relation.RelationType.Id == (int)RelationTypeEnums.OneToOne)
                    {
                        var navigationProperty = PropertyGenerator(primaryEntity.Name, primaryEntity.Name);

                        propertyList.Add(navigationProperty);
                    }
                    else
                    {
                        var navigationProperty = PropertyGenerator(primaryEntity.Name, primaryEntity.Name);

                        propertyList.Add(navigationProperty);
                    }
                }
            }
        }        
    }

    private static MemberDeclarationSyntax PropertyGenerator(string type, string identifier)
    {
        return SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(type), identifier)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );
    }
}
