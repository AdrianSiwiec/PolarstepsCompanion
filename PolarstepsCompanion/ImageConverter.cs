using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PolarstepsCompanion
{
    public class ImageConverter : IValueConverter
    {
        //private readonly int IMAGE_HEIGHT =  Application.Resources["PreviewImageHeight"];

        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(value.ToString());
            bitmap.DecodePixelHeight = System.Convert.ToInt32(Application.Current.Resources["PreviewImageHeight"]);
            bitmap.EndInit();

            return bitmap;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();

        }
    }
}
