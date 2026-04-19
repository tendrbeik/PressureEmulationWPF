using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PressureEmulationWPF
{
    internal class PortRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var resultValue;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"Порт не может быть пустой строкой!");
                if (((string)value).Length > 0)
                    resultValue = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода порта - \"502\".");
            }
            // Я не уверен, что на практике может понадобиться порт с номером ноль, поэтому я добавлю его в ошибку валидации.
            if(resultValue <= 0)
                return new ValidationResult(false, $"Порт не может быть задан отрицательным числом или нулём!");
            return ValidationResult.ValidResult;
        }
    }
}
