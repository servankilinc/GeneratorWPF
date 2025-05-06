﻿namespace GeneratorWPF.Dtos._Entity
{
    public class EntityUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string TableName { get; set; } = null!;
        
        public bool SoftDeletable { get; set; }
        public bool Auditable { get; set; }
        public bool Loggable { get; set; }
        public bool Archivable { get; set; }

        public int? CreateDtoId { get; set; }
        public int? UpdateDtoId { get; set; }
        public int? BasicResponseDtoId { get; set; }
        public int? DetailResponseDtoId { get; set; }
    }
}
