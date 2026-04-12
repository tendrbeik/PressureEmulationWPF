using System.Globalization;
using System.Windows.Controls;

namespace PressureEmulationWPF
{
    class PositiveDoubleValueRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double resultValue = 0;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"Положительное число не может быть пустой строкой!");
                if (((string)value).Length > 0)
                    resultValue = double.Parse((string)value, CultureInfo.InvariantCulture);
            }
            catch(Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода числа - \"90.30\".");
            }

            if(resultValue < 0)
            {
                return new ValidationResult(false, $"Пожалуйста введите положительное число. Используйте формат ввода числа - \"90.30\".");
            }
            return ValidationResult.ValidResult;
        }
    }
}
