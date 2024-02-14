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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace LightningPRO.Views
{
    public partial class Analytics : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] StageLabels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public string[] DateLabels { get; set; } // Adding this for the new chart

        public Analytics()
        {
            InitializeComponent();
            LoadKeyMetrics();
            LoadTrackingGraph();
            LoadDateBasedGraph(DateTime.Now.AddDays(-30), DateTime.Now); // load the last 30 days initially
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

        private void LoadDateBasedGraph(DateTime startDate, DateTime endDate)
        {
            using (var db = new LightningProDatabaseContext())
            {
                endDate = endDate.Date.AddDays(1).AddTicks(-1);  // include the end of the selected end day

                // Group by date and count for each date
                var enteredCounts = db.Product
                    .Where(p => p.EnteredDate >= startDate && p.EnteredDate <= endDate)
                    .GroupBy(p => p.EnteredDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new ObservableValue(g.Count())).ToList();

                var releasedCounts = db.Product
                    .Where(p => p.ReleaseDate >= startDate && p.ReleaseDate <= endDate)
                    .GroupBy(p => p.ReleaseDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new ObservableValue(g.Count())).ToList();

                var commitCounts = db.Product
                    .Where(p => p.CommitDate >= startDate && p.StartDate <= endDate)
                    .GroupBy(p => p.CommitDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new ObservableValue(g.Count())).ToList();

                // Set up the SeriesCollection
                SeriesCollection dateBasedSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Entered",
                        Values = new ChartValues<ObservableValue>(enteredCounts)
                    },
                    new LineSeries
                    {
                        Title = "Released",
                        Values = new ChartValues<ObservableValue>(releasedCounts)
                    },
                    new LineSeries
                    {
                        Title = "Commit",
                        Values = new ChartValues<ObservableValue>(commitCounts)
                    }
                };

                DateLabels = Enumerable.Range(0, 31).Select(d => DateTime.Now.AddDays(-d).ToShortDateString()).Reverse().ToArray();

                DateBasedChart.Series = dateBasedSeries;
                DateBasedChart.DataContext = this;
            }
        }

        private void OnLoadDataButtonClick(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                LoadDateBasedGraph(StartDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value);
            }
            else
            {
                MessageBox.Show("Please select a valid date range.");
            }
        }


    }

}
