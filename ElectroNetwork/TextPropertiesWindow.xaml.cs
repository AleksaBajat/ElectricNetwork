using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElectroNetwork;

public partial class TextPropertiesWindow : Window
{
    public double Size { get; set; } = 1;
    public string Text { get; set; }
    public string Color { get; set; }
    
    public TextPropertiesWindow()
    {
        InitializeComponent();
        DataContext = this;
        InitColorPicker();
    }

    public TextPropertiesWindow(TextBlock textBlock)
    {
        InitializeComponent();
        DataContext = this;
        InitColorPicker();

        Text = textBlock.Text;
        Size = textBlock.FontSize;
        if (textBlock.Foreground is SolidColorBrush brush)
        {
            Color = GetColorName(brush);
            ColorPicker.SelectedItem = Color;
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

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Add_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void ColorPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Color = (string)ColorPicker.SelectedItem;
    }
}