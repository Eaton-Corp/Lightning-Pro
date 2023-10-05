using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace LightningPRO.Views
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : UserControl
    {
        public Editor()
        {
            InitializeComponent();
            orderedChildren = new List<ChildDrawing>();
            childrenCounter = 0;
        }

        Point currentPoint = new Point();
        

        public enum Item
        {
            Brush, Pencil, Highlighter, Rectangle, Text, Pan, Check, Cross, Short
        }

        public enum ColourSelected
        {
            Red, Blue, Green, Indigo
        }

        Item CurrItem;
        ColourSelected CurrentColour = ColourSelected.Red;


        public class ChildDrawing
        {
            public int Index { get; set; }
            public int Length { get; set; }
        }

        readonly List<ChildDrawing> orderedChildren;
        int childrenCounter;

       

        private Point startPoint;
        private Rectangle rect;
        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(Drawing);
            if(CurrItem == Item.Rectangle)
            {
                startPoint = e.GetPosition(Drawing);
                rect = new Rectangle
                {
                    StrokeThickness = 2
                };

                if (CurrentColour == ColourSelected.Blue)
                {
                    rect.Stroke = Brushes.Blue;
                }
                else if (CurrentColour == ColourSelected.Green)
                {
                    rect.Stroke = Brushes.Green;
                }
                else if (CurrentColour == ColourSelected.Indigo)
                {
                    rect.Stroke = Brushes.Indigo;
                }
                else    // all other cases default to red
                {
                    rect.Stroke = Brushes.Red;
                }

                Canvas.SetLeft(rect, startPoint.X);
                Canvas.SetTop(rect, startPoint.Y);
                

                int rectIndex = Drawing.Children.Add(rect);
                int rectRange = 1;

                orderedChildren.Insert(0, new ChildDrawing() { Index = rectIndex, Length = rectRange });
                childrenCounter++;
            }
            else if(CurrItem == Item.Text)
            {
                TextBox box = new TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Thickness margin = new Thickness(e.GetPosition(Drawing).X, e.GetPosition(Drawing).Y, 0, 0);
                box.Margin = margin;
                
                int textIndex = Drawing.Children.Add(box);
                int textRange = 1;

                orderedChildren.Insert(0, new ChildDrawing() { Index = textIndex, Length = textRange });
                childrenCounter++;
            }
            else if (CurrItem == Item.Check)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = "✔",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 22,
                    Width = 28,
                    FontSize = 20
                };

                if (CurrentColour == ColourSelected.Red)
                {
                    textBlock.Foreground = Brushes.Red;
                }
                else if (CurrentColour == ColourSelected.Blue)
                {
                    textBlock.Foreground = Brushes.Blue;
                }
                else if (CurrentColour == ColourSelected.Indigo)
                {
                    textBlock.Foreground = Brushes.Indigo;
                }
                else 
                {
                    textBlock.Foreground = Brushes.Green;
                }

                Canvas.SetLeft(textBlock, e.GetPosition(Drawing).X - 14);
                Canvas.SetTop(textBlock, e.GetPosition(Drawing).Y - 15);
               
                int checkIndex = Drawing.Children.Add(textBlock);
                int checkRange = 1;

                orderedChildren.Insert(0, new ChildDrawing() { Index = checkIndex, Length = checkRange });
                childrenCounter++;
            }
            else if (CurrItem == Item.Cross)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = "❌",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontSize = 14
                };

                if (CurrentColour == ColourSelected.Green)
                {
                    textBlock.Foreground = Brushes.Green;
                }
                else if (CurrentColour == ColourSelected.Blue)
                {
                    textBlock.Foreground = Brushes.Blue;
                }
                else if (CurrentColour == ColourSelected.Indigo)
                {
                    textBlock.Foreground = Brushes.Indigo;
                }
                else
                {
                    textBlock.Foreground = Brushes.Red;
                }

                Canvas.SetLeft(textBlock, e.GetPosition(Drawing).X - 9.61);
                Canvas.SetTop(textBlock, e.GetPosition(Drawing).Y - 9.25);

                int crossIndex = Drawing.Children.Add(textBlock);
                int crossRange = 1;

                orderedChildren.Insert(0, new ChildDrawing() { Index = crossIndex, Length = crossRange });
                childrenCounter++;
            }
            else if (CurrItem == Item.Short)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = "SHORT",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontWeight = FontWeights.Bold,
                    FontSize = 20
                };

                if (CurrentColour == ColourSelected.Green)
                {
                    textBlock.Foreground = Brushes.Green;
                }
                else if (CurrentColour == ColourSelected.Blue)
                {
                    textBlock.Foreground = Brushes.Blue;
                }
                else if (CurrentColour == ColourSelected.Indigo)
                {
                    textBlock.Foreground = Brushes.Indigo;
                }
                else
                {
                    textBlock.Foreground = Brushes.Red;
                }

                Canvas.SetLeft(textBlock, e.GetPosition(Drawing).X - 34);
                Canvas.SetTop(textBlock, e.GetPosition(Drawing).Y - 14);

                int shortageIndex = Drawing.Children.Add(textBlock);
                int shortageRange = 1;

                orderedChildren.Insert(0, new ChildDrawing() { Index = shortageIndex, Length = shortageRange });
                childrenCounter++;
            }

        }


        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(CurrItem == Item.Rectangle)
                {
                    if (e.LeftButton == MouseButtonState.Released || rect == null)
                        return;

                    var pos = e.GetPosition(Drawing);

                    var x = Math.Min(pos.X, startPoint.X);
                    var y = Math.Min(pos.Y, startPoint.Y);

                    var w = Math.Max(pos.X, startPoint.X) - x;
                    var h = Math.Max(pos.Y, startPoint.Y) - y;

                    rect.Width = w;
                    rect.Height = h;

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                }
                else
                {
                    Line line = new Line();
                    if (CurrItem == Item.Brush)
                    {
                        if (CurrentColour == ColourSelected.Blue)
                        {
                            line.Stroke = Brushes.Blue;
                        }
                        else if (CurrentColour == ColourSelected.Green)
                        {
                            line.Stroke = Brushes.Green;
                        }
                        else if (CurrentColour == ColourSelected.Indigo)
                        {
                            line.Stroke = Brushes.Indigo;
                        }
                        else    // all other cases
                        {
                            line.Stroke = Brushes.Red;
                        }
                        line.StrokeThickness = 4;
                    }
                    else if (CurrItem == Item.Pencil)
                    {
                        line.Stroke = SystemColors.WindowFrameBrush;
                    }
                    else if (CurrItem == Item.Highlighter)
                    {
                        line.Stroke = Brushes.Yellow;
                        line.StrokeThickness = 10;
                        line.Opacity = 0.2;
                    }


                    line.X1 = currentPoint.X;
                    line.Y1 = currentPoint.Y;
                    line.X2 = e.GetPosition(Drawing).X;
                    line.Y2 = e.GetPosition(Drawing).Y;

                    currentPoint = e.GetPosition(Drawing);

                    
                    if (lineIndexCounter == 0)
                    {
                        lineIndex = Drawing.Children.Add(line);
                        lineIndexCounter++;
                    }
                    else 
                    {
                        Drawing.Children.Add(line);
                        lineIndexCounter++;
                    }
                }   
            }
        }

        int lineIndexCounter = 0;
        int lineIndex;

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rect = null;

            if (CurrItem == Item.Brush || CurrItem == Item.Pencil || CurrItem == Item.Highlighter)
            {
                if (lineIndexCounter != 0)
                {
                    orderedChildren.Insert(0, new ChildDrawing() { Index = lineIndex, Length = lineIndexCounter });
                    childrenCounter++;
                }
            }
            lineIndexCounter = 0;
        }


        //sets the image Back source to inputed BitmapImage
        public void LoadImage(BitmapImage img) 
        {
            border.Reset();

            double width = img.Width / border.ActualWidth;

            double height = img.Height / border.ActualHeight;
            double divisor;
            if (width <= height)
            {
                divisor = height;
            }
            else
            {
                divisor = width;
            }

            Drawing.Width = img.Width / divisor;
            Drawing.Height = img.Height / divisor;
            Back.Width = img.Width / divisor;
            Back.Height = img.Height / divisor;
            Component.Width = img.Width / divisor;
            Component.Height = img.Height / divisor;
            Back.Source = img;
        }

        public BitmapImage Save()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            int width = ((int)Back.Width) * 2;
            int height = ((int)Back.Height) * 2;
            drawingContext.DrawImage(Back.Source, new Rect(0, 0, (int)Back.Width, (int)Back.Height));//pdf 
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)Drawing.Width, (int)Drawing.Height, 96, 96, PixelFormats.Pbgra32);//creates a new RTB of the specified size
            var intermediate = Drawing;//drawing of the canvas
            intermediate.Measure(new Size((int)Drawing.Width, (int)Drawing.Height));
            intermediate.Arrange(new Rect(new Size((int)Back.Width, (int)Back.Height)));
            renderBitmap.Render(intermediate);
            drawingContext.DrawImage(renderBitmap, new Rect(0, 0, Back.Width, Back.Height));
            drawingContext.Close();
            RenderTargetBitmap renderBmap = new RenderTargetBitmap(width, height, 192, 192, PixelFormats.Pbgra32);
            renderBmap.Render(drawingVisual);
            border.Reset();
            Back.Source = Utility.TargetToBitmap(renderBmap);   //sets the background to new image
            ResetCanvas();
            return Utility.TargetToBitmap(renderBmap);          //returns the saved image
        }


        //used as a check as to not unnecessarily save images
        public Boolean HasDrawings() 
        {
            if (childrenCounter > 0)
            {
                return true;
            }
            else 
            { 
                return false;
            }
        }


        private void ColourRed(object sender, RoutedEventArgs e)
        {
            CurrentColour = ColourSelected.Red;
        }

        private void ColourBlue(object sender, RoutedEventArgs e)
        {
            CurrentColour = ColourSelected.Blue;
        }

        private void ColourGreen(object sender, RoutedEventArgs e)
        {
            CurrentColour = ColourSelected.Green;
        }

        private void ColourIndigo(object sender, RoutedEventArgs e)
        {
            CurrentColour = ColourSelected.Indigo;
        }


        private void Click_Pencil(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Pencil;
            border.CanMove = false;
        }

        private void Click_Brush(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Brush;
            border.CanMove = false;
        }

        private void Click_Text(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Text;
            border.CanMove = false;
        }

        private void Click_Pan(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Pan;
            border.CanMove = true;
        }

        private void Click_Highlight(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Highlighter;
            border.CanMove = false;
        }

        private void Click_Rectangle(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Rectangle;
            border.CanMove = false;
        }

        private void Click_Check(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Check;
            border.CanMove = false;
        }

        private void Click_Cross(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Cross;
            border.CanMove = false;
        }

        private void Click_Short(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Short;
            border.CanMove = false;
        }



        private void Click_RemoveLastEdit(object sender, RoutedEventArgs e)
        {
            if (childrenCounter > 0)
            {
                childrenCounter--;
                int index = orderedChildren[0].Index;
                int count = orderedChildren[0].Length;
                orderedChildren.RemoveAt(0);
                Drawing.Children.RemoveRange(index, count);
            }
        }

        private void Click_RemoveEdits(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
        }

        private void ResetCanvas()
        {
            orderedChildren.Clear();
            childrenCounter = 0;
            lineIndexCounter = 0;

            Drawing.Children.Clear();
        }



        private void Click_Reset(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Pan;
            border.Reset();
            border.CanMove = true;
            percentage.Content = "100 %";
        }

        private void Click_ZoomOut(object sender, RoutedEventArgs e)
        {
            if ((int)ZoomAmount.Value > 40) 
            {
                CurrItem = Item.Pan;
                border.CanMove = true;
                border.ZoomIn(ZoomAmount.Value / 100.0 - 0.5, new Point(border.ActualWidth / 2, border.ActualHeight / 2));
                ZoomAmount.Value -= 50;
                percentage.Content = ((int)ZoomAmount.Value).ToString() + " %";
            }
        }

        private void Click_Zoom(object sender, RoutedEventArgs e)
        {
            CurrItem = Item.Pan;
            border.CanMove = true;
            border.ZoomIn(ZoomAmount.Value / 100.0 + 0.5, new Point(border.ActualWidth / 2, border.ActualHeight / 2));
            ZoomAmount.Value += 50;
            percentage.Content = ((int)ZoomAmount.Value).ToString() + " %";
        }

        private void Slider_move(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrItem = Item.Pan;
            border.CanMove = true;
            border.ZoomIn(ZoomAmount.Value/100.0, new Point(border.ActualWidth/2,border.ActualHeight/2));
            percentage.Content = ((int)ZoomAmount.Value).ToString() + " %";
        }

    }
}
