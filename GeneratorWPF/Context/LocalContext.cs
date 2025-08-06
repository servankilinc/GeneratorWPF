using GeneratorWPF.Models.LocalModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GeneratorWPF.Context
{
    public class LocalContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite("Data Source=LocalProjectDatabase.db"); // migration
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalProjectDatabase.db");
            optionsBuilder.UseSqlite("Data Source=C:\\Users\\Express\\Desktop\\GeneratorWPF\\GeneratorWPF\\LocalProjectDatabase.db");
        }

        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
