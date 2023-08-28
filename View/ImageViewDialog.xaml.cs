using System.Windows;
using System.Windows.Controls;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for ImageViewDialog.xaml
    /// </summary>
    public partial class ImageViewDialog : UserControl
    {
        public ImageViewDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
