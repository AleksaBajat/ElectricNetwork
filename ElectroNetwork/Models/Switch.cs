using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ElectroNetwork.Models
{
    public class Switch:IElectricItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Point Point { get; set; }
        public Ellipse Drawing { get; set; }

        public bool IsAnimationRunning { get; set; } = false;


        public Switch(string id, string name, Point point)
        {
            Id = id;
            Name = name;
            Point = point;
        }
    }
}
