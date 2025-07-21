using GeneratorWPF.Models.Enums;
using GeneratorWPF.Models.Signature;
using System.Linq;

namespace GeneratorWPF.Models
{
    public class Field : EntityBase
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public int FieldTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsUnique { get; set; }
        public bool IsRequired { get; set; }
        public bool IsList { get; set; }
        public bool Filterable { get; set; }

        public Entity Entity { get; set; } = null!;
        public FieldType FieldType { get; set; } = null!;
        public virtual ICollection<Relation> RelationsPrimary { get; set; } = null!;
        public virtual ICollection<Relation> RelationsForeign { get; set; } = null!;
        public virtual ICollection<DtoField> DtoFields { get; set; } = null!;

        public string GetMapedTypeName()
        {
            return this.FieldTypeId switch
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
                _ => this.Name
            };
        }

        /// <summary>
        /// 1: Select
        /// 2: Number
        /// 3: Text
        /// 4: CheckBox
        /// 5: DateTime
        /// 6: Undefined
        /// </summary>
        /// <returns></returns>
        public int GetVariableGroup(Dictionary<string, string> selectableRelations) // key: fieldName, value: entityName
        {
            if (selectableRelations.Any(f => f.Key.Trim().ToLower() == this.Name.Trim().ToLower())) // || this.FieldTypeId == (byte)FieldTypeEnums.Int || this.FieldTypeId == (byte)FieldTypeEnums.Guid
            {
                return 1;
            }
            else if (
                this.FieldTypeId == (byte)FieldTypeEnums.Int ||
                this.FieldTypeId == (byte)FieldTypeEnums.Double ||
                this.FieldTypeId == (byte)FieldTypeEnums.Float ||
                this.FieldTypeId == (byte)FieldTypeEnums.Byte ||
                this.FieldTypeId == (byte)FieldTypeEnums.Long)
            {
                return 2;
            }
            else if (this.FieldTypeId == (byte)FieldTypeEnums.String && this.FieldTypeId == (byte)FieldTypeEnums.Char)
            {
                return 3;
            }
            else if (this.FieldTypeId == (byte)FieldTypeEnums.Bool)
            {
                return 4;
            }
            else if (this.FieldTypeId == (byte)FieldTypeEnums.DateOnly || this.FieldTypeId == (byte)FieldTypeEnums.DateTime)
            {
                return 5;
            }

            return 0;
        }

        /// <summary>
        /// 1: Select
        /// 2: Number
        /// 3: Text
        /// 4: CheckBox
        /// 5: DateTime
        /// 6: Undefined
        /// </summary>
        /// <returns></returns>
        public int GetVariableGroup() // key: fieldName, value: entityName
        {
            if (this.FieldTypeId == (byte)FieldTypeEnums.Int || this.FieldTypeId == (byte)FieldTypeEnums.Guid)
            {
                return 1;
            }
            else if (
                this.FieldTypeId == (byte)FieldTypeEnums.Int ||
                this.FieldTypeId == (byte)FieldTypeEnums.Double ||
                this.FieldTypeId == (byte)FieldTypeEnums.Float ||
                this.FieldTypeId == (byte)FieldTypeEnums.Byte ||
                this.FieldTypeId == (byte)FieldTypeEnums.Long)
            {
                return 2;
            }
            else if (this.FieldTypeId == (byte)FieldTypeEnums.String && this.FieldTypeId == (byte)FieldTypeEnums.Char)
            {
                return 3;
            }
            else if (this.FieldTypeId == (byte)FieldTypeEnums.Bool)
            {
                return 4;
            }
            else if (this.FieldTypeId == (byte)FieldTypeEnums.DateOnly || this.FieldTypeId == (byte)FieldTypeEnums.DateTime)
            {
                return 5;
            }

            return 0;
        }
    }
}
