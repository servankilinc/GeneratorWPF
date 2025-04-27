using GeneratorWPF.Dtos._Entity;
using GeneratorWPF.Dtos._Field;
using GeneratorWPF.ViewModel;
using GeneratorWPF.ViewModel._Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GeneratorWPF.View._Entity.Partials
{
    /// <summary>
    /// Interaction logic for EntityCreateDialog.xaml
    /// </summary>
    public partial class EntityCreateDialog : Window
    {
        public EntityCreateDialog()
        {
            InitializeComponent();

            var viewModel = new EntityCreateVM();
            viewModel.CloseDialogAction = () => this.Close(); // Dialog'u kapatma işlemi
            this.DataContext = viewModel;
        }
    }
}
