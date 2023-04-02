using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ElectroNetwork.Models;
using Point = ElectroNetwork.Models.Point;

namespace ElectroNetwork.Layers;

public class WireLayer
{
    private Canvas _mainLayer;
    private IElectricItem[,] _itemMatrix;
    private bool[,] _lineMatrix;


    public WireLayer(Canvas mainLayer, IElectricItem[,] itemMatrix, List<Wire> wires)
    {
        _mainLayer = mainLayer;
        _itemMatrix = itemMatrix;
        _lineMatrix = new bool[itemMatrix.GetLength(0), itemMatrix.GetLength(1)];
        ConnectWires(wires);
    }
    
    public void ConnectWires(List<Wire> wires)
    {
        foreach (Wire wire in wires)
        {
            IElectricItem firstEndItem = FindItemById(wire.FirstEnd.ToString());
            IElectricItem secondEndItem = FindItemById(wire.SecondEnd.ToString());

            if (firstEndItem == null || secondEndItem == null)
            {
                continue;
            }

            List<Point> path = FindShortestPath(firstEndItem.Point, secondEndItem.Point);

            if (path != null)
            {
                DrawPath(path, Brushes.Green, wire);
            }
        }
    }
    
    private IElectricItem FindItemById(string id)
    {
        for (int x = 0; x < _itemMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < _itemMatrix.GetLength(1); y++)
            {
                if (_itemMatrix[x, y]?.Id == id)
                {
                    return _itemMatrix[x, y];
                }
            }
        }

        return null;
    }
    
    
    private void DrawPath(List<Point> path, Brush color, Wire wire = null)
    {
        if (path == null || path.Count < 2 || wire == null)
        {
            return;
        }

        Polyline polyline = new Polyline
        {
            Stroke = color,
            StrokeThickness = 1,
            Opacity = 0.5
        };

        polyline.ToolTip = new ToolTip
        {
            Content = $"ID : {wire.Id}\nName : {wire.Name}"
        };

        foreach (Point point in path)
        {
            polyline.Points.Add(new System.Windows.Point(point.X, point.Y));
        }
        polyline.MouseLeftButtonDown += (sender, e) =>
        {
            Storyboard storyboard = new Storyboard();

            ColorAnimation polylineColorAnimation = new ColorAnimation()
            {
                From = ((SolidColorBrush)color).Color,
                To = Colors.Blue,
                Duration = TimeSpan.FromMilliseconds(300),
                AutoReverse = true
            };
            
            Storyboard.SetTarget(polylineColorAnimation, polyline);
            Storyboard.SetTargetProperty(polylineColorAnimation, new PropertyPath("(Shape.Stroke).(SolidColorBrush.Color)"));
            storyboard.Children.Add(polylineColorAnimation);
            
 

            DoubleAnimation polylineAnimation = new DoubleAnimation
            {
                From = 1,
                To = 3,
                Duration = TimeSpan.FromMilliseconds(300),
                AutoReverse = true
            };
            
            Storyboard.SetTarget(polylineAnimation, polyline);
            Storyboard.SetTargetProperty(polylineAnimation, new PropertyPath("(Shape.StrokeThickness)"));
            storyboard.Children.Add(polylineAnimation);

            IElectricItem firstItem = FindItemById(wire.FirstEnd.ToString());
            IElectricItem lastItem = FindItemById(wire.SecondEnd.ToString());

            if (firstItem != null)
            {
                AnimateConnectedItem(firstItem, storyboard);
            }

            if (lastItem != null)
            {
                AnimateConnectedItem(lastItem, storyboard);
            } 


            storyboard.Begin();
            e.Handled = true;
        };        
        
        

        _mainLayer.Children.Add(polyline);
    }
    
    private void AnimateConnectedItem(IElectricItem item, Storyboard storyboard)
    {
        Ellipse ellipse = item.Drawing;

        if (ellipse != null)
        {
            if (item.IsAnimationRunning == false)
            {
                item.IsAnimationRunning = true;
                DoubleAnimation ellipseAnimation = new DoubleAnimation
                {
                    From = ellipse.Width,
                    To = ellipse.Width * 2,
                    Duration = TimeSpan.FromMilliseconds(300),
                    AutoReverse = true
                };
                Storyboard.SetTarget(ellipseAnimation, ellipse);
                Storyboard.SetTargetProperty(ellipseAnimation, new PropertyPath("(Width)"));
                storyboard.Children.Add(ellipseAnimation);

                DoubleAnimation ellipseAnimationHeight = new DoubleAnimation
                {
                    From = ellipse.Height,
                    To = ellipse.Height * 2,
                    Duration = TimeSpan.FromMilliseconds(300),
                    AutoReverse = true
                };
                Storyboard.SetTarget(ellipseAnimationHeight, ellipse);
                Storyboard.SetTargetProperty(ellipseAnimationHeight, new PropertyPath("(Height)"));
                storyboard.Children.Add(ellipseAnimationHeight);

                storyboard.Completed += (sender, args) =>
                {
                    item.IsAnimationRunning = false;
                };
            }
        }
    }
    
    
    private List<Point> FindShortestPath(Point start, Point end)
{
    int matrixWidth = _itemMatrix.GetLength(0);
    int matrixHeight = _itemMatrix.GetLength(1);

    int startX = (int)(start.X / _mainLayer.Width * matrixWidth);
    int startY = (int)(start.Y / _mainLayer.Height * matrixHeight);

    int endX = (int)(end.X / _mainLayer.Width * matrixWidth);
    int endY = (int)(end.Y / _mainLayer.Height * matrixHeight);

    Queue<(int, int)> queue = new Queue<(int, int)>();
    Dictionary<(int, int), (int, int)> previous = new Dictionary<(int, int), (int, int)>();
    queue.Enqueue((startX, startY));

    while (queue.Count > 0)
    {
        var current = queue.Dequeue();

        if (current == (endX, endY))
        {
            break;
        }

        var neighbors = GetNeighbors(current, matrixWidth, matrixHeight);

        foreach (var neighbor in neighbors)
        {
            if (!previous.ContainsKey(neighbor) && !_lineMatrix[neighbor.Item1, neighbor.Item2])
            {
                previous[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }
    }

    if (!previous.ContainsKey((endX, endY)))
    {
        return null;
    }

    List<Point> path = new List<Point>();
    var currentPosition = (endX, endY);

    while (currentPosition != (startX, startY))
    {
        path.Add(new Point(
            currentPosition.Item1 * _mainLayer.Width / matrixWidth,
            currentPosition.Item2 * _mainLayer.Height / matrixHeight
        ));
        currentPosition = previous[currentPosition];
    }

    path.Add(start);
    path.Reverse();

    return path;
}

    private List<(int, int)> GetNeighbors((int, int) position, int matrixWidth, int matrixHeight)
    {
        int x = position.Item1;
        int y = position.Item2;

        var neighbors = new List<(int, int)>
        {
            (x + 1, y),
            (x - 1, y),
            (x, y + 1),
            (x, y - 1)
        };

        return neighbors
            .Where(neighbor => neighbor.Item1 >= 0 && neighbor.Item1 < matrixWidth &&
                               neighbor.Item2 >= 0 && neighbor.Item2 < matrixHeight)
            .ToList();
    }
}