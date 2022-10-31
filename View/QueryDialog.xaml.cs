using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for QueryDialog.xaml
    /// </summary>
    public partial class QueryDialog : UserControl
    {
        public QueryDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void ExecutionFinished_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (ExecutionFinished.Text == "True")
            {
                Window.GetWindow(this).Close();
            }
        }
    }
}
