namespace GeneratorWPF.Dtos._DtoField
{
    public class DtoFieldUpdateDto
    {
        public int Id { get; set; }
        public int SourceFieldId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsList { get; set; }
    }
}
