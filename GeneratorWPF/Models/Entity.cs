using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Entity : EntityBase
    {
        public int Id { get; set; }
        public string TableName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<Field> Fields { get; set; } = null!;
        public List<Dto>? Dtos { get; set; }
        public List<Service>? Services { get; set; }
    }
}
