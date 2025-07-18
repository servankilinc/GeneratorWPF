using GeneratorWPF.Models.Enums;
using GeneratorWPF.Models.Signature;

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
    }
}
