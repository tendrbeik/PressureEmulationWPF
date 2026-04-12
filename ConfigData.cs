namespace PressureEmulationWPF
{
    class ConfigData
    {
        public double UpperPressureLimit { get; set; }
        public double ConstantPressureValue { get; set; }
        public double PressureDelta { get; set; }
        public double StartPressureValue {  get; set; }
        public string EmulationName {  get; set; }
        public DateTime EmulationDateTime {  get; set; }
        public bool RandomPressureMode { get; set; }
        public bool ConstantPressureMode { get; set; }
        public bool ConstantChangingPressureMode { get; set; }
        //Пока я не буду добавлять это свойство в JSON файл, так как у меня не получилось его технически реализовать.
        //public EmulationData SelectedEmulation {  get; set; }
    }
}
