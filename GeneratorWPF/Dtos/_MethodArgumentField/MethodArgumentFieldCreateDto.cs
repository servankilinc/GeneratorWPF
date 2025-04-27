namespace GeneratorWPF.Dtos._MethodArgumentField
{
    public class MethodArgumentFieldCreateDto
    {
        public int MethodId { get; set; }
        public int FieldTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsList { get; set; }
    }
}