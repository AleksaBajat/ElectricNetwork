using System;
using ElectroNetwork.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Shapes;
using ElectroNetwork.CustomShapes;
using ElectroNetwork.Layers;
using Point = System.Windows.Point;

namespace ElectroNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _startMousePosition;
        private static int _nextZIndex = 2;
        private bool _isAddEllipseModeActive = false;
        private bool _isAddPolygonModeActive = false;
        private bool _isAddTextModeActive = false;
        private PointCollection _polygonPointCollection = new PointCollection();
        private object _lastAddedObject = null;
        private object _lastUndoneObject = null;
        private List<object> _addedObjects = new List<object>();

        public double ScaleValue
        {
            get { return Scale.ScaleX; }
            set
            {
                Scale.ScaleX = value;
                Scale.ScaleY = value;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            StyleMainLayer(MainLayer);
            var datalayer = new DataLayer(MainLayer);
        }
        void StyleMainLayer(Canvas mainLayer)
        {
            mainLayer.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainLayer.VerticalAlignment = VerticalAlignment.Stretch;
            mainLayer.Background = Brushes.AntiqueWhite;
            Canvas.SetZIndex(mainLayer, 0);
            mainLayer.Height = 750;
            mainLayer.Width = 750;
            mainLayer.MouseWheel += Zoom;
            mainLayer.MouseDown += Canvas_MouseDown;
            mainLayer.MouseUp += Canvas_MouseUp;
            mainLayer.MouseMove += Canvas_MouseMove;
        }

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? 1.1 : 0.9;
            Point mousePosition = e.GetPosition(MainLayer);
            Point controlPosition = Scale.Inverse.Transform(mousePosition);

            Scale.ScaleX = Scale.ScaleX * zoom;
            Scale.ScaleY = Scale.ScaleY * zoom;
            Scale.CenterX = controlPosition.X;
            Scale.CenterY = controlPosition.Y;
            
            ZoomSlider.Value = Scale.ScaleX;
        }
        
        
        
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _startMousePosition = e.GetPosition(MainLayer);
                MainLayer.CaptureMouse();
            }
        }
        
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                MainLayer.ReleaseMouseCapture();
            }
        }
        
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && MainLayer.IsMouseCaptured)
            {
                Point currentPosition = e.GetPosition(MainLayer);
                double deltaX = currentPosition.X - _startMousePosition.X;
                double deltaY = currentPosition.Y - _startMousePosition.Y;

                deltaX = Math.Round(deltaX, 2);
                deltaY = Math.Round(deltaY, 2);

                MoveChildren(deltaX, deltaY);

                _startMousePosition = currentPosition;
            }
        }

        private void MoveChildren(double deltaX, double deltaY)
        {
            Translate.X += deltaX;
            Translate.Y += deltaY;
        }

        private void DrawEllipse_Click(object sender, RoutedEventArgs e)
        {
            _isAddEllipseModeActive = true;
            MainLayer.MouseDown += AddEllipse_Click;
            MainLayer.MouseDown -= AddPolygon_Click;
            MainLayer.MouseDown -= AddText_Click;
        }

        private async void AddEllipse_Click(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(MainLayer);

            var ellipsePropertiesWindow = new EllipsePropertiesWindow();

            if (ellipsePropertiesWindow.ShowDialog() == true)
            {
                var strokeColor =
                    (SolidColorBrush)new BrushConverter().ConvertFromString(ellipsePropertiesWindow.ColorStroke);
                
                var fillColor = 
                    (SolidColorBrush)new BrushConverter().ConvertFromString(ellipsePropertiesWindow.Color);
                
                Ellipse newEllipse = new Ellipse
                {
                    Width = ellipsePropertiesWindow.EllipseWidth,
                    Height = ellipsePropertiesWindow.EllipseHeight,
                    Stroke = strokeColor ?? new SolidColorBrush(Colors.Black),
                    StrokeThickness = ellipsePropertiesWindow.Thickness,
                    Fill = fillColor ?? new SolidColorBrush(Colors.White),
                    Opacity = ellipsePropertiesWindow.EllipseOpacity,
                    IsHitTestVisible = true
                };


                TextEllipse textEllipse = new TextEllipse();
                textEllipse.Ellipse = newEllipse;
                
                Canvas.SetLeft(newEllipse, position.X - newEllipse.Width / 2);
                Canvas.SetTop(newEllipse, position.Y - newEllipse.Height / 2);
                
                Canvas.SetZIndex(newEllipse, _nextZIndex);
                _nextZIndex++;
                MainLayer.Children.Add(newEllipse);
                _addedObjects.Add((object) newEllipse);
                _lastAddedObject = textEllipse;
                _lastUndoneObject = null;
                textEllipse.Ellipse.MouseDown += Ellipse_MouseDown;
                textEllipse.Ellipse.Tag = textEllipse;

                if (!string.IsNullOrWhiteSpace(ellipsePropertiesWindow.Text))
                {
                    var textColor =
                        (SolidColorBrush)new BrushConverter().ConvertFromString(ellipsePropertiesWindow.ColorText);
                    
                    var textBlock = new TextBlock
                    {
                        Text = ellipsePropertiesWindow.Text,
                        FontWeight = FontWeights.Bold,
                        Foreground = textColor ?? new SolidColorBrush(Colors.Black)
                    };
                    
                    CenterTextInTheEllipse(textBlock, newEllipse);
                    Canvas.SetZIndex(textBlock, _nextZIndex);
                    _nextZIndex++;
                    textEllipse.TextBlock = textBlock;
                    
                    MainLayer.Children.Add(textBlock);
                    _addedObjects.Add((object) textBlock);
                }

            }

            MainLayer.MouseDown -= AddEllipse_Click;
            _isAddEllipseModeActive = false;
        }


        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !_isAddEllipseModeActive && !_isAddPolygonModeActive && !_isAddTextModeActive)
            {
                e.Handled = true;

                if (sender is Ellipse clickedEllipse)
                {
                    TextEllipse textEllipse = (TextEllipse)clickedEllipse.Tag;
                    
                    var ellipsePropertiesWindow = new EllipsePropertiesWindow(textEllipse);

                    if (ellipsePropertiesWindow.ShowDialog() == true)
                    {
                        textEllipse.Ellipse.Height = ellipsePropertiesWindow.EllipseHeight;
                        textEllipse.Ellipse.Width = ellipsePropertiesWindow.EllipseWidth;
                        textEllipse.Ellipse.StrokeThickness = ellipsePropertiesWindow.Thickness;
                        textEllipse.Ellipse.Opacity = ellipsePropertiesWindow.EllipseOpacity;

                        var color =
                            (SolidColorBrush)new BrushConverter().ConvertFromString(
                                ellipsePropertiesWindow.Color);
                        var strokeColor = 
                            (SolidColorBrush)new BrushConverter().ConvertFromString(
                                ellipsePropertiesWindow.ColorStroke);
                        var textColor = 
                            (SolidColorBrush)new BrushConverter().ConvertFromString(
                                ellipsePropertiesWindow.ColorText);
                        
                        textEllipse.Ellipse.Stroke = strokeColor ?? new SolidColorBrush(Colors.Black);
                        textEllipse.Ellipse.Fill = color ?? new SolidColorBrush(Colors.White);
                        if (textEllipse.TextBlock != null)
                        {
                            textEllipse.TextBlock.Foreground = textColor ?? new SolidColorBrush(Colors.Black);
                            textEllipse.TextBlock.Text = ellipsePropertiesWindow.Text;
                        }
                        else
                        {
                            textEllipse.TextBlock = new TextBlock()
                            {
                                Text = ellipsePropertiesWindow.Text,
                                Foreground = textColor ?? new SolidColorBrush(Colors.Black),
                                FontWeight = FontWeights.Bold,
                            };
                            CenterTextInTheEllipse(textEllipse.TextBlock, textEllipse.Ellipse);
                            Canvas.SetZIndex(textEllipse.TextBlock, _nextZIndex);
                            _nextZIndex++;
                            
                            MainLayer.Children.Add(textEllipse.TextBlock);
                            _addedObjects.Add((object) textEllipse.TextBlock);
                        }
                        
                        CenterTextInTheEllipse(textEllipse.TextBlock, textEllipse.Ellipse);
                    }
                }
            }
        }


        private void CenterTextInTheEllipse(TextBlock textBlock, Ellipse ellipse)
        {
            double ellipseCenterX = Canvas.GetLeft(ellipse) + ellipse.Width / 2;
            double ellipseCenterY = Canvas.GetTop(ellipse) + ellipse.Height / 2;

            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            double textBlockLeft = ellipseCenterX - textBlock.DesiredSize.Width / 2;
            double textBlockTop = ellipseCenterY - textBlock.DesiredSize.Height / 2;

            Canvas.SetLeft(textBlock, textBlockLeft);
            Canvas.SetTop(textBlock, textBlockTop);
        }
        
        private Point GetPolygonCenter(PointCollection points)
        {
            double centerX = points.Average(p => p.X);
            double centerY = points.Average(p => p.Y);
            return new Point(centerX, centerY);
        }

        private void CenterTextInPolygon(TextBlock textBlock, Polygon polygon, Point referencePoint)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size textSize = textBlock.DesiredSize;
            Point center = GetPolygonCenter(polygon.Points);

            double left = center.X - textSize.Width / 2;
            double top = center.Y - textSize.Height / 2;

                Canvas.SetLeft(textBlock, left + referencePoint.X);
                Canvas.SetTop(textBlock, top + referencePoint.Y);
        }

        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            _isAddPolygonModeActive = true;
            MainLayer.MouseDown += AddPolygon_Click;
            MainLayer.MouseDown -= AddEllipse_Click;
            MainLayer.MouseDown -= AddText_Click;
        }

        private void AddPolygon_Click(object sender, MouseButtonEventArgs e)
        {
            PointCollection points = _polygonPointCollection; 
            if (e.ChangedButton == MouseButton.Right)
            {
                var point = e.GetPosition(MainLayer);
                points.Add(point);
            }

            if (e.ChangedButton == MouseButton.Left)
            {

                var polygonPropertiesWindow = new PolygonPropertiesWindow();

                if (polygonPropertiesWindow.ShowDialog() == true)
                {
                    var strokeColor =
                        (SolidColorBrush)new BrushConverter().ConvertFromString(polygonPropertiesWindow.ColorStroke);
                    
                    var fillColor = 
                        (SolidColorBrush)new BrushConverter().ConvertFromString(polygonPropertiesWindow.Color);
                    
                    Polygon newPolygon = new Polygon
                    {
                        Stroke = strokeColor ?? new SolidColorBrush(Colors.Black), 
                        StrokeThickness = polygonPropertiesWindow.Thickness,
                        Fill = fillColor ?? new SolidColorBrush(Colors.White),
                    };
                    Point referencePoint = new Point(
                        points.Min(p => p.X), points.Min(p => p.Y)
                    );
                    newPolygon.Points = new PointCollection();
                    foreach (Point point in points)
                    {
                        newPolygon.Points.Add(new Point(point.X - referencePoint.X, point.Y - referencePoint.Y));
                    }

                    TextPolygon textPolygon = new TextPolygon();
                    textPolygon.Polygon = newPolygon;

                    _lastAddedObject = textPolygon;
                    _lastUndoneObject = null;
                    
                    Canvas.SetLeft(newPolygon, referencePoint.X);
                    Canvas.SetTop(newPolygon, referencePoint.Y);
                    
                    Canvas.SetZIndex(newPolygon, _nextZIndex);
                    _nextZIndex++;
                    points.Clear();
                    MainLayer.Children.Add(newPolygon);
                    _addedObjects.Add(newPolygon);
                   textPolygon.Polygon.MouseDown += Polygon_MouseDown;
                   textPolygon.Polygon.Tag = textPolygon;

                    if (!string.IsNullOrWhiteSpace(polygonPropertiesWindow.Text))
                    {
                        var textColor =
                            (SolidColorBrush)new BrushConverter().ConvertFromString(polygonPropertiesWindow.ColorText);
                        
                        var textBlock = new TextBlock
                        {
                            Text = polygonPropertiesWindow.Text,
                            FontWeight = FontWeights.Bold,
                            Foreground = textColor ?? new SolidColorBrush(Colors.Black) 
                        };
                        
                        
                       CenterTextInPolygon(textBlock, newPolygon, referencePoint); 
                       Canvas.SetZIndex(textBlock, _nextZIndex);
                       _nextZIndex++;
                       textPolygon.TextBlock = textBlock;

                       MainLayer.Children.Add(textBlock);
                       _addedObjects.Add(textBlock);
                    }

                }

                MainLayer.MouseDown -= AddPolygon_Click;
                _isAddPolygonModeActive = false;
            }
        }
        
        
        private Point GetPolygonCenterEdit(PointCollection points)
        {
            double centerX = 0;
            double centerY = 0;

            foreach (var point in points)
            {
                centerX += point.X;
                centerY += point.Y;
            }

            centerX /= points.Count;
            centerY /= points.Count;

            return new Point(centerX, centerY);
        }
        
        private void CenterTextInPolygonEdit(TextBlock textBlock, Polygon polygon, Point referencePoint)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size textSize = textBlock.DesiredSize;
            Point center = GetPolygonCenter(polygon.Points);

            double left = center.X - textSize.Width / 2;
            double top = center.Y - textSize.Height / 2;

            double polygonLeft = Canvas.GetLeft(polygon);
            double polygonTop = Canvas.GetTop(polygon);

            if (double.IsNaN(polygonLeft)) polygonLeft = 0;
            if (double.IsNaN(polygonTop)) polygonTop = 0;

            Canvas.SetLeft(textBlock, left + polygonLeft);
            Canvas.SetTop(textBlock, top + polygonTop);
        }


        private void Polygon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !_isAddEllipseModeActive && !_isAddPolygonModeActive && !_isAddTextModeActive)
            {
                e.Handled = true;

                if (sender is Polygon clickedPolygon)
                {
                    TextPolygon textPolygon = (TextPolygon)clickedPolygon.Tag;

                    var polygonPropertiesWindow = new PolygonPropertiesWindow(textPolygon);

                    if (polygonPropertiesWindow.ShowDialog() == true)
                    {
                        textPolygon.Polygon.StrokeThickness = polygonPropertiesWindow.Thickness;
                        
                        var color =
                            (SolidColorBrush)new BrushConverter().ConvertFromString(
                                polygonPropertiesWindow.Color);
                        var strokeColor = 
                            (SolidColorBrush)new BrushConverter().ConvertFromString(
                                polygonPropertiesWindow.ColorStroke);

                        SolidColorBrush textColor;
                        try
                        {
                            textColor =
                                (SolidColorBrush)new BrushConverter().ConvertFromString(
                                    polygonPropertiesWindow.ColorText);
                        }
                        catch
                        {
                            textColor = new SolidColorBrush(Colors.Black);
                        }

                        textPolygon.Polygon.Stroke = strokeColor ?? new SolidColorBrush(Colors.Black);
                        textPolygon.Polygon.Fill = color ?? new SolidColorBrush(Colors.White);
                        textPolygon.Polygon.Opacity = polygonPropertiesWindow.PolygonOpacity;
                        textPolygon.Polygon.IsHitTestVisible = true;
                        Point referencePoint = new Point(
                            textPolygon.Polygon.Points.Min(p => p.X), textPolygon.Polygon.Points.Min(p => p.Y)
                        );
                        if (textPolygon.TextBlock != null)
                        {
                            textPolygon.TextBlock.Foreground = textColor ?? new SolidColorBrush(Colors.Black);
                            textPolygon.TextBlock.Text = polygonPropertiesWindow.Text;
                        }
                        else
                        {
                            textPolygon.TextBlock = new TextBlock()
                            {
                                Text = polygonPropertiesWindow.Text,
                                Foreground = textColor ?? new SolidColorBrush(Colors.Black),
                                FontWeight = FontWeights.Bold,
                            };
                           CenterTextInPolygonEdit(textPolygon.TextBlock, textPolygon.Polygon, GetPolygonCenterEdit(textPolygon.Polygon.Points)); 
                           Canvas.SetZIndex(textPolygon.TextBlock, _nextZIndex);
                           _nextZIndex++;

                           MainLayer.Children.Add(textPolygon.TextBlock);
                           _addedObjects.Add(textPolygon.TextBlock);
                        }
                    }
                }
            }
        }

        private void DrawText_Click(object sender, RoutedEventArgs e)
        {
            _isAddTextModeActive = true;
            MainLayer.MouseDown += AddText_Click;
            MainLayer.MouseDown -= AddEllipse_Click;
            MainLayer.MouseDown -= AddPolygon_Click;
        }

        private async void AddText_Click(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(MainLayer);

            var textPropertiesWindow = new TextPropertiesWindow();

            if (textPropertiesWindow.ShowDialog() == true)
            {
                var textColor = 
                    (SolidColorBrush)new BrushConverter().ConvertFromString(textPropertiesWindow.Color);


                TextBlock textBlock = new TextBlock()
                {
                    FontSize = textPropertiesWindow.Size,
                    Text = textPropertiesWindow.Text,
                    Foreground = textColor ?? new SolidColorBrush(Colors.Black)
                };
                
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size textSize = textBlock.DesiredSize;
                
                Canvas.SetLeft(textBlock, position.X - textSize.Width / 2);
                Canvas.SetTop(textBlock, position.Y - textSize.Height / 2);
                Canvas.SetZIndex(textBlock, _nextZIndex);
                _nextZIndex++;
                textBlock.MouseDown += Text_MouseDown;
                MainLayer.Children.Add(textBlock);
                _addedObjects.Add(textBlock);
                _lastAddedObject = textBlock;
                _lastUndoneObject = null;
            }


            MainLayer.MouseDown -= AddText_Click;
            _isAddTextModeActive = false;

        }

        private void Text_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !_isAddEllipseModeActive && !_isAddPolygonModeActive &&
                !_isAddTextModeActive)
            {
                e.Handled = true;

                if (sender is TextBlock clickedText)
                {
                    var textPropertiesWindow = new TextPropertiesWindow(clickedText);

                    if (textPropertiesWindow.ShowDialog() == true)
                    {
                        clickedText.Text = textPropertiesWindow.Text;
                        clickedText.FontSize = textPropertiesWindow.Size;
                            
                        var color =
                            (SolidColorBrush)new BrushConverter().ConvertFromString(
                                textPropertiesWindow.Color);

                        clickedText.Foreground = color ?? new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_lastAddedObject != null)
            {
                _lastUndoneObject = _lastAddedObject;
                if (_lastAddedObject is TextEllipse textEllipse)
                {
                    MainLayer.Children.Remove(textEllipse.TextBlock); 
                    MainLayer.Children.Remove(textEllipse.Ellipse);
                } else if (_lastAddedObject is TextPolygon textPolygon)
                {
                    MainLayer.Children.Remove(textPolygon.TextBlock);
                    MainLayer.Children.Remove(textPolygon.Polygon);
                }else if (_lastAddedObject is TextBlock textBlock)
                {
                    MainLayer.Children.Remove(textBlock);
                }
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_lastUndoneObject != null)
            {
                _lastAddedObject = _lastUndoneObject;
                _lastUndoneObject = null;
                if (_lastAddedObject is TextEllipse textEllipse)
                {
                    MainLayer.Children.Add(textEllipse.TextBlock); 
                    MainLayer.Children.Add(textEllipse.Ellipse);
                } else if (_lastAddedObject is TextPolygon textPolygon)
                {
                    MainLayer.Children.Add(textPolygon.TextBlock);
                    MainLayer.Children.Add(textPolygon.Polygon);
                }else if (_lastAddedObject is TextBlock textBlock)
                {
                    MainLayer.Children.Add(textBlock);
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            var copyChildrenList = new List<object>();

            foreach (var child in MainLayer.Children)
            {
                   copyChildrenList.Add(child); 
            }

            foreach (object o in copyChildrenList)
            {
                if (_addedObjects.Contains(o))
                {
                    MainLayer.Children.Remove((UIElement)o);
                }
            }

            _lastAddedObject = null;
            _lastUndoneObject = null;
        }
    }
} 