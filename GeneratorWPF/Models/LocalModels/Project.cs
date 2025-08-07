using System.ComponentModel.DataAnnotations;

namespace GeneratorWPF.Models.LocalModels
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string ProjectName { get; set; } = $"My Project {DateTime.Now:yyyy_MM_dd}";
        public DateTime CreateDate { get; set; } 
    }
}
