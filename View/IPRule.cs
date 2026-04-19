using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PressureEmulationWPF.View
{
    internal class IPRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            IPAddress resultValue;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"IP не может быть пустой строкой!");

                resultValue = IPAddress.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода IP адреса - \"127.0.0.1\".");
            }
            return ValidationResult.ValidResult;
        }
    }
}
