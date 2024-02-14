using System;
using System.Collections.Generic;
using System.Data;
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

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for AttributesMaintenance.xaml
    /// </summary>
    public partial class AttributesMaintenance : UserControl
    {
        public AttributesMaintenance()
        {
            InitializeComponent();
            FilterFamilies();
        }

        string selectedAttribute;

        private void FilterFamilies()
        {
            DataTable attributes = Utility.SearchLP($"SELECT DISTINCT [FAMILY1], [FAMILY2], [FAMILY3] FROM [TBL_ATTRIBUTES_LIST] ORDER BY [FAMILY1], [FAMILY2], [FAMILY3] ASC ");
            if (attributes.Rows.Count > 0)
            {
                foreach (DataRow row in attributes.Rows)
                {
                    Family1ComboBox.Items.Add(row["FAMILY1"].ToString());
                    Family2ComboBox.Items.Add(row["FAMILY2"].ToString());
                    Family3ComboBox.Items.Add(row["FAMILY3"].ToString());
                }
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            AttributeValueListBox.ItemsSource = null;
            string Family1 = Family1ComboBox.Text;
            string Family2 = Family2ComboBox.Text;
            string Family3 = Family3ComboBox.Text;
            AttributeNameListBox.ItemsSource = Utility.FilterAttributeMaintenance(Family1, Family2, Family3);
        }

        private void AddOption_Click(object sender, RoutedEventArgs e)
        {
            if (AttributeNameTextBox.Text != null)
            {
                selectedAttribute = AttributeNameTextBox.Text;
            }
            if (selectedAttribute != null)
            {
                var result = MessageBox.Show("Adding items under '" + selectedAttribute + "' will affect " +
                    "{NUM} conditions. Would you like to proceed?", "Warning!", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                if (result == MessageBoxResult.OK)
                {
                    var inputBox = new InputBoxWindow();
                    var inputBoxresult = inputBox.ShowDialog();

                    if (inputBoxresult == true)
                    {
                        string enteredOption = inputBox.InputText;
                        string Family1 = Family1ComboBox.Text;
                        string Family2 = Family2ComboBox.Text;
                        string Family3 = Family3ComboBox.Text;
                        string updateQuery = $@"
                            INSERT INTO [TBL_ATTRIBUTE_OPTIONS] ([ATTR_ID], [ATTR_VALUE])
                            SELECT [ATTR_ID], '{enteredOption.Replace("'", "''")}'
                            FROM [TBL_ATTRIBUTES_LIST]
                            WHERE [ATTR_NAME] = '{selectedAttribute.Replace("'", "''")}'";

                        Utility.ExecuteNonQueryLP(updateQuery);
                        // REDIRECT TO CONDITIONS MAINTENANCE with the new added option
                    }
                    else
                    {
                        // The user clicked Cancel or closed the input box
                    }
                }
                else
                {
                    // The user clicked Cancel or closed the message box
                }
            }
            else
            {
                var result = MessageBox.Show("Please select a Drop-Down Item", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }

        }

        private void DisableOption_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (AttributeNameTextBox.Text != null)
            {
                selectedAttribute = AttributeNameTextBox.Text;
            }
            if (button != null)
            {
                var selectedOption = button.CommandParameter as string;
                string Family1 = Family1ComboBox.Text;
                string Family2 = Family2ComboBox.Text;
                string Family3 = Family3ComboBox.Text;

                DataTable attributes = Utility.SearchLP($@"
            SELECT [Disabled] FROM [TBL_ATTRIBUTE_OPTIONS]
            INNER JOIN [TBL_ATTRIBUTES_LIST] ON [TBL_ATTRIBUTES_LIST].[ATTR_ID] = [TBL_ATTRIBUTE_OPTIONS].[ATTR_ID]
            WHERE [FAMILY1] = '{Family1.Replace("'", "''")}' AND [FAMILY2] = '{Family2.Replace("'", "''")}' AND [FAMILY3] = '{Family3.Replace("'", "''")}'
            AND [ATTR_NAME] = '{selectedAttribute.Replace("'", "''")}' AND [ATTR_VALUE] = '{selectedOption.Replace("'", "''")}'");

                if (attributes.Rows.Count > 0)
                {
                    bool isDisabled = Convert.ToBoolean(attributes.Rows[0]["Disabled"]);
                    if (!isDisabled)
                    {
                        var result = MessageBox.Show($"Disabling the '{selectedOption}' option under '{selectedAttribute}' will affect {{NUM}} conditions. Would you like to proceed?", "Warning!", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                        if (result == MessageBoxResult.OK)
                        {
                            // The user clicked OK, proceed with disabling the option
                            string updateQuery = $@"
                        UPDATE [TBL_ATTRIBUTE_OPTIONS]
                        SET [Disabled] = true
                        WHERE EXISTS (
                            SELECT 1 FROM [TBL_ATTRIBUTES_LIST]
                            WHERE [TBL_ATTRIBUTES_LIST].[ATTR_ID] = [TBL_ATTRIBUTE_OPTIONS].[ATTR_ID]
                            AND [FAMILY1] = '{Family1.Replace("'", "''")}' AND [FAMILY2] = '{Family2.Replace("'", "''")}' AND [FAMILY3] = '{Family3.Replace("'", "''")}'
                            AND [ATTR_NAME] = '{selectedAttribute.Replace("'", "''")}' AND [ATTR_VALUE] = '{selectedOption.Replace("'", "''")}'
                        )";
                            Utility.ExecuteNonQueryLP(updateQuery);
                        }
                        else
                        {
                            // The user clicked Cancel or closed the message box
                        }
                    }
                    else
                    {
                        MessageBox.Show($"The '{selectedOption}' option under '{selectedAttribute}' is already disabled.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // No record found, handle accordingly
                }
            }
        }


        private void EnableOption_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (AttributeNameTextBox.Text != null)
            {
                selectedAttribute = AttributeNameTextBox.Text;
            }
            if (button != null)
            {
                var selectedOption = button.CommandParameter as string;
                string Family1 = Family1ComboBox.Text;
                string Family2 = Family2ComboBox.Text;
                string Family3 = Family3ComboBox.Text;

                DataTable attributes = Utility.SearchLP($@"
            SELECT [Disabled] FROM [TBL_ATTRIBUTE_OPTIONS]
            INNER JOIN [TBL_ATTRIBUTES_LIST] ON [TBL_ATTRIBUTES_LIST].[ATTR_ID] = [TBL_ATTRIBUTE_OPTIONS].[ATTR_ID]
            WHERE [FAMILY1] = '{Family1}' AND [FAMILY2] = '{Family2}' AND [FAMILY3] = '{Family3}'
            AND [ATTR_NAME] = '{selectedAttribute}' AND [ATTR_VALUE] = '{selectedOption}'");

                if (attributes.Rows.Count > 0)
                {
                    bool isDisabled = Convert.ToBoolean(attributes.Rows[0]["Disabled"]);
                    if (isDisabled)
                    {
                        var result = MessageBox.Show($"Are you sure you want to enable the '{selectedOption}' option under '{selectedAttribute}'?", "Warning!", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                        if (result == MessageBoxResult.OK)
                        {
                            // The user clicked OK, proceed with disabling the option
                            string updateQuery = $@"
                        UPDATE [TBL_ATTRIBUTE_OPTIONS]
                        SET [Disabled] = false
                        WHERE EXISTS (
                            SELECT 1 FROM [TBL_ATTRIBUTES_LIST]
                            WHERE [TBL_ATTRIBUTES_LIST].[ATTR_ID] = [TBL_ATTRIBUTE_OPTIONS].[ATTR_ID]
                            AND [FAMILY1] = '{Family1.Replace("'", "''")}' AND [FAMILY2] = '{Family2.Replace("'", "''")}' AND [FAMILY3] = '{Family3.Replace("'", "''")}'
                            AND [ATTR_NAME] = '{selectedAttribute.Replace("'", "''")}' AND [ATTR_VALUE] = '{selectedOption.Replace("'", "''")}'
                        )";
                            Utility.ExecuteNonQueryLP(updateQuery);

                            // Optionally refresh the UI or perform other actions to reflect the change
                        }
                        else
                        {
                            // CLOSE MSG
                        }
                    }
                    else
                    {
                        MessageBox.Show($"The '{selectedOption}' option under '{selectedAttribute}' is already enabled.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // No record found
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //var parentWindow = Window.GetWindow(this) as ConfiguratorPRLC;
            //parentWindow?.HideAttributesMaintenance();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            AttributeValueListBox.ItemsSource = null;
            Family1ComboBox.Text = null;
            Family2ComboBox.Text = null;
            Family3ComboBox.Text = null;
            string AttributeNameSearch = AttributeNameTextBox.Text;
            AttributeNameListBox.ItemsSource = Utility.SearchAttributeMaintenance(AttributeNameSearch);
        }

        private void AttributeNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AttributeNameListBox.SelectedItem is string selected)
            {
                HandleAttributeSelected(selected);
                if (AttributeNameTextBox.Text != null)
                {
                    selectedAttribute = AttributeNameTextBox.Text;
                }
                AttributeNameListBox.SelectedIndex = -1;
            }
        }

        public void HandleAttributeSelected(string selectedAttribute)
        {
            string[] attributeOptions = Utility.SearchAttributeOptionsMaintenance(selectedAttribute);

            AttributeValueListBox.ItemsSource = attributeOptions;

            DataTable attributes = Utility.SearchLP($"SELECT TOP 1 [FAMILY1], [FAMILY2], [FAMILY3] FROM [TBL_ATTRIBUTES_LIST] WHERE [ATTR_NAME] = '{selectedAttribute.Replace("'", "''")}'"); // Added SQL injection protection.
            if (attributes.Rows.Count > 0)
            {
                DataRow row = attributes.Rows[0];
                AttributeNameTextBox.Text = selectedAttribute;
                Family1ComboBox.Text = row["FAMILY1"].ToString();
                Family2ComboBox.Text = row["FAMILY2"].ToString();
                Family3ComboBox.Text = row["FAMILY3"].ToString();
            }
        }

    }
}