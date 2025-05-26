using GeneratorWPF.Context;
using GeneratorWPF.Dtos._Dto;
using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GeneratorWPF.Repository
{
    public class DtoRepository : EFRepositoryBase<Dto>
    {
        public DtoField GetDtoFieldByValidations(Expression<Func<DtoField, bool>> expresion)
        {
            using var context = new LocalContext();
            return context.DtoFields
                    .Where(expresion)
                    .Include(i => i.Validations)
                        .ThenInclude(i => i.ValidatorType)
                            .ThenInclude(i=> i.ValidatorTypeParams)
                    .Include(i => i.Validations)
                        .ThenInclude(i => i.ValidationParams)
                            .ThenInclude(i => i.ValidatorTypeParam)
                    .FirstOrDefault();
        }


        public List<Dto> GetListByValidations(Expression<Func<Dto, bool>> expresion)
        {
            using var context = new LocalContext();
            return context.Dtos
                    .Where(expresion)
                    .Include(i => i.RelatedEntity)
                    .Include(i => i.DtoFields)
                        .ThenInclude(df => df.Validations)
                            .ThenInclude(v => v.ValidatorType)
                    .Include(i => i.DtoFields)
                        .ThenInclude(df => df.Validations)
                            .ThenInclude(v => v.ValidationParams)
                                .ThenInclude(vp => vp.ValidatorTypeParam)
                    .ToList();
        }


        public List<Dto> GetList(Expression<Func<Dto, bool>> expresion)
        {
            using var context = new LocalContext();
            return context.Dtos
                    .Where(expresion)
                    .Include(i => i.RelatedEntity)
                    .Include(i => i.DtoFields)
                        .ThenInclude(df => df.SourceField)
                            .ThenInclude(sf => sf.FieldType)
                    .ToList();
        }

        public void CreateByFields(DtoCreateDto createDto)
        {
            using var _context = new LocalContext();
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Insert Dto
                var insertedDto = _context.Dtos.Add(new Dto()
                {
                    Name = createDto.Name,
                    RelatedEntityId = createDto.RelatedEntityId,
                }).Entity;
                _context.SaveChanges();

                // Insert Dto as FiledType
                var insertedFieldType = _context.FieldTypes.Add(new FieldType
                {
                    Name = insertedDto.Name,
                    SourceTypeId = (int)FieldTypeSourceEnums.Dto,
                }).Entity;
                _context.SaveChanges();

                // Insert Field
                var insertedField = _context.Fields.Add(new Field()
                {
                    EntityId = createDto.RelatedEntityId,
                    FieldTypeId = insertedFieldType.Id,
                    Name = createDto.Name,
                }).Entity;
                _context.SaveChanges();

                // Insert DtoFields
                foreach (var sourceField in createDto.DtoFields)
                {
                    _context.DtoFields.Add(new DtoField
                    {
                        DtoId = insertedDto.Id,
                        SourceFieldId = sourceField.SourceFieldId,
                        Name = sourceField.Name,
                    });
                }
                _context.SaveChanges();

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Update(DtoUpdateDto updateModel)
        {
            using var context = new LocalContext();
            var existData = context.Dtos.FirstOrDefault(f => f.Id == updateModel.Id);
            if(existData == null) throw new Exception("Data to update not found!");

            bool nameChanged = existData.Name != updateModel.Name;
            bool entityIdChanged = existData.RelatedEntityId != updateModel.RelatedEntityId;

            var existFieldType = context.FieldTypes.FirstOrDefault(f => f.Name == existData.Name && f.SourceTypeId == (int)FieldTypeSourceEnums.Dto);
            var existField = context.Fields.FirstOrDefault(f => f.Name == existData.Name && f.EntityId == existData.RelatedEntityId);
            if (nameChanged)
            {
                if (existFieldType != null) existFieldType.Name = updateModel.Name;
                if (existField != null) existField.Name = updateModel.Name;

                existData.Name = updateModel.Name;
            }
            if (entityIdChanged)
            {
                if (existField != null) existField.EntityId = updateModel.RelatedEntityId;
                
                existData.RelatedEntityId = updateModel.RelatedEntityId;
            }

            if (existField != null) context.Update(existField);
            if (existFieldType != null) context.Update(existFieldType);

            context.Update(existData);
            context.SaveChanges();
        }

        public void Delete(int id)
        {
            using var context = new LocalContext();
            var existData = context.Dtos.FirstOrDefault(f => f.Id == id);

            if (existData == null) throw new Exception("Data not found!");

            context.Remove(existData);
            context.SaveChanges();
        }
    }
}
