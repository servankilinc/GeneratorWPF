using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class CrudType : EntityBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Dto>? Dtos { get; set; }
    }
}