using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DicomEditor.Theme
{
    public static class ThemeProperties
    {
        public static string GetSearchFieldName(DependencyObject obj)
        {
            return (string)obj.GetValue(SearchFieldNameProperty);
        }

        public static void SetSearchFieldName(DependencyObject obj, string value)
        {
            obj.SetValue(SearchFieldNameProperty, value);
        }

        public static readonly DependencyProperty SearchFieldNameProperty =
            DependencyProperty.RegisterAttached(
                "SearchFieldName",
                typeof(string),
                typeof(ThemeProperties),
                new FrameworkPropertyMetadata());
    }
}
