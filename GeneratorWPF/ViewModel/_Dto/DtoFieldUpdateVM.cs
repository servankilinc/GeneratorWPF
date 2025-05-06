using GeneratorWPF.Utils;
using System.Windows.Input;
using GeneratorWPF.Repository;
using System.Windows;
using GeneratorWPF.Services;
using GeneratorWPF.Dtos._DtoField;
using GeneratorWPF.Models;

namespace GeneratorWPF.ViewModel._Dto;

public class DtoFieldUpdateVM : BaseViewModel
{
    private INavigationService _navigation;
    private FieldRepository _fieldRepository { get; set; }
    private DtoFieldRepository _dtoFieldRepository { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    private List<Field> _fields = new List<Field>();
    public List<Field> Fields { get => _fields; set { _fields = value; OnPropertyChanged(nameof(Fields)); } }

    private DtoFieldUpdateDto _dtoFieldUpdateDto;
    public DtoFieldUpdateDto DtoFieldUpdateDto { get => _dtoFieldUpdateDto; set { _dtoFieldUpdateDto = value; OnPropertyChanged(nameof(DtoFieldUpdateDto)); } }
    public Action? CloseDialogAction { get; set; } // Dialog'u kapatmak için callback

    public DtoFieldUpdateVM(INavigationService navigation)
    {
        _navigation = navigation;
        _fieldRepository = new FieldRepository();
        _dtoFieldRepository = new DtoFieldRepository();
        Fields = _fieldRepository.GetAll(filter: f => f.EntityId == StateStatics.DtoDetailRelatedEntityId, enableTracking: false);

        var dtoField = _dtoFieldRepository.Get(f => f.Id == StateStatics.DtoDetailUpdateDtoFieldId);
        DtoFieldUpdateDto = new DtoFieldUpdateDto
        {
            Id = dtoField.Id,
            Name = dtoField.Name,
            SourceFieldId = dtoField.SourceFieldId,
            IsRequired = dtoField.IsRequired,
            IsList = dtoField.IsList
        };

        SaveCommand = new RellayCommand(obj =>
        {
            try
            {
                if (string.IsNullOrEmpty(DtoFieldUpdateDto.Name) || DtoFieldUpdateDto.SourceFieldId == default)
                {
                    MessageBox.Show("Check The Fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _dtoFieldRepository.Update(DtoFieldUpdateDto);

                MessageBox.Show("Field Updated Successfully", "Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseDialogAction?.Invoke();
                _navigation.NavigateTo<DtoDetailVM>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        CancelCommand = new RellayCommand(obj =>
        {
            CloseDialogAction?.Invoke();
        });
    }
}
