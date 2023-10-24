using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using LightningPRO.Models;
using System;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;

namespace LightningPRO.Views
{
    public partial class Analytics : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] StageLabels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public Analytics()
        {
            InitializeComponent();
            LoadKeyMetrics();
            LoadTrackingGraph();
        }

        private void LoadKeyMetrics()
        {
            using (var db = new LightningProDatabaseContext())
            {
                // Fetch the total number of orders from the database
                var totalOrders = db.Product.Count();
                txtTotalOrders.Text = totalOrders.ToString();

                // Fetch the number of orders from the last month
                var lastMonthOrders = db.Product.Count(order => order.EnteredDate > DateTime.Now.AddMonths(-1));
                txtLastMonthOrders.Text = lastMonthOrders.ToString();

                // ... You can continue adding metrics as needed
            }
        }

        private void LoadTrackingGraph()
        {
            using (var db = new LightningProDatabaseContext())
            {
                // Fetch the counts for each tracking stage using LINQ
                var specialistCount = db.Product.Count(p => p.Tracking == "Specialist");
                var mICompleteCount = db.Product.Count(p => p.Tracking == "MIComplete");
                var productionCount = db.Product.Count(p => p.Tracking == "Production");
                var shippingCount = db.Product.Count(p => p.Tracking == "Shipping");


                // Create your SeriesCollection and bind it to the graph
                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Tracking",
                        Values = new ChartValues<double> { specialistCount, mICompleteCount, productionCount, shippingCount }
                    }
                };

                StageLabels = new[] { "Specialist", "MIComplete", "Production", "Shipping" };
                Formatter = value => value.ToString("N");

                TrackingChart.Series = SeriesCollection;
                TrackingChart.DataContext = this;
            }
        }
    }
}
