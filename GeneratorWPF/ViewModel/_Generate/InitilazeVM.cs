using GeneratorWPF.Utils;
using System.Windows.Input;

namespace GeneratorWPF.ViewModel._Generate
{
    public class InitilazeVM : BaseViewModel
    {
        public ICommand CancelCommand { get; set; }
        public ICommand GenerateCommand { get; set; }
        public Action? CloseDialogAction { get; set; }

        public InitilazeVM(GenerateVM generateVM, Action closeDialogAction)
        {
            CloseDialogAction = closeDialogAction;

            GenerateCommand = new RellayCommand(obj =>
            {
                // TODO: Implement the logic for generating the project
            });

            CancelCommand = new RellayCommand(obj =>
            {
                CloseDialogAction?.Invoke();
            });
        }
    }
}
