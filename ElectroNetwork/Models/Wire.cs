using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroNetwork.Models
{
    public class Wire
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsUnderground { get; set; }

        public double R { get; set; }

        public string LineType { get; set; }

        public int ThermalConstantHeat { get; set; }

        public int FirstEnd { get; set; }

        public int SecondEnd { get; set; }
       
        public Point StartPoint { get; set; }

        public Point EndPoint { get; set; }

        public Wire(string id, string name, bool isUnderground, double r, string lineType, int thermalConstantHeat, int firstEnd, int secondEnd, Point startPoint, Point endPoint)
        {
            Id = id;
            Name = name;
            IsUnderground = isUnderground;
            R = r;
            LineType = lineType;
            ThermalConstantHeat = thermalConstantHeat;
            FirstEnd = firstEnd;
            SecondEnd = secondEnd;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}
