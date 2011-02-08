using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;

namespace pcclient
{
    public class IndentConverter : IValueConverter
    {
        public double Indent { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (null == item)
                return new Thickness(0);
            double ind = Indent * item.GetDepth();
            App.Logger.Debug(string.Format("indent {0}", ind));

            return new Thickness(ind, 0, 0, 0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
