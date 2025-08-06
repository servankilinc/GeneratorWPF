using GeneratorWPF.Dtos._Method;
using GeneratorWPF.Context;
using GeneratorWPF.Models;

namespace GeneratorWPF.Repository
{
    public class MethodRepository : EFRepositoryBase<Method>
    {
        public void CreateByFields(MethodCreateDto createDto)
        {
            using var _context = new ProjectContext();
            using var transaction = _context.Database.BeginTransaction();
            try
            {

                // Insert Method
                var insertedMethod = _context.Methods.Add(new Method
                {
                    ServiceId = createDto.ServiceId,
                    Name = createDto.Name,
                    IsVoid = createDto.IsVoid,
                    IsAsync = createDto.IsAsync,
                    Description = createDto.Description,
                }).Entity;
                _context.SaveChanges();


                // Insert ReturnField if exist
                if(createDto.IsVoid == false && createDto.MethodReturnField != null)
                {
                    var insertedReturnField = _context.MethodReturnFields.Add(new MethodReturnField
                    {
                        MethodId = insertedMethod.Id,
                        FieldTypeId = createDto.MethodReturnField.FieldTypeId,
                        IsList = createDto.MethodReturnField.IsList
                    }).Entity;

                    insertedMethod.MethodReturnFieldId = insertedReturnField.Id;
                    _context.Update(insertedMethod);

                    _context.SaveChanges();
                }


                // Insert ArgumentFields if exist
                if (createDto.MethodArgumentFields != null)
                {
                    foreach (var sourceField in createDto.MethodArgumentFields)
                    {
                        _context.MethodArgumentFields.Add(new MethodArgumentField
                        {
                            MethodId = insertedMethod.Id,
                            FieldTypeId = sourceField.FieldTypeId,
                            IsList = sourceField.IsList,
                            Name = sourceField.Name,
                        });
                    }

                    _context.SaveChanges();
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
