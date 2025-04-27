using GeneratorWPF.Utils;
using System.Windows.Input;
using GeneratorWPF.Repository;
using System.Windows;
using GeneratorWPF.Dtos._Entity;
using GeneratorWPF.Services;

namespace GeneratorWPF.ViewModel._Entity;

public class EntityUpdateVM : BaseViewModel
{
    private INavigationService _navigation;
    private EntityRepository _entityRepository { get; set; } 
    public ICommand SaveCommand { get; set; }  
    public ICommand CancelCommand { get; set; } 

    private EntityUpdateDto _entityUpdateModel;
    public EntityUpdateDto EntityUpdateModel { get => _entityUpdateModel; set { _entityUpdateModel = value; OnPropertyChanged(nameof(EntityUpdateModel)); } }
    public Action? CloseDialogAction { get; set; } // Dialog'u kapatmak için callback

    public EntityUpdateVM(INavigationService navigation)
    {
        _navigation = navigation;
        _entityRepository = new EntityRepository();


        var entity = _entityRepository.Get(f => f.Id == StateStatics.EntityUpdateId);
        EntityUpdateModel = new EntityUpdateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            TableName = entity.TableName,
        };

        SaveCommand = new RellayCommand(obj =>
        {
            try
            {
                if (string.IsNullOrEmpty(EntityUpdateModel.Name) || string.IsNullOrEmpty(EntityUpdateModel.TableName))
                {
                    MessageBox.Show("Check The Fields!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                } 

                _entityRepository.Update(EntityUpdateModel);

                MessageBox.Show("Entity Updated Successfully", "Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseDialogAction?.Invoke(); // Dialog'u kapat
                _navigation.NavigateTo<EntityHomeVM>();
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
