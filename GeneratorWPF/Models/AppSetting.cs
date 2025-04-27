using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class AppSetting : EntityBase
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string SolutionName { get; set; } = null!;
        public string Path { get; set; } = null!;
    }
}