using System.Windows.Controls;
using System.Windows.Media;

namespace ElectroNetwork.Layers;

public interface ILayer
{
    void Render(Canvas layer);
}