using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Controls;
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

namespace ProIdTool {
    /// <summary>
    /// Interaction logic for ImageAttachmentListDialog.xaml
    /// </summary>
    public partial class ImageAttachmentListDialog : ProWindow {
        public ImageAttachmentListDialog(List<ImageAttachment> aryImageAttachments) {
            InitializeComponent();

            lstImageAttachments.ItemsSource = aryImageAttachments;
            lstImageAttachments.SelectionChanged += LstImageAttachments_SelectionChanged;

        }

        private void LstImageAttachments_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SelectedItem = (ImageAttachment)e.AddedItems[0];
            this.DialogResult = true;
        }

        public ImageAttachment SelectedItem { get; private set; }
    }
}
