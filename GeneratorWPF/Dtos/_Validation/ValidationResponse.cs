using GeneratorWPF.Models;

namespace GeneratorWPF.Dtos._Validation;

public class ValidationResponse
{
    public int ValidationId { get; set; }
    public int DtoFieldId { get; set; }
    public string ValidatorTypeName { get; set; } = null!;
    public string? ErrorMessage { get; set; }
    public virtual ICollection<ValidationParam>? ValidationParams { get; set; }

    // DtoField
    public int DtoId { get; set; }
    public string SourceFieldName { get; set; } = null!;
    public string DtoFieldName { get; set; } = null!;
}
