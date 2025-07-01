using GeneratorWPF.Dtos._DtoField;
using GeneratorWPF.Models;

namespace GeneratorWPF.Dtos._Dto
{
    public class DtoUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int RelatedEntityId { get; set; }
        public int CrudTypeId { get; set; }
    }

    public class DtoDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string RelatedEntityName { get; set; } = null!;
        public string CrudTypeName { get; set; } = null!;
        
        public virtual ICollection<DtoFieldResponseDto> DtoFields { get; set; } = null!;
    } 
}
