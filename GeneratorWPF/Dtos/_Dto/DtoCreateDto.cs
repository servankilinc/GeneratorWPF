using GeneratorWPF.Dtos._DtoField;
using System.Collections.ObjectModel;

namespace GeneratorWPF.Dtos._Dto;

public class DtoCreateDto
{
    public string Name { get; set; } = null!;
    public int RelatedEntityId { get; set; }
    public int CrudTypeId { get; set; }
    public ObservableCollection<DtoFieldCreateDto> DtoFields { get; set; } = null!;
}
