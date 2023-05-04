using Prism_Drive.Models;
using System.Globalization;

namespace Prism_Drive.BindingConverters
{
    class ItemStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UploadItem uploadItem)
            {
                switch (uploadItem.Status)
                {
                    case "Pending":
                        return Colors.Green;
                    case "Uploading":
                        return Colors.Blue;
                    case "Uploaded":
                        return Colors.Gray;
                    case "Failed":
                        return Colors.Red;
                    default:
                        return Colors.Red;
                }
            }
            else
            {
                return Colors.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
