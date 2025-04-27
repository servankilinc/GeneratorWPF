using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Validation : EntityBase
    {
        public int Id { get; set; }
        public int DtoFieldId { get; set; }
        public int ValidatorTypeId { get; set; }
        public string? ErrorMessage { get; set; }

        public DtoField DtoField { get; set; } = null!;
        public ValidatorType ValidatorType { get; set; } = null!;
        public virtual ICollection<ValidationParam>? ValidationParams { get; set; }
    }
}