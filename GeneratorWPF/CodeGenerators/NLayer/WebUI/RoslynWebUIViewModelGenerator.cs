using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using GeneratorWPF.Models.Statics;
using GeneratorWPF.Repository;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace GeneratorWPF.CodeGenerators.NLayer.WebUI;

public class RoslynWebUIViewModelGenerator
{
    private readonly RelationRepository _relationRepository;
    private readonly DtoRepository _dtoRepository;
    private readonly FieldRepository _fieldRepository;
    public RoslynWebUIViewModelGenerator(AppSetting appSetting)
    {
        _relationRepository = new();
        _dtoRepository = new();
        _fieldRepository = new();
    }

    public string GenerateViewModelIndex(Entity entity)
    {
        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>();

        // a) SelectList Props
        List<Field> filterableFields = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id && f.Filterable, include: i => i.Include(x => x.FieldType));
        foreach (var field in filterableFields)
        {
            Relation? relation = _relationRepository.Get(
              filter: f => f.ForeignFieldId == field.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
              include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

            if (relation == null) continue;

            string selectPropName = field.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
            propertyList.Add(GeneratorProperty("SelectList?", selectPropName));
        }

        // b) FilterModel Prop
        if (filterableFields.Any() || entity.SoftDeletable)
            propertyList.Add(GeneratorProperty($"{entity.Name}FilterModel", "FilterModel", earlyInstance: true));


        // 2) Class List
        List<ClassDeclarationSyntax> classes =
        [
            SyntaxFactory.ClassDeclaration($"{entity.Name}ViewModel")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(propertyList.ToArray()),
        ];

        if (filterableFields.Any() || entity.SoftDeletable)
            classes.Add(GenerateFilterModel(entity, filterableFields));

        // 3) Namespace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName($"WebUI.Models.ViewModels.{entity.Name}_"))
            .AddMembers(classes.ToArray());


        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc.Rendering")))
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GenerateViewModelCreate(Entity entity)
    {
        Dto? createDto = entity.CreateDtoId != default ? _dtoRepository.Get(f => f.Id == entity.CreateDtoId, include: i=> i.Include(x => x.DtoFields).ThenInclude(y => y.SourceField)) : default;
        bool isThereCreateDto = createDto != default;
        string createModelType = isThereCreateDto ? createDto!.Name : entity.Name;

        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>
        {
            // a) FilterModel Prop
            GeneratorProperty(createModelType, "CreateModel", earlyInstance: true)
        };

        // b) SelectList Props
        List<Field> filterableFields = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id && f.Filterable, include: i => i.Include(x => x.FieldType));
        if (isThereCreateDto)
        {
            foreach (var dtoField in createDto!.DtoFields)
            {
                if (filterableFields.Any(f => f.Id == dtoField.SourceFieldId) == false) continue;
             
                Relation? relation = _relationRepository.Get(
                    filter: f => f.ForeignFieldId == dtoField.SourceField.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
                    include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

                if (relation == null) continue;

                string selectPropName = dtoField.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
                propertyList.Add(GeneratorProperty("SelectList?", selectPropName));
            }
        }
        else
        {
            foreach (var field in filterableFields)
            {
                Relation? relation = _relationRepository.Get(
                    filter: f => f.ForeignFieldId == field.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
                    include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

                if (relation == null) continue;

                string selectPropName = field.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
                propertyList.Add(GeneratorProperty("SelectList?", selectPropName));

            }
        }

        // 2) Class List
        List<ClassDeclarationSyntax> classes =
        [
            SyntaxFactory.ClassDeclaration($"{entity.Name}CreateViewModel")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(propertyList.ToArray())
        ];


        // 3) Namespace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName($"WebUI.Models.ViewModels.{entity.Name}_"))
            .AddMembers(classes.ToArray());


        List<UsingDirectiveSyntax> usingsList = new List<UsingDirectiveSyntax>(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc.Rendering"))
        };

        if (isThereCreateDto) 
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{entity.Name}_")));
        else 
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Entities")));

        var compilationUnit = SyntaxFactory
                .CompilationUnit()
                .AddUsings(usingsList.ToArray())
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    public string GenerateViewModelUpdate(Entity entity)
    {
        Dto? updateDto = entity.UpdateDtoId != default ? _dtoRepository.Get(f => f.Id == entity.UpdateDtoId, include: i => i.Include(x => x.DtoFields).ThenInclude(y => y.SourceField)) : default;
        bool isThereUpdateDto = updateDto != default;
        string updateModelType = isThereUpdateDto ? updateDto!.Name : entity.Name;

        // 1) Property List
        var propertyList = new List<MemberDeclarationSyntax>
        {
            // a) FilterModel Prop
            GeneratorProperty(updateModelType, "UpdateModel", earlyInstance: true)
        };

        // b) SelectList Props
        List<Field> filterableFields = _fieldRepository.GetAll(filter: f => f.EntityId == entity.Id && f.Filterable, include: i => i.Include(x => x.FieldType));
        if (isThereUpdateDto)
        {
            foreach (var dtoField in updateDto!.DtoFields)
            {
                if (filterableFields.Any(f => f.Id == dtoField.SourceFieldId) == false) continue;

                Relation? relation = _relationRepository.Get(
                    filter: f => f.ForeignFieldId == dtoField.SourceField.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
                    include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

                if (relation == null) continue;

                string selectPropName = dtoField.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
                propertyList.Add(GeneratorProperty("SelectList?", selectPropName));
            }
        }
        else
        {
            foreach (var field in filterableFields)
            {
                Relation? relation = _relationRepository.Get(
                    filter: f => f.ForeignFieldId == field.Id && f.RelationTypeId == (byte)RelationTypeEnums.OneToMany,
                    include: i => i.Include(x => x.PrimaryField).ThenInclude(x => x.Entity));

                if (relation == null) continue;

                string selectPropName = field.Name.ToForeignFieldSlectListName(relation.PrimaryField.Entity.Name);
                propertyList.Add(GeneratorProperty("SelectList?", selectPropName));
            }
        }

        // 2) Class List
        List<ClassDeclarationSyntax> classes =
        [
            SyntaxFactory.ClassDeclaration($"{entity.Name}UpdateViewModel")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(propertyList.ToArray())
        ];


        // 5) Namespace
        var namespaceDeclaration = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName($"WebUI.Models.ViewModels.{entity.Name}_"))
            .AddMembers(classes.ToArray());

        List<UsingDirectiveSyntax> usingsList = new List<UsingDirectiveSyntax>(){
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc.Rendering"))
        };

        if (isThereUpdateDto) 
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Dtos.{entity.Name}_")));
        else
            usingsList.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"Model.Entities")));
        
        var compilationUnit = SyntaxFactory
            .CompilationUnit()
            .AddUsings(usingsList.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }


    private ClassDeclarationSyntax GenerateFilterModel(Entity entity, List<Field> filterableFields)
    {
        var propertyList = new List<MemberDeclarationSyntax>();
        foreach (var field in filterableFields)
        {
            propertyList.Add(GeneratorProperty($"{field.GetMapedTypeName()}?", field.Name, false));
        }
        if (entity.SoftDeletable)
        {
            propertyList.Add(GeneratorProperty("bool", "IsDeleted", false));
        }

        return SyntaxFactory.ClassDeclaration($"{entity.Name}FilterModel")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(propertyList.ToArray());
    }


    #region Helpers
    private MemberDeclarationSyntax GeneratorProperty(string type, string name, bool isRequired = false, bool earlyInstance = false)
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

        bool isRequiredReferanceType = isRequired && !Statics.nonReferanceTypes.Contains(type);
        if (earlyInstance)
        {
            property = property
                .WithInitializer(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.Token(SyntaxKind.EqualsToken),
                        SyntaxFactory.ParseExpression($"new {type}()")
                    )
                )
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
        else if (isRequiredReferanceType)
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
    #endregion
}
