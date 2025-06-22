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

public partial class RoslynEntityGenerator
{
    private readonly RelationRepository _relationRepository;
    private readonly FieldRepository _fieldRepository;
    private readonly AppSetting _appSetting;
    public RoslynEntityGenerator(AppSetting appSetting)
    {
        _relationRepository = new();
        _fieldRepository = new();
        _appSetting = appSetting;
    }

    public string GeneraterEntity(Entity entity)
    {
        // 1) Implemantation List
        List<string> interfaces = new() { Statics.IEntity };
        if (entity.SoftDeletable) interfaces.Add(Statics.ISoftDeletableEntity);
        if (entity.Archivable) interfaces.Add(Statics.IArchivableEntity);
        if (entity.Auditable) interfaces.Add(Statics.IAuditableEntity);
        if (entity.Loggable) interfaces.Add(Statics.ILoggableEntity);
        var baseList = interfaces.Select(i => SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(i))).ToArray();

        // 2) Property List
        var propertyList = new List<MemberDeclarationSyntax>();

        List<Field> baseTypeFields = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id, include: i => i.Include(x => x.FieldType));
        foreach (var field in baseTypeFields.Where(f => f.FieldType.SourceTypeId == (int)FieldTypeSourceEnums.Base))
        {
            var propertyDeclaration = GeneratorProperty(field);
            propertyList.Add(propertyDeclaration);
        }

        // 3) Virtual Propert List
        HandleVirtualProps(propertyList, entity.Id);

        // 4) İmplemented İnterface List
        if (entity.Auditable)
        {
            propertyList.Add(GeneratorImplementationProperty("string?", "CreatedBy"));
            propertyList.Add(GeneratorImplementationProperty("string?", "UpdatedBy"));
            propertyList.Add(GeneratorImplementationProperty("DateTime?", "CreateDateUtc"));
            propertyList.Add(GeneratorImplementationProperty("DateTime?", "UpdateDateUtc"));
        }
        if (entity.SoftDeletable)
        {
            propertyList.Add(GeneratorImplementationProperty("string?", "DeletedBy"));
            propertyList.Add(GeneratorImplementationProperty("bool", "IsDeleted"));
            propertyList.Add(GeneratorImplementationProperty("DateTime?", "DeletedDateUtc"));
        }

        // 5) Class
        var classDeclaration = SyntaxFactory
            .ClassDeclaration(entity.Name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(baseList)
            .AddMembers(propertyList.ToArray());

        // 6) NameSpace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("Model.Entities"))
            .AddMembers(classDeclaration);

        // 7) Usings
        List<UsingDirectiveSyntax> usingsList = new(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Core.Model"))
        };

        if (_appSetting.UserEntityId == entity.Id || _appSetting.RoleEntityId == entity.Id)
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Identity")));

        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    #region Helpers
    private void HandleVirtualProps(List<MemberDeclarationSyntax> propertyList, int entityId)
    {
        var relationsOnPrimary = _relationRepository.GetRelationsOnPrimary(entityId);
        var relationsOnForeign = _relationRepository.GetRelationsOnForeign(entityId);

        foreach (var relation in relationsOnPrimary)
        {
            if (relation.RelationTypeId == (int)RelationTypeEnums.OneToOne)
            {
                propertyList.Add(GeneratorVirtualProperty($"{relation.ForeignField.Entity.Name}?", relation.PrimaryEntityVirPropName));
            }
        }
        foreach (var relation in relationsOnForeign)
        {
            if (relation.RelationTypeId == (int)RelationTypeEnums.OneToOne)
            {
                propertyList.Add(GeneratorVirtualProperty($"{relation.PrimaryField.Entity.Name}?", relation.ForeignEntityVirPropName));
            }
            else if (relation.RelationTypeId == (int)RelationTypeEnums.OneToMany)
            {
                propertyList.Add(GeneratorVirtualProperty($"{relation.PrimaryField.Entity.Name}?", relation.ForeignEntityVirPropName));
            }
        }

        // ICollections append end of relations
        foreach (var relation in relationsOnPrimary)
        {
            if (relation.RelationTypeId == (int)RelationTypeEnums.OneToMany)
            {
                propertyList.Add(GeneratorVirtualProperty($"ICollection<{relation.ForeignField.Entity.Name}>?", relation.PrimaryEntityVirPropName));
            }
        }

        // RefreshTokens
        if (entityId == _appSetting.UserEntityId)
        {
            propertyList.Add(GeneratorVirtualProperty("ICollection<RefreshToken>?", "RefreshTokens"));
        }
    }

    private MemberDeclarationSyntax GeneratorProperty(Field field)
    {
        string fieldTypeName = field.MapFieldTypeName();
        string mappedFieldTypeName = fieldTypeName;

        if (field.IsList)
        {
            mappedFieldTypeName = $"List<{mappedFieldTypeName}>";
        }

        if (!field.IsRequired)
        {
            mappedFieldTypeName = $"{mappedFieldTypeName}?";
        }

        var property = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(mappedFieldTypeName), SyntaxFactory.Identifier(field.Name))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        bool isRequiredReferanceType = field.IsRequired && !Statics.nonReferanceTypes.Contains(fieldTypeName);
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

    private MemberDeclarationSyntax GeneratorVirtualProperty(string type, string name)
    {
        return SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(type), SyntaxFactory.Identifier(name))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory
                    .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );
    }

    private MemberDeclarationSyntax GeneratorImplementationProperty(string type, string name)
    {
        return SyntaxFactory
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
    }
    #endregion
}
