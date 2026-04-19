using System.Globalization;
using System.Windows.Controls;

namespace PressureEmulationWPF.View.ValidationRules
{
    class DoubleValueRule:ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double resultValue = 0;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"Число не может быть пустой строкой!");
                if (((string)value).Length > 0)
                    resultValue = double.Parse((string)value, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода числа - \"90.30\".");
            }

            return ValidationResult.ValidResult;
        }
    }
}
