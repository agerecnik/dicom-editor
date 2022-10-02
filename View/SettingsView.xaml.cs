using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>().CurrentView;
        }
    }
}
