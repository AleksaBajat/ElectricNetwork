using System.Windows.Shapes;

namespace ElectroNetwork.Models;

public interface IElectricItem
{
    string Id { get; set; }
    string Name { get; set; }
    Point Point { get; set; }
    
    Ellipse Drawing { get; set; }
    
    bool IsAnimationRunning { get; set; } 
}