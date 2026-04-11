using LiveChartsCore.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PressureEmulationWPF
{
    internal class EmulationData
    {
        public String Name { get; set; }
        public DateTime Date { get; set; }
        public List<MyPoint> Values { get; set; } = new List<MyPoint>();
    }
}
