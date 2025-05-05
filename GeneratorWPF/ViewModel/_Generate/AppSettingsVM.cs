using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using GeneratorWPF.Utils;
using System.Windows;
using System.Windows.Input;

namespace GeneratorWPF.ViewModel._Generate;

public class AppSettingsVM : BaseViewModel
{
    private AppSettingsRepository _appSettingsRepository;
    public ICommand SaveAppSettings { get; set; }
    public ICommand CancelCommand { get; set; }
    private AppSetting _appSetting;
    public AppSetting AppSetting { get => _appSetting; set { _appSetting = value; OnPropertyChanged(nameof(AppSetting)); } }
    public Action CloseDialogAction { get; set; }

    public AppSettingsVM(GenerateVM generateVM)
    {
        _appSettingsRepository = new AppSettingsRepository();

        var existAppSetting = _appSettingsRepository.GetAll().LastOrDefault();
        AppSetting = existAppSetting != null ? existAppSetting : AppSetting = new AppSetting();

        SaveAppSettings = new RellayCommand(obj =>
        {
            if (AppSetting != null)
            {
                if (string.IsNullOrEmpty(AppSetting.ProjectName) || string.IsNullOrEmpty(AppSetting.SolutionName) || string.IsNullOrEmpty(AppSetting.Path))
                {
                    MessageBox.Show("Please fill all fields");
                    return;
                }
                _appSettingsRepository.Add(AppSetting);

                generateVM.CurrentSection = new InitilazeVM(generateVM, CloseDialogAction!);
            }
        });

        CancelCommand = new RellayCommand(obj =>
        {
            CloseDialogAction?.Invoke();
        });
    }
}
