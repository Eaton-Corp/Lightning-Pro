using System;

using System.Collections.Generic;

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

using System.Windows.Shapes;

namespace LightningPRO

{

    /// <summary>

    /// Interaction logic for InputBoxWindow.xaml

    /// </summary>

    public partial class InputBoxWindow : Window

    {

        public string InputText { get; private set; }

        public InputBoxWindow()

        {

            InitializeComponent();

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)

        {

            InputText = InputTextBox.Text;

            DialogResult = true;

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)

        {

            DialogResult = false;

        }

    }

}
