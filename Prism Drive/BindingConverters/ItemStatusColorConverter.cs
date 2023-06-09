﻿using System.Globalization;

namespace Prism_Drive.BindingConverters
{
    class ItemStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Pending" => Colors.Green,
                    "Uploading" => Colors.Blue,
                    "Uploaded" => Colors.Gray,
                    "Failed" => Colors.Red,
                    _ => Colors.Red,
                };
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
