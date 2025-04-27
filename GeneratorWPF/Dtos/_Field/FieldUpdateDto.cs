﻿namespace GeneratorWPF.Dtos._Field
{
    public class FieldUpdateDto
    {
        public int Id { get; set; } 
        public int FieldTypeId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsList { get; set; }
    }
}
