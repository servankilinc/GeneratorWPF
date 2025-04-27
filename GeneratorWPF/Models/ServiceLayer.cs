using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class ServiceLayer : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Service>? Services { get; set; }
    }
}
