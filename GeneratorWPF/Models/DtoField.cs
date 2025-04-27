using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class DtoField : EntityBase
    {
        public int Id { get; set; }
        public int DtoId { get; set; }
        public int SourceFieldId { get; set; }
        public string Name { get; set; } = null!;

        public Dto Dto { get; set; } = null!;
        public Field SourceField { get; set; } = null!;
        public virtual ICollection<Validation>? Validations { get; set; }
    }


    //public class DtoFieldBasicModel
    //{
    //    public int Id { get; set; }
    //    public int DtoId { get; set; }
    //    public int SourceFieldId { get; set; }
    //    public string Name { get; set; } = null!;
    //    public string SourceFieldName { get; set; } = null!;

    //}
}