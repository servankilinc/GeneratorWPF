﻿using Microsoft.EntityFrameworkCore;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using GeneratorWPF.Utils;

namespace GeneratorWPF.Context
{
    public class ProjectContext : DbContext
    {
        // Static Version
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=.; Initial Catalog=CodeGeneratorV3; Integrated Security=SSPI; Trusted_Connection=True; TrustServerCertificate=True;");
        //}

        // Dynamic Version
        public ProjectContext(): base(GetOptions())
        {
        }

        private static DbContextOptions<ProjectContext> GetOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProjectContext>();
            optionsBuilder.UseSqlServer(StateStatics.CurrentProject.ProjectName); // burası bir şekilde conn stringe dönüştürülmeli
            return optionsBuilder.Options;
        }

        public DbSet<AppSetting> AppSettings { get; set; }

        public DbSet<Entity> Entities { get; set; }

        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldType> FieldTypes { get; set; }
        public DbSet<FieldTypeSource> FieldTypeSources { get; set; }

        public DbSet<DeleteBehaviorType> DeleteBehaviorTypes { get; set; }
        public DbSet<Relation> Relations { get; set; }
        public DbSet<RelationType> RelationTypes { get; set; }

        public DbSet<CrudType> CrudTypes { get; set; }

        public DbSet<Dto> Dtos { get; set; }
        public DbSet<DtoField> DtoFields { get; set; }
        public DbSet<DtoFieldRelations> DtoFieldRelations { get; set; }

        public DbSet<Validation> Validations { get; set; }
        public DbSet<ValidationParam> ValidationParams { get; set; }
        public DbSet<ValidatorType> ValidatorTypes { get; set; }
        public DbSet<ValidatorTypeParam> ValidatorTypeParams { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceLayer> ServiceLayers { get; set; }

        public DbSet<Method> Methods { get; set; }
        public DbSet<MethodArgumentField> MethodArgumentFields { get; set; }
        public DbSet<MethodReturnField> MethodReturnFields { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region AppSetting
            modelBuilder.Entity<AppSetting>(ap =>
            {
                ap.HasKey(ap => ap.Id);

                ap.HasData(new AppSetting()
                {
                    Id = 1,
                    Path = "C:\\Generator",
                    ProjectName = "MyProject",
                    SolutionName = "MyProject",
                    IsThereIdentiy = true,
                    Control = false,
                    DBConnectionString = "Data Source=.; Initial Catalog=MyGeneratedDatabase; Integrated Security=SSPI; Trusted_Connection=True; TrustServerCertificate=True;"
                });

                modelBuilder.Entity<AppSetting>()
                    .HasOne(a => a.UserEntity)
                    .WithOne(e => e.AsUserAppSetting)
                    .HasForeignKey<AppSetting>(a => a.UserEntityId);

                modelBuilder.Entity<AppSetting>()
                    .HasOne(a => a.RoleEntity)
                    .WithOne(e => e.AsRoleAppSetting)
                    .HasForeignKey<AppSetting>(a => a.RoleEntityId);
            });
            #endregion

            #region Entity
            modelBuilder.Entity<Entity>(e =>
            {
                e.HasKey(e => e.Id);

                e.HasOne(e => e.CreateDto)
                    .WithMany(cd => cd.CreateEntities)
                    .HasForeignKey(e => e.CreateDtoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(e => e.UpdateDto)
                    .WithMany(cd => cd.UpdateEntities)
                    .HasForeignKey(e => e.UpdateDtoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(e => e.DeleteDto)
                    .WithMany(cd => cd.DeleteEntities)
                    .HasForeignKey(e => e.DeleteDtoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(e => e.ReportDto)
                    .WithMany(cd => cd.ReportEntities)
                    .HasForeignKey(e => e.ReportDtoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(e => e.BasicResponseDto)
                    .WithMany(cd => cd.BasicResponseEntities)
                    .HasForeignKey(e => e.BasicResponseDtoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(e => e.DetailResponseDto)
                    .WithMany(cd => cd.DetailResponseEntities)
                    .HasForeignKey(e => e.DetailResponseDtoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(e => e.Fields)
                    .WithOne(f => f.Entity)
                    .HasForeignKey(f => f.EntityId);

                e.HasMany(e => e.Dtos)
                    .WithOne(f => f.RelatedEntity)
                    .HasForeignKey(f => f.RelatedEntityId);

                e.HasMany(e => e.Services)
                    .WithOne(s => s.RelatedEntity)
                    .HasForeignKey(s => s.RelatedEntityId);
            });
            #endregion

            #region Field
            modelBuilder.Entity<Field>(f =>
            {
                f.HasKey(f => f.Id);

                f.HasOne(f => f.Entity)
                   .WithMany(e => e.Fields)
                   .HasForeignKey(f => f.EntityId);

                f.HasOne(f => f.FieldType)
                    .WithMany(t => t.Fields)
                    .HasForeignKey(f => f.FieldTypeId);

                f.HasMany(f => f.RelationsPrimary)
                    .WithOne(r => r.PrimaryField)
                    .HasForeignKey(f => f.PrimaryFieldId)
                    .OnDelete(DeleteBehavior.Restrict);

                f.HasMany(f => f.RelationsForeign)
                    .WithOne(r => r.ForeignField)
                    .HasForeignKey(f => f.ForeignFieldId)
                    .OnDelete(DeleteBehavior.Restrict);

                f.HasMany(f => f.DtoFields)
                    .WithOne(df => df.SourceField)
                    .HasForeignKey(df => df.SourceFieldId);
            });
            #endregion

            #region FieldType
            modelBuilder.Entity<FieldType>(ft =>
            {
                ft.HasKey(ft => ft.Id);

                ft.HasOne(ft => ft.SourceType)
                    .WithMany(fts => fts.FieldTypes)
                    .HasForeignKey(ft => ft.SourceTypeId);

                ft.HasMany(ft => ft.Fields)
                    .WithOne(f => f.FieldType)
                    .HasForeignKey(f => f.FieldTypeId);

                ft.HasMany(ft => ft.MethodArgumentFields)
                    .WithOne(maf => maf.FieldType)
                    .HasForeignKey(f => f.FieldTypeId);

                ft.HasMany(ft => ft.MethodReturnFields)
                    .WithOne(mrf => mrf.FieldType)
                    .HasForeignKey(f => f.FieldTypeId);

                ft.HasData(
                   new FieldType { Id = 1, Name = "Int", SourceTypeId = 1 },
                   new FieldType { Id = 2, Name = "String", SourceTypeId = 1 },
                   new FieldType { Id = 3, Name = "Long", SourceTypeId = 1 },
                   new FieldType { Id = 4, Name = "Float", SourceTypeId = 1 },
                   new FieldType { Id = 5, Name = "Double", SourceTypeId = 1 },
                   new FieldType { Id = 6, Name = "Bool", SourceTypeId = 1 },
                   new FieldType { Id = 7, Name = "Char", SourceTypeId = 1 },
                   new FieldType { Id = 8, Name = "Byte", SourceTypeId = 1 },
                   new FieldType { Id = 9, Name = "DateTime", SourceTypeId = 1 },
                   new FieldType { Id = 10, Name = "DateOnly", SourceTypeId = 1 },
                   new FieldType { Id = 11, Name = "Guid", SourceTypeId = 1 }
                );
            });
            #endregion

            #region FieldTypeSource
            modelBuilder.Entity<FieldTypeSource>(fts =>
            {
                fts.HasKey(fts => fts.Id);

                fts.HasMany(fts => fts.FieldTypes)
                    .WithOne(ft => ft.SourceType)
                    .HasForeignKey(ft => ft.SourceTypeId);

                fts.HasData(
                     new FieldTypeSource { Id = 1, Name = "Base" },
                     new FieldTypeSource { Id = 2, Name = "Entity" },
                     new FieldTypeSource { Id = 3, Name = "Dto" }
                );
            });
            #endregion

            #region DeleteBehaviorType
            modelBuilder.Entity<DeleteBehaviorType>(dbt =>
            {
                dbt.HasKey(dbt => dbt.Id);

                dbt.HasMany(dbt => dbt.Relations)
                    .WithOne(r => r.DeleteBehaviorType)
                    .HasForeignKey(r => r.DeleteBehaviorTypeId);

                dbt.HasData(
                    new DeleteBehaviorType { Id = 1, Name = "Cascade" },
                    new DeleteBehaviorType { Id = 2, Name = "ClientCascade" },
                    new DeleteBehaviorType { Id = 3, Name = "Restrict" },
                    new DeleteBehaviorType { Id = 4, Name = "ClientSetNull" },
                    new DeleteBehaviorType { Id = 5, Name = "ClientNoAction" },
                    new DeleteBehaviorType { Id = 6, Name = "SetNull" },
                    new DeleteBehaviorType { Id = 7, Name = "NoAction" }
                );
            });
            #endregion

            #region Relation
            modelBuilder.Entity<Relation>(r =>
            {
                r.HasKey(r => r.Id);

                r.HasIndex(r => new { r.PrimaryFieldId, r.ForeignFieldId }).IsUnique();

                r.HasOne(r => r.PrimaryField)
                    .WithMany(f => f.RelationsPrimary)
                    .HasForeignKey(r => r.PrimaryFieldId);
                //.OnDelete(DeleteBehavior.Restrict);

                r.HasOne(r => r.ForeignField)
                    .WithMany(f => f.RelationsForeign)
                    .HasForeignKey(r => r.ForeignFieldId);
                //.OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict kullanıldı.

                r.HasOne(r => r.RelationType)
                    .WithMany(rt => rt.Relations)
                    .HasForeignKey(r => r.RelationTypeId);
                //.OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict kullanıldı.

                r.HasMany(r => r.DtoFieldRelations)
                  .WithOne(df => df.Relation)
                  .HasForeignKey(r => r.RelationId);

                r.HasOne(r => r.DeleteBehaviorType)
                    .WithMany(dbt => dbt.Relations)
                    .HasForeignKey(r => r.DeleteBehaviorTypeId);
            });
            #endregion

            #region RelationType
            modelBuilder.Entity<RelationType>(rt =>
            {
                rt.HasKey(rt => rt.Id);

                rt.HasMany(rt => rt.Relations)
                    .WithOne(r => r.RelationType)
                    .HasForeignKey(r => r.RelationTypeId);

                rt.HasData(
                    new RelationType { Id = (int)RelationTypeEnums.OneToOne, Name = "OnoToOne" },
                    new RelationType { Id = (int)RelationTypeEnums.OneToMany, Name = "OnoToMany" }
                );
            });
            #endregion

            #region CrudType
            modelBuilder.Entity<CrudType>(ct =>
            {
                ct.HasKey(ct => ct.Id);

                ct.HasMany(ct => ct.Dtos)
                    .WithOne(d => d.CrudType)
                    .HasForeignKey(d => d.CrudTypeId);

                ct.HasData(
                    new CrudType { Id = (int)CrudTypeEnums.Read, Name = "Read" },
                    new CrudType { Id = (int)CrudTypeEnums.Create, Name = "Create" },
                    new CrudType { Id = (int)CrudTypeEnums.Update, Name = "Update" },
                    new CrudType { Id = (int)CrudTypeEnums.Delete, Name = "Delete" }
                );
            });
            #endregion

            #region Dto
            modelBuilder.Entity<Dto>(d =>
            {
                d.HasKey(d => d.Id);

                d.HasOne(d => d.RelatedEntity)
                    .WithMany(r => r.Dtos)
                    .HasForeignKey(r => r.RelatedEntityId)
                    .OnDelete(DeleteBehavior.Restrict);

                d.HasOne(d => d.CrudType)
                    .WithMany(ct => ct.Dtos)
                    .HasForeignKey(d => d.CrudTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                d.HasMany(d => d.DtoFields)
                    .WithOne(r => r.Dto)
                    .HasForeignKey(r => r.DtoId);

                d.HasMany(e => e.CreateEntities)
                    .WithOne(cd => cd.CreateDto)
                    .HasForeignKey(e => e.CreateDtoId);

                d.HasMany(e => e.UpdateEntities)
                    .WithOne(cd => cd.UpdateDto)
                    .HasForeignKey(e => e.UpdateDtoId);

                d.HasMany(e => e.DeleteEntities)
                    .WithOne(cd => cd.DeleteDto)
                    .HasForeignKey(e => e.DeleteDtoId);

                d.HasMany(e => e.ReportEntities)
                    .WithOne(cd => cd.ReportDto)
                    .HasForeignKey(e => e.ReportDtoId);

                d.HasMany(e => e.BasicResponseEntities)
                    .WithOne(cd => cd.BasicResponseDto)
                    .HasForeignKey(e => e.BasicResponseDtoId);

                d.HasMany(e => e.DetailResponseEntities)
                    .WithOne(cd => cd.DetailResponseDto)
                    .HasForeignKey(e => e.DetailResponseDtoId);
            });
            #endregion

            #region DtoField
            modelBuilder.Entity<DtoField>(df =>
            {
                df.HasKey(df => df.Id);


                df.HasOne(df => df.Dto)
                   .WithMany(f => f.DtoFields)
                   .HasForeignKey(df => df.DtoId);

                df.HasOne(df => df.SourceField)
                   .WithMany(f => f.DtoFields)
                   .HasForeignKey(df => df.SourceFieldId);

                df.HasMany(df => df.Validations)
                   .WithOne(v => v.DtoField)
                   .HasForeignKey(v => v.DtoFieldId);

                df.HasMany(df => df.DtoFieldRelations)
                   .WithOne(dfr => dfr.DtoField)
                   .HasForeignKey(dfr => dfr.DtoFieldId);
            });
            #endregion

            #region DtoFieldRelations
            modelBuilder.Entity<DtoFieldRelations>(dfr =>
            {
                dfr.HasKey(dfr => new { dfr.DtoFieldId, dfr.RelationId, dfr.SequenceNo });

                dfr.HasOne(dfr => dfr.DtoField)
                   .WithMany(df => df.DtoFieldRelations)
                   .HasForeignKey(dfr => dfr.DtoFieldId);

                dfr.HasOne(df => df.Relation)
                   .WithMany(r => r.DtoFieldRelations)
                   .HasForeignKey(dfr => dfr.RelationId);
            });
            #endregion

            #region Validation
            modelBuilder.Entity<Validation>(v =>
            {
                v.HasKey(v => v.Id);

                v.HasOne(v => v.DtoField)
                    .WithMany(df => df.Validations)
                    .HasForeignKey(v => v.DtoFieldId);

                v.HasOne(v => v.ValidatorType)
                    .WithMany(vt => vt.Validations)
                    .HasForeignKey(v => v.ValidatorTypeId);

                v.HasMany(v => v.ValidationParams)
                    .WithOne(vp => vp.Validation)
                    .HasForeignKey(vp => vp.ValidationId);
            });
            #endregion

            #region ValidationParam
            modelBuilder.Entity<ValidationParam>(vp =>
            {
                vp.HasKey(vp => new { vp.ValidationId, vp.ValidatorTypeParamId });

                vp.HasOne(vp => vp.Validation)
                    .WithMany(v => v.ValidationParams)
                    .HasForeignKey(v => v.ValidationId);

                vp.HasOne(vp => vp.ValidatorTypeParam)
                    .WithMany(vtp => vtp.ValidationParams)
                    .HasForeignKey(v => v.ValidatorTypeParamId);
            });
            #endregion

            #region ValidatorType
            modelBuilder.Entity<ValidatorType>(vt =>
            {
                vt.HasKey(vt => vt.Id);

                vt.HasMany(vt => vt.Validations)
                    .WithOne(v => v.ValidatorType)
                    .HasForeignKey(v => v.ValidatorTypeId);

                vt.HasMany(vt => vt.ValidatorTypeParams)
                    .WithOne(vtp => vtp.ValidatorType)
                    .HasForeignKey(vtp => vtp.ValidatorTypeId)
                    .OnDelete(DeleteBehavior.Restrict);


                vt.HasData(
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.NotEmpty, Name = "NotEmpty", Description = "Field cannot be empty" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.NotNull, Name = "NotNull", Description = "Field cannot be null" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.NotEqual, Name = "NotEqual", Description = "Field cannot be ..." },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.MaxLength, Name = "MaxLength", Description = "Field cannot exceed maximum length" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Range, Name = "Range", Description = "Value must be within a specific range" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.MinLength, Name = "MinLength", Description = "Field must have a minimum number of characters" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Regex, Name = "Regex", Description = "Field must match a regular expression" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.GreaterThan, Name = "GreaterThan", Description = "Value must be greater than a specific number" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.LessThan, Name = "LessThan", Description = "Value must be less than a specific number" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.EmailAddress, Name = "EmailAddress", Description = "Field must be a valid email address" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.CreditCard, Name = "CreditCard", Description = "Field must be a valid credit card number" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Phone, Name = "Phone", Description = "Field must be a valid phone number" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Url, Name = "Url", Description = "Field must be a valid URL" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Date, Name = "Date", Description = "Field must be a valid date" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Number, Name = "Number", Description = "Field must be a valid number" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.GuidNotEmpty, Name = "GuidNotEmpty", Description = "Field mus be a valid guid value" },
                    new ValidatorType { Id = (int)Models.Enums.ValidatorTypes.Length, Name = "Length", Description = "Field must have a exact number of characters\", \"Length" }
                );
            });
            #endregion

            #region ValidatorTypeParam
            modelBuilder.Entity<ValidatorTypeParam>(vtp =>
            {
                vtp.HasKey(vt => vt.Id);

                vtp.HasOne(vtp => vtp.ValidatorType)
                    .WithMany(v => v.ValidatorTypeParams)
                    .HasForeignKey(vtp => vtp.ValidatorTypeId);

                vtp.HasMany(vtp => vtp.ValidationParams)
                    .WithOne(vp => vp.ValidatorTypeParam)
                    .HasForeignKey(vp => vp.ValidatorTypeParamId);

                vtp.HasData(
                    // NotEmpty Validator (no additional params needed)
                    // NotNull Validator (no additional params needed)
                    // NotEqual Validator
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.NotEqual_Value, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.NotEqual, Key = "Value" },
                    // MaxLength Validator max
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.MaxLength_Max, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.MaxLength, Key = "Max" },
                    // Range Validator min, max
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.Range_Min, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.Range, Key = "Min" },
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.Range_Max, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.Range, Key = "Max" },
                    // MinLength Validator min
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.MinLength_Min, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.MinLength, Key = "Min" },
                    // Regex Validator pattern
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.Regex_Pattern, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.Regex, Key = "Pattern" },
                    // GreaterThan Validator
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.GreaterThan_Value, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.GreaterThan, Key = "Value" },
                    // LessThan Validator
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.LessThan_Value, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.LessThan, Key = "Value" },
                    // EmailAddress Validator (no additional params needed)
                    // CreditCard Validator (no additional params needed)
                    // Phone Validator (no additional params needed)
                    // Url Validator (no additional params needed)
                    // Date Validator (no additional params needed)
                    // Number Validator (no additional params needed)
                    // GuidNotEmpty Validator (no additional params needed)
                    // LessThan Validator
                    new ValidatorTypeParam { Id = (int)Models.Enums.ValidatorTypeParams.Length_Value, ValidatorTypeId = (int)Models.Enums.ValidatorTypes.Length, Key = "Value" }
                );
            });
            #endregion

            #region Service
            modelBuilder.Entity<Service>(s =>
            {
                s.HasKey(s => s.Id);

                s.HasIndex(s => new { s.ServiceLayerId, s.RelatedEntityId }).IsUnique();

                s.HasOne(s => s.ServiceLayer)
                    .WithMany(sl => sl.Services)
                    .HasForeignKey(s => s.ServiceLayerId);

                s.HasOne(s => s.RelatedEntity)
                    .WithMany(e => e.Services)
                    .HasForeignKey(s => s.RelatedEntityId);

                s.HasMany(s => s.Methods)
                    .WithOne(m => m.Service)
                    .HasForeignKey(m => m.ServiceId);
            });
            #endregion

            #region ServiceLayer
            modelBuilder.Entity<ServiceLayer>(sl =>
            {
                sl.HasKey(sl => sl.Id);

                sl.HasMany(sl => sl.Services)
                    .WithOne(s => s.ServiceLayer)
                    .HasForeignKey(s => s.ServiceLayerId);

                sl.HasData(
                    new ServiceLayer { Id = 1, Name = "Core" },
                    new ServiceLayer { Id = 2, Name = "Model" },
                    new ServiceLayer { Id = 3, Name = "DataAccess" },
                    new ServiceLayer { Id = 4, Name = "Business" },
                    new ServiceLayer { Id = 5, Name = "Presentation" }
                );
            });
            #endregion

            #region Method
            modelBuilder.Entity<Method>(m =>
            {
                m.HasKey(m => m.Id);

                m.HasOne(m => m.Service)
                    .WithMany(s => s.Methods)
                    .HasForeignKey(m => m.ServiceId);

                m.HasOne(m => m.MethodReturnField)
                    .WithOne(rmf => rmf.Method)
                    .HasForeignKey<Method>(m => m.MethodReturnFieldId);

                m.HasMany(m => m.ArgumentMethodFields)
                    .WithOne(maf => maf.Method)
                    .HasForeignKey(maf => maf.MethodId);
            });
            #endregion

            #region MethodArgumentField
            modelBuilder.Entity<MethodArgumentField>(maf =>
            {
                maf.HasKey(maf => maf.Id);

                maf.HasOne(maf => maf.Method)
                    .WithMany(m => m.ArgumentMethodFields)
                    .HasForeignKey(maf => maf.MethodId);

                maf.HasOne(maf => maf.FieldType)
                    .WithMany(ft => ft.MethodArgumentFields)
                    .HasForeignKey(maf => maf.FieldTypeId);
            });
            #endregion

            #region MethodReturnField
            modelBuilder.Entity<MethodReturnField>(mrf =>
            {
                mrf.HasKey(mrf => mrf.Id);

                mrf.HasIndex(mrf => mrf.MethodId).IsUnique();


                mrf.HasOne(mrf => mrf.Method)
                    .WithOne(m => m.MethodReturnField)
                    .HasForeignKey<MethodReturnField>(mrf => mrf.FieldTypeId);

                mrf.HasOne(mrf => mrf.FieldType)
                    .WithMany(ft => ft.MethodReturnFields)
                    .HasForeignKey(mrf => mrf.FieldTypeId);
            });
            #endregion
        }
    }
}
