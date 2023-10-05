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
    /// Interaction logic for NotesWindow.xaml
    /// </summary>
    public partial class NotesWindow : Window
    {
        readonly string ProductTable;
        readonly System.Windows.Controls.Button btnNotes;

        public NotesWindow(string GOI, string productTBL, System.Windows.Controls.Button NotesButton)
        {
            InitializeComponent();
            
            GOItem.Content = GOI;
            ProductTable = productTBL;
            btnNotes = NotesButton;
            Note.Text = Utility.GetNotes(GOItem.Content.ToString(), ProductTable);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Utility.UpdateNotes(GOItem.Content.ToString(), ProductTable, Note.Text);
            Task<int> ShowProgressBar = TurnOnStatus();
            int result = await ShowProgressBar;
            if (result == 1) Status.Content = "";
            if (string.IsNullOrEmpty(Note.Text)) btnNotes.Background = System.Windows.Media.Brushes.LightGray; else btnNotes.Background = System.Windows.Media.Brushes.Blue;
        }

        private async Task<int> TurnOnStatus()
        {
            Status.Content = "Note Saved";
            await Task.Delay(800);
            return 1;
        }
    }
}
