﻿using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
using Humanizer;

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
                _ => field.FieldType != null ? field.FieldType.Name : "dynmaic"
            };
        }

        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;


            if (input.Length < 2)
                return input.ToLowerInvariant();

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        public static string ToForeignFieldSlectListName(this string input, string entityName)
        {
            if (string.IsNullOrEmpty(input))
                return input;


            if (input.Length < 2)
                return input.ToLowerInvariant();

            if (input.Trim().ToLowerInvariant().EndsWith("id"))
            {
                return input.Trim().Substring(0, input.Length - 2) + "List";
            }
            else
            {
                return $"{entityName}List";
            }
        }


        public static string DivideToLabelName(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();

            // Eğer sonu "id" ile bitiyorsa kaldır (case-insensitive)
            if (input.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(0, input.Length - 2);
            }

            // Humanizer ile camelCase’i "Customer Order" gibi yap
            var result = input.Humanize(LetterCasing.Title);

            return result;
        }
    }
}
