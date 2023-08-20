using DicomEditor.Interfaces;
using DicomEditor.Model.EditorModel.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DicomEditor.Converters
{
    public class DatasetModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IDatasetModel attribute;
            if (value is not null)
            {
                TreeNode node = (TreeNode)value;
                attribute = (IDatasetModel)node.Tag;
            }
            else
            {
                attribute = null;
            }
            return attribute;
        }
    }
}
