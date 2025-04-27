using GeneratorWPF.Dtos._MethodArgumentField;
using GeneratorWPF.Dtos._MethodReturnField;

namespace GeneratorWPF.Dtos._Method
{
    public class MethodCreateDto
    {
        public int ServiceId { get; set; }
        public bool IsVoid { get; set; }
        public string Name { get; set; } = null!;
        public bool IsAsync { get; set; }
        public string? Description { get; set; }

        public MethodReturnFieldCreateDto? MethodReturnField { get; set; }
        public List<MethodArgumentFieldCreateDto>? MethodArgumentFields { get; set; }
    }
}