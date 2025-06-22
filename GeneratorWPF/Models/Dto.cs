using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Dto : EntityBase
    {
        public int Id { get; set; }
        public int RelatedEntityId { get; set; }
        public int CrudTypeId { get; set; }
        public string Name { get; set; } = null!;
        
        public virtual CrudType CrudType { get; set; } = null!;
        public virtual Entity RelatedEntity { get; set; } = null!;
        public virtual ICollection<DtoField> DtoFields { get; set; } = null!;
        public virtual ICollection<Entity> CreateEntities { get; set; } = null!;
        public virtual ICollection<Entity> UpdateEntities { get; set; } = null!;
        public virtual ICollection<Entity> DeleteEntities { get; set; } = null!;
        public virtual ICollection<Entity> BasicResponseEntities { get; set; } = null!;
        public virtual ICollection<Entity> DetailResponseEntities { get; set; } = null!;
    }
}