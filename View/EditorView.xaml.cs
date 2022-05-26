using DicomEditor.Model.EditorModel;
using DicomEditor.ViewModel;
using FellowOakDicom;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
		}

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in _tree.SelectedNodes)
                if (node.IsExpandable)
                    node.IsExpanded = !node.IsExpanded;
        }
    }
}
