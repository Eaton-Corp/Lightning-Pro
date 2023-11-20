using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.DatabaseServices;

namespace LightningPRO
{
    public class EatonDataGrid : DataGrid
    {
        public EatonDataGrid()
        {
            // Constructor code, if any
        }

        private bool isFirstButtonColumn = true;

        public void AddButtonColumn(string header, object buttonContent, RoutedEventHandler clickEventHandler,
                                Color backgroundColor, double buttonWidth, double buttonHeight)
        {
            var column = new DataGridTemplateColumn();
            column.Header = header;

            // If it's the first button column, set the width to "*" and align content to right
            if (isFirstButtonColumn)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                isFirstButtonColumn = false; // Update flag so that this is only applied to the first column
            }
            else
            {
                column.Width = buttonWidth + 10; // Fixed width for subsequent button columns
            }

            column.CellTemplate = new DataTemplate { VisualTree = CreateButtonFactory(buttonContent, clickEventHandler, backgroundColor, buttonWidth, buttonHeight, isFirstButtonColumn) };
            Columns.Add(column);
        }

        private FrameworkElementFactory CreateButtonFactory(object content, RoutedEventHandler clickEventHandler,
                                                           Color backgroundColor, double width, double height, bool alignRight)
        {
            FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.BackgroundProperty, new SolidColorBrush(backgroundColor));
            buttonFactory.SetValue(Button.WidthProperty, width);
            buttonFactory.SetValue(Button.HeightProperty, height);
            buttonFactory.AddHandler(Button.ClickEvent, clickEventHandler);

            if (alignRight)
            {
                buttonFactory.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            }

            if (content is string text)
            {
                buttonFactory.SetValue(Button.ContentProperty, text);
            }
            else if (content is string imagePath)
            {
                var image = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                    Stretch = Stretch.Fill
                };
                buttonFactory.SetValue(Button.ContentProperty, image);
            }

            buttonFactory.SetValue(Button.StyleProperty, Application.Current.FindResource("EatonDataGridButtonStyle") as Style);
            return buttonFactory;
        }
    }

}
