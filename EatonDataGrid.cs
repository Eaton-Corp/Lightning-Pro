using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.DatabaseServices;
using System.Windows.Data;
using System.Collections;
using Image = System.Windows.Controls.Image;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Globalization;

namespace LightningPRO
{
    public partial class EatonDataGrid : DataGrid
    {
        private bool isFirstColumnAdded = false;


        /// <summary>
        /// Adds a button column to the DataGrid with the specified physical properties and handler. 
        /// </summary>
        /// <param name="header">The header text for the column in the datagrid, please fill with empty string if unneccessary</param>
        /// <param name="buttonContent">The content of the button, must be a string.</param>
        /// <param name="clickEventHandler">The event handler for the button click event.</param>
        /// <param name="backgroundColor">The background color of the button.</param>
        /// <param name="buttonWidth">The width of the button.</param>
        /// <param name="buttonHeight">The height of the button</param>
        public void AddButtonColumn(string header, object buttonContent, RoutedEventHandler clickEventHandler,
                        Color backgroundColor, double buttonWidth, double buttonHeight)
        {
            var column = new DataGridTemplateColumn();
            // Set header only if it's not null or empty
            if (!string.IsNullOrEmpty(header))
            {
                column.Header = header;
            }

            if (!isFirstColumnAdded)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                isFirstColumnAdded = true; // Update flag so that this is only applied to the first column
            }
            else
            {
                column.Width = buttonWidth + 18; // Fixed width for subsequent button columns
            }

            column.CellTemplate = new DataTemplate { VisualTree = CreateButtonFactory(buttonContent, clickEventHandler, backgroundColor, buttonWidth, buttonHeight, true) };
            Columns.Add(column);
        }


        /// <summary>
        /// Adds a button column to the DataGrid with the specified properties. 
        /// </summary>
        /// <param name="content">The content of the button, either a string or an image path.</param>
        /// <param name="clickEventHandler">The event handler for the button click event.</param>
        /// <param name="backgroundColor">The background color of the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="alignRight">This is a boolean to align a specific button to the right, this is ideal for CRUD buttons</param>
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

            if (content is string imagePath && imagePath.EndsWith(".png"))
            {
                Uri imageUri = new Uri($"pack://application:,,,/LightningPRO;component/{imagePath}", UriKind.Absolute);
                BitmapImage bitmapImage = new BitmapImage(imageUri);

                // Create an Image control and set its source
                FrameworkElementFactory imageControlFactory = new FrameworkElementFactory(typeof(Image));
                imageControlFactory.SetValue(Image.SourceProperty, bitmapImage);
                imageControlFactory.SetValue(Image.StretchProperty, Stretch.Fill);

                // Set the Image control as the content of the button
                buttonFactory.SetValue(Button.ContentProperty, imageControlFactory);
            }
            else if (content is string text)
            {
                buttonFactory.SetValue(Button.ContentProperty, text);
            }

            buttonFactory.SetValue(Button.StyleProperty, Application.Current.FindResource("EatonDataGridButtonStyle") as Style);

            return buttonFactory;
        }


        /// <summary>
        /// Adds a dropdown button column to the DataGrid with the specified header, options, button width, and button height.
        /// </summary>
        /// <param name="header">The header text for the column.</param>
        /// <param name="options">A dictionary containing the option keys and their corresponding RoutedEventHandlers.</param>
        /// <param name="buttonWidth">The width of the dropdown button in the column.</param>
        /// <param name="buttonHeight">The height of the dropdown button in the column.</param>
        public void AddDropdownButtonColumn(string header, IDictionary<string, RoutedEventHandler> options, double buttonWidth, double buttonHeight)
        {
            var column = new DataGridTemplateColumn();
            column.Header = "Other";
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);

            var template = new DataTemplate();
            var comboBoxFactory = new FrameworkElementFactory(typeof(ComboBox));

            comboBoxFactory.SetValue(ComboBox.ItemsSourceProperty, options.Keys);
            comboBoxFactory.SetValue(ComboBox.TemplateProperty, Application.Current.FindResource("DropdownComboBoxTemplate") as ControlTemplate);
            comboBoxFactory.SetValue(ComboBox.WidthProperty, buttonWidth);
            comboBoxFactory.SetValue(ComboBox.HeightProperty, buttonHeight);
            comboBoxFactory.AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler((sender, e) =>
            {
                ComboBox comboBox = sender as ComboBox;
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is string optionKey && options.TryGetValue(optionKey, out var handler))
                {
                    handler(sender, e);
                    // Reset the ComboBox selection
                    comboBox.SelectedIndex = -1;
                }
            }));

            template.VisualTree = comboBoxFactory;
            column.CellTemplate = template;

            Columns.Add(column);
        }


        /// <summary>
        /// Removes a button column from the DataGrid with the specified header.
        /// </summary>
        /// <param name="header">The header text of the column to remove.</param>
        public void RemoveButtonColumn(string header)
        {
            var columnToRemove = Columns.FirstOrDefault(c => ((c.Header as string) ?? "") == header);
            if (columnToRemove != null)
            {
                Columns.Remove(columnToRemove);
            }
        }

        /// <summary>
        /// Adds a CheckBox column to the DataGrid with the specified header, binding path, and column width.
        /// </summary>
        /// <param name="header">The header text for the column.</param>
        /// <param name="bindingPath">The binding path for the CheckBox's IsChecked property.</param>
        /// <param name="columnWidth">The width of the column.</param>
        public void AddCheckBoxColumn(string header, string bindingPath, double columnWidth)
        {
            var column = new DataGridTemplateColumn
            {
                Header = header
            };

            if (!isFirstColumnAdded)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                isFirstColumnAdded = true;
            }
            else
            {
                column.Width = columnWidth;
            }

            var template = new DataTemplate();
            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));

            // Bind the IsChecked property to the specified binding path
            var binding = new Binding(bindingPath)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, binding);
            checkBoxFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center); // Align to the center

            template.VisualTree = checkBoxFactory;
            column.CellTemplate = template;

            Columns.Add(column);
        }


        /// <summary>
        /// Adds a ComboBox column to the DataGrid with the specified header, binding path, items source,
        /// display member path, selected value path, column width, and selection changed event handler.
        /// </summary>
        /// <param name="header">The header text for the column.</param>
        /// <param name="bindingPath">The binding path for the ComboBox's SelectedItem property.</param>
        /// <param name="itemsSource">The items source for the ComboBox.</param>
        /// <param name="displayMemberPath">The display member path for the ComboBox.</param>
        /// <param name="selectedValuePath">The selected value path for the ComboBox.</param>
        /// <param name="columnWidth">The width of the column.</param>
        /// <param name="selectionChangedHandler">The event handler for the ComboBox's SelectionChanged event.</param>
        public void AddComboBoxColumn(string header, string bindingPath, IEnumerable itemsSource,
                              string displayMemberPath, string selectedValuePath,
                              double columnWidth, SelectionChangedEventHandler selectionChangedHandler)
        {
            var column = new DataGridTemplateColumn
            {
                Header = header
            };

            if (!isFirstColumnAdded)
            {
                //column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                column.Width = new DataGridLength(75);
                isFirstColumnAdded = true;
            }
            else
            {
                column.Width = columnWidth;
            }

            var template = new DataTemplate();
            var comboBoxFactory = new FrameworkElementFactory(typeof(ComboBox));

            // Create a list of strings from "1" to "10"
            var numberStrings = Enumerable.Range(1, 1000).Select(i => i.ToString()).ToList();
            //var numberStrings = itemsSource.OfType<string>().ToList();


            // Set the list as the ItemsSource for the ComboBox
            comboBoxFactory.SetValue(ComboBox.ItemsSourceProperty, numberStrings);

            // Bind the SelectedItem to the Quantity property, using a converter
            var binding = new Binding(bindingPath)
            {
                Converter = new StringToIntConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            comboBoxFactory.SetBinding(ComboBox.SelectedItemProperty, binding);
            comboBoxFactory.SetValue(ComboBox.WidthProperty, columnWidth); // Set the specified width
            comboBoxFactory.SetValue(ComboBox.ForegroundProperty, Brushes.Gray); // Set text color to black
            comboBoxFactory.SetValue(ComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Right); // Align to the right


            // Add the event handler for selection changed
            comboBoxFactory.AddHandler(ComboBox.SelectionChangedEvent, selectionChangedHandler);

            template.VisualTree = comboBoxFactory;
            column.CellTemplate = template;

            Columns.Add(column);
        }
    }

    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && int.TryParse(strValue, out int intValue))
            {
                return intValue;
            }
            return 0; // Or handle this case as you see fit
        }
    }


}
