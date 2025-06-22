using GeneratorWPF.Models.Enums;
using GeneratorWPF.Models.Signature;

namespace GeneratorWPF.Models
{
    public class Relation : EntityBase
    {
        public int Id { get; set; }
        public int PrimaryFieldId { get; set; }
        public int ForeignFieldId { get; set; }
        public int RelationTypeId { get; set; }
        public int DeleteBehaviorTypeId { get; set; }
        public string PrimaryEntityVirPropName { get; set; } = null!;
        public string ForeignEntityVirPropName { get; set; } = null!;

        public virtual Field PrimaryField { get; set; } = null!;
        public virtual Field ForeignField { get; set; } = null!;
        public virtual RelationType RelationType { get; set; } = null!;
        public virtual DeleteBehaviorType DeleteBehaviorType { get; set; } = null!;


        public string GetOnDeleteType()
        {
            return this.DeleteBehaviorTypeId switch
            {
                (int)DeleteBehaviorTypeEnums.Cascade => "DeleteBehavior.Cascade",
                (int)DeleteBehaviorTypeEnums.ClientCascade => "DeleteBehavior.ClientCascade",
                (int)DeleteBehaviorTypeEnums.Restrict => "DeleteBehavior.Restrict",
                (int)DeleteBehaviorTypeEnums.ClientSetNull => "DeleteBehavior.ClientSetNull",
                (int)DeleteBehaviorTypeEnums.ClientNoAction => "DeleteBehavior.ClientNoAction",
                (int)DeleteBehaviorTypeEnums.SetNull => "DeleteBehavior.SetNull",
                (int)DeleteBehaviorTypeEnums.NoAction => "DeleteBehavior.NoAction",
                _ => "DeleteBehavior.Restrict"
            };
        }
    }
}
