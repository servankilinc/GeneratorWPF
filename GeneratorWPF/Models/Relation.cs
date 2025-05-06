using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Relation : EntityBase
    {
        public int Id { get; set; }
        public int PrimaryFieldId { get; set; }
        public int ForeignFieldId { get; set; }
        public int RelationTypeId { get; set; }
        public int DeleteBehaviorTypeId { get; set; }

        public virtual Field PrimaryField { get; set; } = null!;
        public virtual Field ForeignField { get; set; } = null!;
        public virtual RelationType RelationType { get; set; } = null!;
        public virtual DeleteBehaviorType DeleteBehaviorType { get; set; } = null!;
    }
}
