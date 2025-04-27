using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Field : EntityBase
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public int FieldTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsList { get; set; }

        public Entity Entity { get; set; } = null!;
        public FieldType FieldType { get; set; } = null!;
        public virtual ICollection<Relation> RelationsPrimary { get; set; } = null!;
        public virtual ICollection<Relation> RelationsForeign { get; set; } = null!;
        public virtual ICollection<DtoField> DtoFields { get; set; } = null!;
    }
}
