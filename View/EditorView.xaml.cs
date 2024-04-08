using DicomEditor.Interfaces;
using DicomEditor.Model.EditorModel.Tree;
using DicomEditor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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

        private void Tree_ModelChanged(object sender, RoutedEventArgs e)
        {
            Stack<TreeNode> stack = new();
            foreach (var node in Traverse(Tree.Nodes))
            {
                stack.Push(node);
            }

            foreach (var node in stack)
            {
                node.IsExpanded = false;
            }

            foreach (var node in stack)
            {
                IDatasetModel attribute = (IDatasetModel)node.Tag;
                if (attribute.IsSearchResult)
                {
                    ExpandNodes(node);
                }
            }
        }

        private IEnumerable<TreeNode> Traverse(IEnumerable<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;

                if (node.IsExpandable)
                {
                    node.IsExpanded = true;
                    foreach (var child in Traverse(node.Nodes))
                    {
                        yield return child;
                    }
                }
            }
        }

        private void ExpandNodes(TreeNode node)
        {
            node = node.Parent;
            while (node.Level > -1)
            {
                node.IsExpanded = true;
                node = node.Parent;
            }
        }
    }
}
