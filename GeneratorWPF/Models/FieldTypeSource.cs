using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class FieldTypeSource : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<FieldType>? FieldTypes { get; set; }
    }
}
