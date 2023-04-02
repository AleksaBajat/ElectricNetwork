using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ElectroNetwork.Models;

namespace ElectroNetwork.Layers;

public class DataLayer
{
    private Canvas _mainLayer;
    private int _matrixWidth = 300;
    private int _matrixHeight = 240;
    private IElectricItem[,] _state;
    public List<Node> Nodes { get; set; } = new List<Node>();
    public List<Substation> Substations { get; set; } = new List<Substation>();
    public List<Switch> Switches { get; set; } = new List<Switch>();
    public List<Wire> Wires { get; set; } = new List<Wire>();

    public DataLayer(Canvas mainLayer)
    {
        _mainLayer = mainLayer;
        _state = new IElectricItem[_matrixWidth, _matrixHeight];
        DataAccess.InitData(Nodes, Substations, Switches, Wires);
        AddItemsToCanvas(Substations, Nodes, Switches);
        WireLayer wires = new WireLayer(mainLayer, _state, Wires);

    }
    
    private void AddItemsToCanvas(List<Substation> substations, List<Node> nodes, List<Switch> switches)
    {
        // Find the bounding box of all points
        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;

        List<IElectricItem> allItems = substations.Concat<IElectricItem>(nodes).Concat(switches).ToList();

        foreach (var item in allItems)
        {
            minX = Math.Min(minX, item.Point.X);
            maxX = Math.Max(maxX, item.Point.X);
            minY = Math.Min(minY, item.Point.Y);
            maxY = Math.Max(maxY, item.Point.Y);
        }

        // Normalize points
        double width = maxX - minX;
        double height = maxY - minY;
        double scaleFactor = Math.Min((_mainLayer.Width - 40) / width, (_mainLayer.Height - 40) / height); 
        double margin = 20;

        foreach (IElectricItem item in allItems)
        {
            Point point = item.Point;
            point.X = margin + (point.X - minX) * scaleFactor;
            point.Y = _mainLayer.Height - margin - (point.Y - minY) * scaleFactor; 

            int matrixX = (int)(point.X / _mainLayer.Width * _matrixWidth);
            int matrixY = (int)(point.Y / _mainLayer.Height * _matrixHeight);

            if (_state[matrixX, matrixY] != null)
            {
                (matrixX, matrixY) = FindClosestAvailableSlot(matrixX, matrixY);
            }

            matrixX = Math.Min(matrixX, _matrixWidth - 1);
            matrixY = Math.Min(matrixY, _matrixHeight - 1);

            _state[matrixX, matrixY] = item;
            item.Point = new Point(matrixX * (_mainLayer.Width / _matrixWidth), matrixY * (_mainLayer.Height / _matrixHeight));
        }
        // Add ellipses to the canvas
        foreach (Substation substation in substations)
        {
            substation.Drawing = AddEllipseToCanvas(substation.Point, Brushes.Red, substation);
        }

        foreach (Node node in nodes)
        {
            node.Drawing = AddEllipseToCanvas(node.Point, Brushes.Blue, node);
        }

        foreach (Switch sw in switches)
        {
            sw.Drawing = AddEllipseToCanvas(sw.Point, Brushes.Green, sw);
        }
    }
    
    private (int, int) FindClosestAvailableSlot(int startX, int startY)
    {
        for (int distance = 1; distance < Math.Max(_matrixWidth, _matrixHeight); distance++)
        {
            for (int x = Math.Max(0, startX - distance); x <= Math.Min(_matrixWidth - 1, startX + distance); x++)
            {
                for (int y = Math.Max(0, startY - distance); y <= Math.Min(_matrixHeight - 1, startY + distance); y++)
                {
                    if (_state[x, y] == null)
                    {
                        return (x, y);
                    }
                }
            }
        }

        return (startX, startY); // Fallback, should not happen if the matrix is large enough
    }

    private Ellipse AddEllipseToCanvas(Point point, Brush color, IElectricItem item = null)
    {
        Ellipse ellipse = new Ellipse
        {
            Width = 2,
            Height = 2,
            Fill = color
        };

        Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
        Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

        if (item != null)
        {
            ellipse.ToolTip = new ToolTip
            {
                Content = $"ID : {item.Id}\nName : {item.Name}"
            };
        }

        _mainLayer.Children.Add(ellipse);

        return ellipse;
    }
    
}