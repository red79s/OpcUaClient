using System;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace OpcUaClient
{
    public class BoolToColorConverter : MarkupExtension, IValueConverter
    {
        private static BoolToColorConverter Converter { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Converter == null)
            {
                Converter = new BoolToColorConverter();
            }
            return Converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)            
            {
                var val = (bool)value;
                return val ? Brushes.LightSkyBlue : Brushes.LightSlateGray;
            }
            return Brushes.LightSlateGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
