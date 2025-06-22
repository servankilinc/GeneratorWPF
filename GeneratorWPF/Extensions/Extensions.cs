using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;

namespace GeneratorWPF.Extensions
{
    public static class Extensions
    {
        public static string MapFieldTypeName(this Field field)
        {
            return field.FieldTypeId switch
            {
                (int)FieldTypeEnums.Int => "int",
                (int)FieldTypeEnums.String => "string",
                (int)FieldTypeEnums.Long => "long",
                (int)FieldTypeEnums.Float => "float",
                (int)FieldTypeEnums.Double => "double",
                (int)FieldTypeEnums.Bool => "bool",
                (int)FieldTypeEnums.Char => "char",
                (int)FieldTypeEnums.Byte => "byte",
                (int)FieldTypeEnums.DateTime => "DateTime",
                (int)FieldTypeEnums.DateOnly => "DateOnly",
                (int)FieldTypeEnums.Guid => "Guid",
                _ => "dynamic"
            };
        }
    }
}
