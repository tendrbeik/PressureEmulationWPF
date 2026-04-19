using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PressureEmulationWPF.View
{
    internal class InputRegisterAddressRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int resultValue;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"Адрес Input регистра не может быть пустой строкой!");

                resultValue = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода порта - \"0\".");
            }
            //TODO: Надо разобраться в том, какие пределы есть у адресов регистров и прописать эти пределы тут.
            // Я не уверен, что на практике может понадобиться порт с номером ноль, поэтому я добавлю его в ошибку валидации.
            if (resultValue < 0)
                return new ValidationResult(false, $"Адрес Input регистра не может быть задан отрицательным числом!");
            return ValidationResult.ValidResult;
        }
    }
}
