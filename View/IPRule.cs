using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода IP адреса - \"127.0.0.1\".");
            }
            //Проверим IP адрес с помощью регулярного выражения
            const string pattern = @"^(?:(?:25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\.){3}(?:25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";
            if (!Regex.IsMatch((string)value, pattern))
                return new ValidationResult(false, $"IPv4 адрес задан неверно! Используйте для ввода шаблон \"127.0.0.1\".");
            return ValidationResult.ValidResult;
        }
    }
}
