namespace PressureEmulationWPF
{
    internal class EmulationData
    {
        public String Name { get; set; }
        public DateTime Date { get; set; }
        public List<MyPoint> Values { get; set; } = new List<MyPoint>();
    }
}
