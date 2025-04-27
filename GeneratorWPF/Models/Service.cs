using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Service : EntityBase
    {
        public int Id { get; set; }
        public int ServiceLayerId { get; set; }
        public int RelatedEntityId { get; set; }

        public ServiceLayer ServiceLayer { get; set; } = null!;
        public Entity RelatedEntity { get; set; } = null!;
        public virtual ICollection<Method>? Methods { get; set; }
    }
}
