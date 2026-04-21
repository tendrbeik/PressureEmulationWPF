using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureEmulationWPF.Model
{
    internal class RegisterData<T>
    {
        public string? Address  { get; set; }
        public T? RegisterValue { get; set; }
    }
}
