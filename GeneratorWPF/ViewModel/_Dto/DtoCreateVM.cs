using GeneratorWPF.Dtos._Dto;
using GeneratorWPF.Dtos._DtoField;
using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using GeneratorWPF.Services;
using GeneratorWPF.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GeneratorWPF.ViewModel._Dto.Partial
{
    public class DtoCreateVM : BaseViewModel
    {
        private DtoRepository _dtoRepository { get; set; }
        private FieldRepository _fieldRepository { get; set; }
        private readonly INavigationService _navigation;
        public ICommand SaveCommand { get; set; }
        public ICommand AddFieldCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public Action? CloseDialogAction { get; set; }
        private ObservableCollection<Entity> _entityList;
        public ObservableCollection<Entity> EntityList { get => _entityList; set { _entityList = value; OnPropertyChanged(nameof(EntityList)); } }
 
        public DtoCreateDto DtoToCreate { get; set; } = new DtoCreateDto();
        private ObservableCollection<DtoFieldCreateDto> _dtoFields = new ObservableCollection<DtoFieldCreateDto>();
        public ObservableCollection<DtoFieldCreateDto> DtoFields { get => _dtoFields; set { _dtoFields = value; OnPropertyChanged(nameof(DtoFields)); } }

        public DtoCreateVM(INavigationService navigation)
        {
            _navigation = navigation;
            _fieldRepository = new FieldRepository();
            _dtoRepository = new DtoRepository();
            var entityRepository = new EntityRepository();

            EntityList = new ObservableCollection<Entity>(entityRepository.GetAll());


            SaveCommand = new RellayCommand(obj => 
            {
                try
                {
                    if (string.IsNullOrEmpty(DtoToCreate.Name) || DtoToCreate.RelatedEntityId == default || DtoFields.Count == 0)
                    {
                        MessageBox.Show("Check The Fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    DtoToCreate.DtoFields = DtoFields;
                    _dtoRepository.CreateByFields(DtoToCreate);
                     

                    MessageBox.Show("Dto Created Successfully", "Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                    CloseDialogAction?.Invoke(); // Dialog'u kapat

                    _navigation.NavigateTo<DtoHomeVM>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });


            AddFieldCommand = new RellayCommand(obj => 
            {
                if (DtoToCreate.RelatedEntityId == default)
                {
                    MessageBox.Show("Please Select an Entity!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DtoFields.Add(new DtoFieldCreateDto(_fieldRepository)
                {
                    Name = "New Field",
                    SourceFieldId = DtoToCreate.RelatedEntityId,
                });
            });


            RemoveCommand = new RellayCommand(field =>
            {
                if (field != null && DtoFields.Contains(field))
                {
                    DtoFields.Remove((DtoFieldCreateDto)field);
                }
            });


            CancelCommand = new RellayCommand(field =>
            {
                CloseDialogAction?.Invoke();
            });
        }
    }
}
