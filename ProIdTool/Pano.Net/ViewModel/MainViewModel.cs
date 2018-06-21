using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Pano.Net.ViewModel {
    /// <summary>
    /// Main ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        // Public properties
        #region public_properties

        /// <summary>
        /// Panorama
        /// </summary>
        public BitmapImage Image { get; private set; }

        /// <summary>
        /// Is fullscreen mode on
        /// </summary>
        public bool IsFullscreen { get; private set; }

        /// <summary>
        /// Is the model loading
        /// </summary>
        public bool IsLoading { get; private set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Image = null; RaisePropertyChanged("Image");
            IsFullscreen = false; RaisePropertyChanged("IsFullscreen");
            IsLoading = false; RaisePropertyChanged("IsLoading");
        }

        // Private methods
        #region private_methods

        // Open image by file name
        public async Task Open(string path) {
            Image = null; RaisePropertyChanged("Image");
            IsLoading = true; RaisePropertyChanged("IsLoading");

            await Task.Factory.StartNew(() => {
                Image = new BitmapImage();
                Image.BeginInit();
                Image.CacheOption = BitmapCacheOption.OnLoad;
                Image.UriSource = new Uri(path);
                Image.EndInit();
                Image.Freeze();
            });

            if (Math.Abs(Image.Width / Image.Height - 2) > 0.001)
                WarningMessage("Warning", "The opened image is not equirectangular (2:1)! Rendering may be improper.");

            IsLoading = false; RaisePropertyChanged("IsLoading");
            RaisePropertyChanged("Image");
        }
        
        // Open image by input stream
        public async Task Open(MemoryStream memStream) {
            Image = null; RaisePropertyChanged("Image");
            IsLoading = true; RaisePropertyChanged("IsLoading");

            await Task.Factory.StartNew(() => {
                Image = new BitmapImage();
                Image.BeginInit();
                Image.StreamSource = memStream;
                Image.CacheOption = BitmapCacheOption.OnLoad;
                Image.EndInit();
                Image.Freeze();
            });

            if (Math.Abs(Image.Width / Image.Height - 2) > 0.001)
                WarningMessage("Warning", "This image isn't a panorama. It may not display as you expect it to.");

            IsLoading = false; RaisePropertyChanged("IsLoading");
            RaisePropertyChanged("Image");
        }

        // Toggle fullscreen
        private void FullScreen()
        {
            IsFullscreen = !IsFullscreen;
            RaisePropertyChanged("IsFullscreen");
        }

        //Display controls
        private void Controls()
        {
            InfoMessage("Controls", "Click and drag the mouse to move camera.\r\nScroll to zoom.");
        }


        // Helper function to display an information
        private void InfoMessage(string caption, string text)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Helper function to display a warning
        private void WarningMessage(string caption, string text)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion
    }
}
