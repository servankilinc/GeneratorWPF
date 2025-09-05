﻿using GeneratorWPF.Extensions;
using GeneratorWPF.Models.Signature;
using GeneratorWPF.Repository;

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


        public IdentityModelTypes GetIdentityModelTypeNames(EntityRepository entityRepository, FieldRepository fieldRepository)
        {
            Entity? roleEntity = null;
            Entity? userEntity = null;
            string IdentityKeyType = "int";
            if (this.RoleEntityId != null)
            {
                roleEntity = entityRepository.Get(f => f.Id == this.RoleEntityId);

                var uniqueFields = fieldRepository.GetAll(f => f.EntityId == this.RoleEntityId && f.IsUnique);
                if (uniqueFields != null)
                {
                    IdentityKeyType = uniqueFields.First().GetMapedTypeName();
                }
            }
            if (this.UserEntityId != null)
            {
                userEntity = entityRepository.Get(f => f.Id == this.UserEntityId);

                var uniqueFields = fieldRepository.GetAll(f => f.EntityId == this.UserEntityId && f.IsUnique);
                if (uniqueFields != null)
                {
                    IdentityKeyType = uniqueFields.First().GetMapedTypeName();
                }
            }
            string IdentityUserType = $"IdentityUser<{IdentityKeyType}>";
            string IdentityRoleType = $"IdentityRole<{IdentityKeyType}>";
            if (userEntity != null) IdentityUserType = userEntity.Name;
            if (roleEntity != null) IdentityRoleType = roleEntity.Name;

            return new IdentityModelTypes
            {
                IdentityKeyType = IdentityKeyType,
                IdentityUserType = IdentityUserType,
                IdentityRoleType = IdentityRoleType,    
            };
        }
    }
    public class IdentityModelTypes
    {
        public string IdentityUserType { get; set; } = string.Empty;
        public string IdentityRoleType { get; set; } = string.Empty;
        public string IdentityKeyType { get; set; } = string.Empty;
    }
}