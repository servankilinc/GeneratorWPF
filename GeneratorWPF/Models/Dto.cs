using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Dto : EntityBase
    {
        public int Id { get; set; }
        public int RelatedEntityId { get; set; }
        public string Name { get; set; } = null!;
        
        public Entity RelatedEntity { get; set; } = null!;
        public List<DtoField> DtoFields { get; set; } = null!;
    }
}