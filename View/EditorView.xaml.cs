using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>().CurrentView;
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in tree.SelectedNodes)
                if (node.IsExpandable)
                    node.IsExpanded = !node.IsExpanded;
        }
    }
}
