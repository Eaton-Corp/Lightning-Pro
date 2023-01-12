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
using System.Windows.Shapes;
using static PRL123_Final.Insert;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for AMOCheckList.xaml
    /// </summary>
    public partial class AMOCheckList : Window
    {
        public class PartsList
        {
            public string PartName { get; set; }
            public int Quantity { get; set; }
            public info Status { get; set; }
        }
        
        public AMOCheckList(List<part> partsList, List<info> Information )
        {
            InitializeComponent();
            List<PartsList> listOfParts = new List<PartsList>();

            for(int i = 0; i < partsList.Count; i++)
            {
                listOfParts.Add(new PartsList() { PartName = partsList[i].Get_partName(), Quantity = partsList[i].Get_Amount(), Status = Information[i] });
            }

            dg.ItemsSource = listOfParts;
        }

    }
}
