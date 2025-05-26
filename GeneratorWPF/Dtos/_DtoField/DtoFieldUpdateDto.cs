using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using GeneratorWPF.Utils;
using System.Collections.ObjectModel;

namespace GeneratorWPF.Dtos._DtoField;

public class DtoFieldUpdateDto : ObversableObject
{
    private readonly FieldRepository _fieldRepository;

    public DtoFieldUpdateDto(FieldRepository fieldRepository)
    {
        this._fieldRepository = fieldRepository;
    }

    public int Id { get; set; }
    public bool IsRequired { get; set; }
    public bool IsList { get; set; }


    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    private int _sourceFieldId;
    public int SourceFieldId
    {
        get => _sourceFieldId;
        set
        {
            if (_sourceFieldId != value)
            {
                _sourceFieldId = value;
                OnPropertyChanged(nameof(SourceFieldId));

                if (this._fieldRepository != null && string.IsNullOrEmpty(this.Name))
                {
                    this.Name = this._fieldRepository.GetFieldName(value);
                }
            }
        }
    }

    private int _sourceEntityId;
    public int SourceEntityId
    {
        get => _sourceEntityId;
        set
        {
            if (_sourceEntityId != value)
            {
                _sourceEntityId = value;
                OnPropertyChanged(nameof(SourceEntityId));

                if (_fieldRepository != null)
                {
                    FieldList = new ObservableCollection<Field>(
                        _fieldRepository.GetAll(filter: f => f.EntityId == value)
                    );
                }
            }
        }
    }


    private ObservableCollection<Field>? _fieldList;
    public ObservableCollection<Field>? FieldList
    {
        get => _fieldList;
        set
        {
            if (_fieldList != value)
            {
                _fieldList = value;
                OnPropertyChanged(nameof(FieldList));
            }
        }
    }
}
