using GeneratorWPF.Dtos._Field;
using System.Collections.ObjectModel;

namespace GeneratorWPF.Dtos._Entity
{
    public class EntityCreateDto
    {
        public string Name { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public List<FieldCreateDto> Fields { get; set; } = null!;
    }
}
