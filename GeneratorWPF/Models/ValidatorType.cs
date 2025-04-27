using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class ValidatorType : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Validation>? Validations { get; set; }
        public virtual ICollection<ValidatorTypeParam>? ValidatorTypeParams { get; set; }
    }
}