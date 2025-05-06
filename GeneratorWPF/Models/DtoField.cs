using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class DtoField : EntityBase
    {
        public int Id { get; set; }
        public int DtoId { get; set; }
        public int SourceFieldId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsList { get; set; }

        public Dto Dto { get; set; } = null!;
        public Field SourceField { get; set; } = null!;
        public virtual ICollection<Validation>? Validations { get; set; }
    }
}