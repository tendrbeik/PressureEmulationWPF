using System.Globalization;
using System.Windows.Controls;

namespace PressureEmulationWPF
{
    class EmulationNameRule : ValidationRule
    {
        //TODO: тут надо подумать можно ли вообще в строку ввести какие-то недопустимые символы и потом писать проверку. Пока оставлю только проверку на длину имени.
        //Конечно в ТЗ нет требования на длину имени эмуляции, но я всё же добавлю ограничение.
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {

            if (((string)value).Length == 0)
                return new ValidationResult(false, $"Имя эмуляции не должно быть пустым! Пожалуйста введите имя длиной от 1 до 50 символов.");

            if (((string)value).Length > 50)
                return new ValidationResult(false, $"Имя эмуляции не должно быть длиннее 50 символов! Пожалуйста введите имя длиной от 1 до 50 символов.");

            return ValidationResult.ValidResult;
        }
    }
}
