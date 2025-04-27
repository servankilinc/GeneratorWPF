using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class RelationType : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Relation>? Relations { get; set; }
    }
}
