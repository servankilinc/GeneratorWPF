namespace GeneratorWPF.Dtos._ValidationParam
{
    public class ValidationParamUpdateDto
    {
        public int ValidationParamId { get; set; }
        public string? Key { get; set; }
        public int ValidationId { get; set; }
        public int ValidatorTypeParamId { get; set; }
        public string Value { get; set; } = null!;
    }
}