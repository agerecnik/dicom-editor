using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace DicomEditor.View
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    public partial class ImportView : UserControl
    {
        public ImportView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>().CurrentView;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ImportViewModel importVM = (ImportViewModel)DataContext;
            importVM.ParseSelectedEntry(QueryResultTreeView.SelectedItem);
        }
    }
}
