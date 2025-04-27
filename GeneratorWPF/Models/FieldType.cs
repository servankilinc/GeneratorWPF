using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class FieldType : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int SourceTypeId { get; set; }

        public FieldTypeSource? SourceType { get; set; }
        public virtual ICollection<Field>? Fields { get; set; }
        public virtual ICollection<MethodArgumentField>? MethodArgumentFields { get; set; }
        public virtual ICollection<MethodReturnField>? MethodReturnFields { get; set; }
    }
}
