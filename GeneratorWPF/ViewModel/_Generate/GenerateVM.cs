using GeneratorWPF.Utils;

namespace GeneratorWPF.ViewModel._Generate
{
    public class GenerateVM: BaseViewModel
    {
        private BaseViewModel _currentSection;
        public BaseViewModel CurrentSection
        {
            get { return _currentSection; }
            set
            {
                _currentSection = value;
                OnPropertyChanged(nameof(CurrentSection));
            }
        }

        public GenerateVM()
        {
            CurrentSection = new AppSettingsVM(this);
        }
    }
}
