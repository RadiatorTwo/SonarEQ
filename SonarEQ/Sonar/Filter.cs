using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarEQ.Sonar
{
    public class EQFilter
    {
        public EQFilter()
        {
            IsEmpty = true;
        }

        public EQFilter(int iD, double frequency, double gain, double qFactor, string type)
        {
            ID = iD;
            Frequency = frequency;
            Gain = gain;
            QFactor = qFactor;
            Type = type;
        }

        public int ID { get; set; }
        public double Frequency { get; set; }
        public double Gain { get; set; }
        public double QFactor { get; set; }
        public string Type { get; set; } = string.Empty;

        public bool IsEmpty { get; set; }
    }
}
