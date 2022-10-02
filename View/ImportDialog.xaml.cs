using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for RetrievalDialog.xaml
    /// </summary>
    public partial class ImportDialog : UserControl
    {
        public ImportDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void Status_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (Status.Text is "Completed")
            {
                Window.GetWindow(this).Close();
            }
            else if (Status.Text is not null and not "")
            {
                Progress.Visibility = Visibility.Collapsed;
                Status.Visibility = Visibility.Visible;
            }
        }
    }
}
