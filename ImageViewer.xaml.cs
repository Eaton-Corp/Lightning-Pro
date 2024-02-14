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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl
    {

        /*
         * HOW TO USE DIRECTIONS (CAN BE USED IN 2 WAYS)
         * 
         * If you have functions/operations that you would like to use when the back/forward buttons are clicked:
         * 
         *      Upon initialization of the page that the usercontrol is on subscribe your functions to the appropriate eventhandlers as shown
         *      
         *      ImageViewerControl.BackButtonClick += ImageViewerControl_BackButtonClick;
         *      
         *      private void ImageViewerControl_BackButtonClick(object sender, EventArgs e)
                {
                    YourBackFunction();
                }
 
         *      ImageViewerControl.ForwardButtonClick += ImageViewerControl_ForwardButtonClick;
         *      
         *      private void ImageViewerControl_ForwardButtonClick(object sender, EventArgs e)
                {
                    YourForwardFunction();
                }
        *
        * To load all of your images use the following public function:
        * 
        *       ImageViewerControl.LoadImages(List<BitmapImage> images);
        *          
        */

        public ImageViewer()
        {
            InitializeComponent();
            border.CanMove = false;
        }

        public event EventHandler BackButtonClick;
        public event EventHandler ForwardButtonClick;

        bool HasPreviewLoaded = false;

        private List<BitmapImage> _images = new List<BitmapImage>();
        private int _currentIndex = 0;

        public void UpdatePageNumberLabel(string Label)
        {
            pageNumberLabel.Content = Label;
        }

        public void LoadImages(List<BitmapImage> images, int page)
        {
            _images = images;
            _currentIndex = page;
            if (_images.Count > 0)
                LoadImage(_images[_currentIndex]);
            UpdatePageNumberLabel($"Page {_currentIndex + 1} of {_images.Count}");
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (BackButtonClick != null && HasPreviewLoaded)
            {
                BackButtonClick?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (_currentIndex > 0 && HasPreviewLoaded)
                {
                    _currentIndex--;
                    LoadImage(_images[_currentIndex]);
                    UpdatePageNumberLabel($"Page {_currentIndex + 1} of {_images.Count}");
                }
            }
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (ForwardButtonClick != null && HasPreviewLoaded)
            {
                ForwardButtonClick?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (_currentIndex < _images.Count - 1 && HasPreviewLoaded)
                {
                    _currentIndex++;
                    LoadImage(_images[_currentIndex]);
                    UpdatePageNumberLabel($"Page {_currentIndex + 1} of {_images.Count}");
                }
            }
        }

        //sets the image Back source to inputed BitmapImage
        public void LoadImage(BitmapImage img)
        {
            //border.Reset();       // commented out to NOT reset border on every page load 
            Back.Source = img;
            border.CanMove = true;
            HasPreviewLoaded = true;
        }

        private void Click_Reset(object sender, RoutedEventArgs e)
        {
            if (HasPreviewLoaded)
            {
                border.Reset();
                percentage.Content = "100 %";
            }
        }

        private void Click_ZoomOut(object sender, RoutedEventArgs e)
        {
            if ((int)ZoomAmount.Value > 40 && HasPreviewLoaded)
            {
                border.ZoomIn(ZoomAmount.Value / 100.0 - 0.5, new Point(border.ActualWidth / 2, border.ActualHeight / 2));
                ZoomAmount.Value -= 50;
                percentage.Content = ((int)ZoomAmount.Value).ToString() + " %";
            }
        }

        private void Click_Zoom(object sender, RoutedEventArgs e)
        {
            if (HasPreviewLoaded)
            {
                border.ZoomIn(ZoomAmount.Value / 100.0 + 0.5, new Point(border.ActualWidth / 2, border.ActualHeight / 2));
                ZoomAmount.Value += 50;
                percentage.Content = ((int)ZoomAmount.Value).ToString() + " %";
            }
        }

        private void Slider_move(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HasPreviewLoaded)
            {
                border.ZoomIn(ZoomAmount.Value / 100.0, new Point(border.ActualWidth / 2, border.ActualHeight / 2));
                percentage.Content = ((int)ZoomAmount.Value).ToString() + " %";
            }
        }


    }
}