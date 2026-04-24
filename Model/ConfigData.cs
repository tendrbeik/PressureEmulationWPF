using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace PressureEmulationWPF.Model
{
    class ConfigData
    {
        public double UpperPressureLimit { get; set; }
        public double ConstantPressureValue { get; set; }
        public double PressureDelta { get; set; }
        public double StartPressureValue { get; set; }
        public string EmulationName { get; set; }
        public DateTime EmulationDateTime { get; set; }
        public bool RandomPressureMode { get; set; }
        public bool ConstantPressureMode { get; set; }
        public bool ConstantChangingPressureMode { get; set; }
        //Пока я не буду добавлять это свойство в JSON файл, так как у меня не получилось его технически реализовать.
        //public EmulationData SelectedEmulation {  get; set; }
        //Свойства с вкладки ModbusSlaveTab
        public string SlaveIP { get; set; }
        public int SlavePort { get; set; }
        public byte SlaveID { get; set; }
        public int InputRegisterValueAddress { get; set; }
        public int Count { get; set; }
        public bool IsBigEndian { get; set; }
        public string SelectedRegisterType { get; set; }
        private List<string> _valueTypes = new List<string>();
        public List<string> ValueTypes
        {
            get { return _valueTypes; }
            set { _valueTypes = value; }
        }
        public string SelectedValueType { get; set; }
    }
}
