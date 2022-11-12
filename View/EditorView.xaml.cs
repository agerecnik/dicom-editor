using DicomEditor.Interfaces;
using DicomEditor.Model.EditorModel.Tree;
using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text.RegularExpressions;
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

        private void tree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (EditorViewModel)DataContext;
            TreeNode node = (TreeNode)tree.SelectedItem;
            if(node is not null)
            {
                vm.SelectedAttribute = (IDatasetModel)node.Tag;
            }
        }
    }
}
