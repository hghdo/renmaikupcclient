using System.Windows.Media.Imaging;
using System;

public sealed class ImageConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType,
                          object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            return new BitmapImage(new Uri((string)value));
        }
        catch
        {
            return new BitmapImage();
        }
    }

    public object ConvertBack(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}