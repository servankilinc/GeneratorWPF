using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class MethodArgumentField : EntityBase
    {
        public int Id { get; set; }
        public int MethodId { get; set; }
        public int FieldTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsList { get; set; }

        public Method Method { get; set; } = null!;
        public FieldType FieldType { get; set; } = null!;
    }
}