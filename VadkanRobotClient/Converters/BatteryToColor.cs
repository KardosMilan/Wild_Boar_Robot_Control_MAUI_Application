using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VadkanRobotClient.Converters
{
    internal class BatteryToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Colors.Green;

            double battery = System.Convert.ToDouble(value);

            if (battery > 70)
                return Colors.LimeGreen;      // zöld
            else if (battery > 30)
                return Colors.Orange;         // sárga
            else
                return Colors.Red;            // piros
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
