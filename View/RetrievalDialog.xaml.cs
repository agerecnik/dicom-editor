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
    /// Interaction logic for RetrievalDialog.xaml
    /// </summary>
    public partial class RetrievalDialog : UserControl
    {
        public RetrievalDialog()
        {
            InitializeComponent();
        }

        private void retrievalProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(retrievalProgress.Value == 100)
            {
                Window.GetWindow(this).Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
