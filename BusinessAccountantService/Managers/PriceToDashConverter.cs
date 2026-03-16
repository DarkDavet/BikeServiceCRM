using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BusinessAccountantService.Managers
{
    public class PriceToDashConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price && price > 0)
            {
                // Если цена больше нуля, выводим с копейками и валютой
                return $"{price:N2} р.";
            }

            // Если 0 или меньше — ставим прочерк
            return "—";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
