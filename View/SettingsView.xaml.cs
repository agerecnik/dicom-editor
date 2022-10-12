using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Media;
using static DicomEditor.Interfaces.IDICOMServer;

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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if(tb.Text is nameof(VerificationStatus.Successful))
            {
                tb.Foreground = Brushes.LightSeaGreen;
            }
            else if(tb.Text is nameof(VerificationStatus.Failed))
            {
                tb.Foreground = Brushes.DarkRed;
            }
            else if(tb.Text is nameof(VerificationStatus.InProgress))
            {
                tb.Foreground = Brushes.LightGoldenrodYellow;
            }
        }
    }
}
