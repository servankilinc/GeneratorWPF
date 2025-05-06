using GeneratorWPF.Context;
using GeneratorWPF.Dtos._DtoField;
using GeneratorWPF.Dtos._Validation;
using GeneratorWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GeneratorWPF.Repository
{
    public class DtoFieldRepository : EFRepositoryBase<DtoField>
    {
        public List<DtoField> GetBySourceField(Expression<Func<DtoField, bool>> expresion)
        {
            using var context = new LocalContext();
            return context.DtoFields
                    .Where(expresion)
                    .Include(i => i.SourceField)
                        .ThenInclude(ti => ti.FieldType)
                    .ToList();
        }


        public List<ValidationResponse> GetValidations(int dtoId)
        {
            using var context = new LocalContext();
            var data = context.DtoFields
                    .Where(f => f.DtoId == dtoId)
                    .Include(i => i.SourceField)
                    .Include(i => i.Validations)
                        .ThenInclude(i => i.ValidatorType)
                            .ThenInclude(i => i.ValidatorTypeParams)
                    .Include(i => i.Validations)
                        .ThenInclude(i => i.ValidationParams)
                            .ThenInclude(i => i.ValidatorTypeParam)
                    //.Select(d => d.Validations)
                    .ToList();

            var validationList = new List<ValidationResponse>();
            if (data == null) return validationList;

            foreach (var item in data)
            {
                if (item.Validations != null)
                {
                    validationList.AddRange(item.Validations.Select(x => new ValidationResponse
                    {
                        ValidationId = x.Id,
                        DtoFieldId = item.Id,
                        ValidatorTypeName = x.ValidatorType.Name,
                        ErrorMessage = x.ErrorMessage,
                        ValidationParams = x.ValidationParams,
                        DtoId = item.DtoId,
                        SourceFieldName = item.SourceField.Name,
                        DtoFieldName = item.Name,
                    }));
                }
            }
            return validationList;
        }

        public void Add(DtoFieldCreateDto createDto)
        {
            using var context = new LocalContext();
            var data = new DtoField
            {
                DtoId = createDto.DtoId,
                Name = createDto.Name,
                SourceFieldId = createDto.SourceFieldId,
                IsRequired = createDto.IsRequired,
                IsList = createDto.IsList
            };
            
            context.Add(data);
            context.SaveChanges();
        }

        public void Update(DtoFieldUpdateDto updateDto)
        {
            using var context = new LocalContext();
            var existData = context.DtoFields.FirstOrDefault(f => f.Id == updateDto.Id);
            if (existData == null) throw new Exception("Data to update not found!");

            existData.Name = updateDto.Name;
            existData.SourceFieldId = updateDto.SourceFieldId;
            existData.IsRequired = updateDto.IsRequired;
            existData.IsList = updateDto.IsList;

            context.Update(existData);
            context.SaveChanges();
        }

        public void Delete(int id)
        {
            using var context = new LocalContext();
            var existData = context.DtoFields.FirstOrDefault(f => f.Id == id);

            if (existData == null) throw new Exception("Data not found!");

            context.Remove(existData);
            context.SaveChanges();
        }
    }
}
