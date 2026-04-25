using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureEmulationWPF.Model
{
    internal class RegisterData : INotifyPropertyChanged
    {
        private string _address;
        public string? Address
        {
            get => _address;
            set
            {
                _address = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
            }
        }
        private string _registerValue;
        public string? RegisterValue
        {
            get => _registerValue;
            set
            {
                _registerValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegisterValue)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
