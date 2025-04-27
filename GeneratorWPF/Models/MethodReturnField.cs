using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class MethodReturnField : EntityBase
    {
        public int Id { get; set; }
        public int MethodId { get; set; }
        public int FieldTypeId { get; set; }
        public bool IsList { get; set; }

        public FieldType FieldType { get; set; } = null!;
        public Method Method { get; set; } = null!;
    }
}
