namespace GeneratorWPF.Dtos._ValidationParam
{
    public class ValidationParamCreateDto
    {
        public string? Key { get; set; }
        public int ValidationId { get; set; }
        public int ValidatorTypeParamId { get; set; }
        public string Value { get; set; } = null!;
    }
}