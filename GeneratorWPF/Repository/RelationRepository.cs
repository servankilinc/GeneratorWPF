using GeneratorWPF.Context;
using GeneratorWPF.Dtos._Relation;
using GeneratorWPF.Models;

namespace GeneratorWPF.Repository
{

    public class RelationRepository : EFRepositoryBase<Relation>
    {
        public void AddRelation(RelationCreateDto createDto)
        {
            using var _context = new LocalContext();
            _context.Relations.Add(new Relation
            {
                PrimaryFieldId = createDto.PrimaryFieldId,
                ForeignFieldId = createDto.ForeignFieldId,
                RelationTypeId = createDto.RelationTypeId
            });
            _context.SaveChanges();
        }

        public List<RelationType> GetRelationTypes()
        {
            using var _context = new LocalContext();
            return _context.RelationTypes.ToList();
        }

        //public List<RelationResponseDto> GetAllByFields(int fieldId) 
        //{
        //    using var _context = new LocalContext();

        //    var res = from relation in _localContext.Relations.Where(f => f.PrimaryFieldId == fieldId || f.ForeignFieldId == fieldId)
        //              join pf in _localContext.Fields on relation.PrimaryFieldId equals pf.Id
        //              join ft in _localContext.FieldTypes on pf.FieldTypeId equals ft.Id
        //              join ff in _localContext.Fields on relation.ForeignFieldId equals ff.Id
        //              join ft2 in _localContext.FieldTypes on ff.FieldTypeId equals ft2.Id
        //              select new RelationResponseDto
        //              {
        //                  Id = relation.Id,
        //                  RelationTypeId = relation.RelationTypeId,
        //                  RelationType = new RelationTypeResponseDto
        //                  {
        //                      Id = relation.RelationType.Id,
        //                      Name = relation.RelationType.Name
        //                  },
        //                  PrimaryFieldId = relation.PrimaryFieldId,
        //                  PrimaryField = new FieldBasicResponseDto
        //                  {
        //                      Id = relation.PrimaryField.Id,
        //                      EntityId = relation.PrimaryField.EntityId,
        //                      Name = relation.PrimaryField.Name,
        //                      IsUnique = relation.PrimaryField.IsUnique,
        //                      FieldTypeId = relation.PrimaryField.FieldType.Id,
        //                      FieldType = new FieldTypeResponseDto
        //                      {
        //                          Id = relation.PrimaryField.FieldType.Id,
        //                          Name = relation.PrimaryField.FieldType.Name
        //                      }
        //                  },
        //                  ForeignFieldId = relation.ForeignFieldId,
        //                  ForeignField = new FieldBasicResponseDto
        //                  {
        //                      Id = relation.ForeignField.Id,
        //                      EntityId = relation.ForeignField.EntityId,
        //                      Name = relation.ForeignField.Name,
        //                      IsUnique = relation.ForeignField.IsUnique,
        //                      FieldTypeId = relation.ForeignField.FieldType.Id,
        //                      FieldType = new FieldTypeResponseDto
        //                      {
        //                          Id = relation.ForeignField.FieldType.Id,
        //                          Name = relation.ForeignField.FieldType.Name
        //                      }
        //                  }
        //              };

        //    return res.ToList();
        //}
    }
}
