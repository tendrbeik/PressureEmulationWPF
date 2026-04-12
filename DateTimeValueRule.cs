using System.Globalization;
using System.Windows.Controls;

namespace PressureEmulationWPF
{
    class DateTimeValueRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            DateTime resultValue;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"Дата не может быть пустой строкой!");
                if (((string)value).Length > 0)
                    resultValue = DateTime.Parse((string)value, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода - \"4/15/2026 7:53:52 PM\".");
            }
            return ValidationResult.ValidResult;
        }
    }
}
