using GeneratorWPF.Context;
using GeneratorWPF.Dtos._Field;
using GeneratorWPF.Models;
using Microsoft.EntityFrameworkCore;

namespace GeneratorWPF.Repository
{
    public class FieldRepository : EFRepositoryBase<Field>
    {
        public Field Update(FieldUpdateDto updateDto)
        {
            using var context = new LocalContext();

            var existData = context.Fields.FirstOrDefault(f => f.Id == updateDto.Id);
            if (existData == null) throw new Exception("Data to update not found");
              
            existData.FieldTypeId = updateDto.FieldTypeId;
            existData.Name = updateDto.Name;
            existData.IsRequired = updateDto.IsRequired;
            existData.IsUnique = updateDto.IsUnique;
            existData.IsList = updateDto.IsList;

            context.Fields.Update(existData);
            context.SaveChanges();
            return existData;
        }

        public Field Add(FieldCreateDto fieldCreateDto)
        {
            using var context = new LocalContext();

            var data = new Field
            {
                FieldTypeId = fieldCreateDto.FieldTypeId,
                EntityId = fieldCreateDto.EntityId,
                Name = fieldCreateDto.Name,
                IsRequired = fieldCreateDto.IsRequired,
                IsUnique = fieldCreateDto.IsUnique,
                IsList = fieldCreateDto.IsList,
            };
            context.Set<Field>().Add(data);
            context.SaveChanges();
            return data;
        }

        public string GetFieldName(int fieldId)
        {
            using var context = new LocalContext();

            var name = context.Fields.Where(f => f.Id == fieldId).Select(d => d.Name).FirstOrDefault();
 
            return name;
        }
    }
}
