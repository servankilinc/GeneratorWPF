using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class ValidationParam : EntityBase
    {
        public int ValidationId { get; set; }
        public int ValidatorTypeParamId { get; set; }
        public string Value { get; set; } = null!;

        public Validation Validation { get; set; } = null!;
        public ValidatorTypeParam ValidatorTypeParam { get; set; } = null!;
    }
}