using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace pcclient
{
    public class IndentConverter : IValueConverter
    {
        public double Indent { get; set; }
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    var item = value as TreeViewItem;
        //    if (null == item)
        //        return new Thickness(0);
        //    double ind = Indent * item.GetDepth();
        //    App.Logger.Debug(string.Format("indent {0}", ind));

        //    return new Thickness(ind, 0, 0, 0);
        //}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = -1;
            if (value is DependencyObject)
            {
                var parent = VisualTreeHelper.GetParent(value as DependencyObject);
                while (!(parent is TreeView) && (parent != null))
                {
                    if (parent is TreeViewItem)
                        level++;
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
            return level;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
