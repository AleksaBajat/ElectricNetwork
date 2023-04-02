using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using ElectroNetwork.CustomShapes;

namespace ElectroNetwork;

public partial class EllipsePropertiesWindow : Window
{
    public double EllipseWidth { get; set; } = 1;
    
    public double EllipseHeight { get; set; } = 1;
    public double Thickness { get; set; } = 1;

    public double EllipseOpacity { get; set; } = 1;
    public string Text { get; set; }
    public string Color { get; set; }
    
    public string ColorStroke { get; set; }
    
    public string ColorText { get; set; }
    
    
    public EllipsePropertiesWindow()
    {
        InitializeComponent();
        DataContext = this;
        InitColorPicker();
    }

    public EllipsePropertiesWindow(TextEllipse textEllipse)
    {
        InitializeComponent();
        DataContext = this;
        InitColorPicker();
        EllipseWidth = textEllipse.Ellipse.Width;
        EllipseHeight = textEllipse.Ellipse.Height;
        Thickness = textEllipse.Ellipse.StrokeThickness;
        Opacity = textEllipse.Ellipse.Opacity;

        if (textEllipse.Ellipse.Fill is SolidColorBrush brush)
        {
            Color = GetColorName(brush);
            ColorPicker.SelectedItem = Color;
        }
        if (textEllipse.Ellipse.Stroke is SolidColorBrush strokeBrush)
        {
            ColorStroke = GetColorName(strokeBrush);
            ColorPickerStroke.SelectedItem = ColorStroke;
        }

        if (textEllipse.TextBlock != null)
        {
            if (textEllipse.TextBlock.Foreground is SolidColorBrush textBrush)
            {
                ColorText = GetColorName(textBrush);
                ColorPickerText.SelectedItem = ColorText;
            }
            Text = textEllipse.TextBlock.Text;
            EllipseOpacity = textEllipse.TextBlock.Opacity;
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

    private void ColorPickerStroke_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ColorStroke = (string)ColorPickerStroke.SelectedItem;
    }

    private void ColorPickerText_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ColorText = (string)ColorPickerText.SelectedItem;
    }
}