using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for StoreDialog.xaml
    /// </summary>
    public partial class StoreDialog : UserControl
    {
        public StoreDialog()
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
                StoreProgress.Visibility = Visibility.Collapsed;
                Status.Visibility = Visibility.Visible;
            }
        }
    }
}
