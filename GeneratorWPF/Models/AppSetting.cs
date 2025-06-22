using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class AppSetting : EntityBase
    {
        public int Id { get; set; }
        public string? ProjectName { get; set; }
        public string? SolutionName { get; set; }
        public string? Path { get; set; }
        public string? DBConnectionString { get; set; }

        public bool IsThereIdentiy { get; set; }

        public bool IsThereUser { get; set; }
        public int? UserEntityId { get; set; }
        public bool IsThereRole { get; set; }
        public int? RoleEntityId { get; set; }

        public virtual Entity? UserEntity { get; set; }
        public virtual Entity? RoleEntity { get; set; }
    }
}