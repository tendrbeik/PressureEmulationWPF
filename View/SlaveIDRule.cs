using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PressureEmulationWPF.View
{
    internal class SlaveIDRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int resultValue;
            try
            {
                if (((string)value).Length == 0)
                    return new ValidationResult(false, $"SlaveID не может быть пустой строкой!");
                //TODO: Надо разобраться сколько SlaveID вообще может быть на одном сервере.
                resultValue = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Неправильные символы или ошибка \"{e.Message}\". Используйте формат ввода SlaveID - \"1\".");
            }
            if(resultValue > byte.MaxValue)
                return new ValidationResult(false, $"SlaveID не может быть больше 255.");
            return ValidationResult.ValidResult;
        }
    }
}
