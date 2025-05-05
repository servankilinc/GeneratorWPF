using GeneratorWPF.Utils;
using System.Windows.Input;
using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using GeneratorWPF.View._Dto.Partials;
using GeneratorWPF.Services;
using GeneratorWPF.Dtos._DtoField;
using System.Windows;
using GeneratorWPF.ViewModel._Entity;

namespace GeneratorWPF.ViewModel._Dto
{
    public class DtoHomeVM : BaseViewModel
    {
        private DtoRepository _dtoRepository { get; set; }
        private INavigationService _navigation { get; set; }
        public ICommand ShowCreateCommand { get; set; }
        public ICommand ShowDetailCommand { get; set; }
        public ICommand ShowUpdateCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public List<Entity> EntityList { get; set; }

        private List<Dto> _DtoList = new List<Dto>();
        public List<Dto> DtoList { get { return _DtoList; } set { _DtoList = value; OnPropertyChanged(nameof(DtoList)); } }

        private int _entiyIdForFilter = default;
        public int EntiyIdForFilter { 
            get { return _entiyIdForFilter; } 
            set { 
                _entiyIdForFilter = value; 
                OnPropertyChanged(nameof(EntiyIdForFilter));

                DtoList = _dtoRepository.GetList(f => f.RelatedEntityId == value);
            }
        } 

        public DtoHomeVM(INavigationService navigation)
        {
            _navigation = navigation;
            var entityRepository = new EntityRepository();
            _dtoRepository = new DtoRepository();

            EntityList = entityRepository.GetAll(enableTracking: false);
            DtoList = _dtoRepository.GetList(f => true);

            ShowCreateCommand = new RellayCommand(RellayCommand =>
            {
                var dialog = new DtoCreateDialog(_navigation);
                if (dialog.ShowDialog() == false)
                {
                    return;
                }
                else
                {
                    dialog.Show();
                }
            });

            ShowDetailCommand = new RellayCommand(dtoId =>
            {
                StateStatics.DtoDetailId = (int)dtoId;
                _navigation.NavigateTo<DtoDetailVM>();
            });

            ShowUpdateCommand = new RellayCommand(dtoId =>
            {
                StateStatics.DtoUpdateId = (int)dtoId;
                var dialog = new DtoUpdateDialog(_navigation);
                if (dialog.ShowDialog() == false)
                {
                    return;
                }
                else
                {
                    dialog.Show();
                }
            });

            RemoveCommand = new RellayCommand(dtoId =>
            {
                try
                {
                    _dtoRepository.Delete((int)dtoId);
                    MessageBox.Show("Dto removed successfully ", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DtoList = _dtoRepository.GetList(f => EntiyIdForFilter != default ? f.Id == EntiyIdForFilter : true);
                }
                catch (Exception)
                {
                    MessageBox.Show("Dto could'nt remove", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            });
        }
    }
}
