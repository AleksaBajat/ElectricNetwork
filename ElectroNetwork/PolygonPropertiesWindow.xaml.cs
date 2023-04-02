using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ElectroNetwork.CustomShapes;

namespace ElectroNetwork;

public partial class PolygonPropertiesWindow : Window
{
    public double PolygonWidth { get; set; } = 1;
    
    public double PolygonHeight { get; set; } = 1;
    
    public double Thickness { get; set; } = 1;
    public string Text { get; set; }

    public string Color { get; set; } = "";

    public double PolygonOpacity { get; set; } = 1;

    public string ColorStroke { get; set; } = "";

    public string ColorText { get; set; } = "";
    public PolygonPropertiesWindow()
    {
        InitializeComponent();
        DataContext = this;
        InitColorPicker();
    }
    
    public PolygonPropertiesWindow(TextPolygon textPolygon)
    {
        InitializeComponent();
        DataContext = this;
        InitColorPicker();
        PolygonWidth = textPolygon.Polygon.Width;
        PolygonHeight = textPolygon.Polygon.Height;
        Thickness = textPolygon.Polygon.StrokeThickness;
        PolygonOpacity = textPolygon.Polygon.Opacity;

        if (textPolygon.Polygon.Fill is SolidColorBrush brush)
        {
            Color = GetColorName(brush);
            ColorPicker.SelectedItem = Color;
        }
        if (textPolygon.Polygon.Stroke is SolidColorBrush strokeBrush)
        {
            ColorStroke = GetColorName(strokeBrush);
            ColorPickerStroke.SelectedItem = ColorStroke;
        }

        if (textPolygon.TextBlock != null)
        {
            if (textPolygon.TextBlock.Foreground is SolidColorBrush textBrush)
            {
                ColorText = GetColorName(textBrush);
                ColorPickerText.SelectedItem = ColorText;
            }
            Text = textPolygon.TextBlock.Text;
        }
    }
    private string GetColorName(SolidColorBrush brush)
    {
        var results = typeof(Colors).GetProperties().Where(
            p => (Color)p.GetValue(null, null) == brush.Color).Select(p => p.Name);
        
        return results.Count() > 0 ? results.First() : String.Empty;
    }

    private void InitColorPicker()
    {
        List<string> allColors = GetAllAvailableColors();

        foreach (var color in allColors)
        {
            ColorPicker.Items.Add(color);
            ColorPickerStroke.Items.Add(color);
            ColorPickerText.Items.Add(color);
        }
    }
    
    public List<string> GetAllAvailableColors()
    {
        List<string> colorNames = new List<string>();

        PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType == typeof(Color))
            {
                colorNames.Add(property.Name);
            }
        }

        return colorNames;
    }


    private void ColorPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Color = (string)ColorPicker.SelectedItem;
    }
    
    private void ColorPickerStroke_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ColorStroke = (string)ColorPickerStroke.SelectedItem;
    }

    private void ColorPickerText_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ColorText = (string)ColorPickerText.SelectedItem;
    }

    private void Add_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}