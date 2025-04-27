using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Method : EntityBase
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Name { get; set; } = null!;
        public int? MethodReturnFieldId { get; set; }
        public bool IsVoid { get; set; }
        public bool IsAsync { get; set; }
        public string? Description { get; set; }

        public Service Service { get; set; } = null!;
        public MethodReturnField? MethodReturnField { get; set; }
        public virtual ICollection<MethodArgumentField>? ArgumentMethodFields { get; set; }
    }
}
