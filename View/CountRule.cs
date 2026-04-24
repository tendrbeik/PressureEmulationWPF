using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PressureEmulationWPF.View
{
    internal class CountRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int resultValue;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"Количество строк для вывода не может быть пустой строкой!");

                resultValue = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода количества строк для вывода - \"10\".");
            }
            // Я не уверен, что на практике может понадобиться порт с номером ноль, поэтому я добавлю его в ошибку валидации.
            if (resultValue <= 0)
                return new ValidationResult(false, $"Количество строк для вывода не может быть задано отрицательным числом или нулём!");
            if (resultValue > ushort.MaxValue)
                return new ValidationResult(false, $"Количество выводимых строк не может быть больше 65 535");
            return ValidationResult.ValidResult;
        }
    }
}
