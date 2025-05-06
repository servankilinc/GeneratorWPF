using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models;

public class DeleteBehaviorType : EntityBase
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public virtual ICollection<Relation>? Relations { get; set; }
}
