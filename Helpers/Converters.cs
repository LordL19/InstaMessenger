using InstantMessenger.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InstantMessenger.Helpers
{
    public class MessageBubbleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Message message)
            {
                return message.IsFromMe
                    ? new SolidColorBrush(Color.FromRgb(0, 122, 255)) // Azul para mis mensajes
                    : new SolidColorBrush(Color.FromRgb(199, 199, 199)); // Gris para mensajes recibidos
            }
            else if (value is string text && text.StartsWith("Tú:"))
            {
                return new SolidColorBrush(Color.FromRgb(0, 122, 255)); // Azul
            }

            return new SolidColorBrush(Color.FromRgb(199, 199, 199)); // Gris claro
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Message message)
            {
                return message.IsFromMe
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left;
            }
            else if (value is string text)
            {
                return text.StartsWith("Tú:")
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left;
            }

            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter != null && parameter.ToString() == "Invert")
                {
                    boolValue = !boolValue;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                if (parameter != null && parameter.ToString() == "Invert")
                {
                    result = !result;
                }

                return result;
            }

            return false;
        }
    }
}