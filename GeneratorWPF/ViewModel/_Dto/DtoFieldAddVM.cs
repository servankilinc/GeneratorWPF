using GeneratorWPF.Utils;
using System.Windows.Input;
using GeneratorWPF.Repository;
using System.Windows;
using GeneratorWPF.Services;
using GeneratorWPF.Dtos._DtoField;
using GeneratorWPF.Models;
using System.Collections.ObjectModel;

namespace GeneratorWPF.ViewModel._Dto;

public class DtoFieldAddVM : BaseViewModel
{
    private INavigationService _navigation;
    private FieldRepository _fieldRepository { get; set; }
    private DtoFieldRepository _dtoFieldRepository { get; set; }
    private EntityRepository _entityRepository { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    public ObservableCollection<Entity> EntityList { get; set; }

    private DtoFieldCreateDto _dtoFieldCreateDto;
    public DtoFieldCreateDto DtoFieldCreateDto { get => _dtoFieldCreateDto; set { _dtoFieldCreateDto = value; OnPropertyChanged(nameof(DtoFieldCreateDto)); }}
    public Action? CloseDialogAction { get; set; } // Dialog'u kapatmak için callback

    public DtoFieldAddVM(INavigationService navigation)
    {
        _navigation = navigation;
        _fieldRepository = new FieldRepository();
        _dtoFieldRepository = new DtoFieldRepository();
        _entityRepository = new EntityRepository();

        EntityList = new ObservableCollection<Entity>(_entityRepository.GetAll());

        DtoFieldCreateDto = new DtoFieldCreateDto(_fieldRepository) 
        { 
            DtoId = StateStatics.DtoDetailAddDtoFieldDtoId,
            SourceEntityId = StateStatics.DtoDetailRelatedEntityId,
            IsRequired = true,
            IsList = false
        };

        SaveCommand = new RellayCommand(obj =>
        {
            try
            {
                if (string.IsNullOrEmpty(DtoFieldCreateDto.Name) || DtoFieldCreateDto.SourceFieldId == default)
                {
                    MessageBox.Show("Check The Fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                _dtoFieldRepository.Add(DtoFieldCreateDto);

                MessageBox.Show("Field Added Successfully", "Successful", MessageBoxButton.OK, MessageBoxImage.Information);

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
            CloseDialogAction?.Invoke(); // Dialog'u kapat
        });
    }
}
