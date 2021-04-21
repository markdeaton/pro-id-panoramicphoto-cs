using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;
using Pano.Net.View;
using Pano.Net.ViewModel;
using System.IO;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Internal.Framework.Controls;

namespace ProIdTool {
    internal class PanoIDTool : MapTool {
        public PanoIDTool() {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen;

        }

        protected override Task OnToolActivateAsync(bool active) {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry) {
            // Get nearest point if within tolerance; or report no feature found nearby click location
            MapView mv = MapView.Active;
            try {
                bool identifyResult = await QueuedTask.Run(() => {
                    //List<Attachment> lstAllAttachments = new List<Attachment>();
                    List<ImageAttachment> lstImageAttachments = new List<ImageAttachment>();

                    Dictionary<BasicFeatureLayer, List<long>> feats = null;
                    try {
                        feats = mv.GetFeatures(geometry);
                    } catch (Exception e) {
                        MessageBox.Show("Couldn't get features at that location: " + e.Message);
                        return false;
                    }

                    if (feats.Count <= 0) {
                        MessageBox.Show("No features are there.");
                        return true;
                    }

                    foreach (BasicFeatureLayer bfl in feats.Keys) {
                        List<long> featIds = new List<long>();
                        if (feats.TryGetValue(bfl, out featIds)) {
                            // Get OID field name
                            using (Table tbl = bfl.GetTable()) {
                                string sOIDField = tbl.GetDefinition().GetObjectIDField();
                                string sFeatIds = String.Join(",", featIds);

                                QueryFilter qf = new QueryFilter();
                                qf.WhereClause = sOIDField + " IN (" + sFeatIds + ")";

                                using (RowCursor cur = bfl.Search(qf)) {
                                    while (cur.MoveNext()) {
                                        Row row = cur.Current;
                                        IReadOnlyList<Attachment> attachments = row.GetAttachments(null, true);
                                        if (attachments.Count > 0)
                                            // Only add jpegs to selection list
                                            foreach (Attachment attachment in attachments) {
                                                if (attachment.GetContentType() == System.Net.Mime.MediaTypeNames.Image.Jpeg)
                                                    lstImageAttachments.Add(new ImageAttachment(attachment, row));
                                            } else row.Dispose();
                                    }
                                }
                            }
                        }
                    }
                    ImageAttachment selectedAttachment = null;
                    // Open the attachment selection dialog
                    if (lstImageAttachments.Count <= 0) { // No results
                        MessageBox.Show("No images are attached to that feature.");
                    } else if (lstImageAttachments.Count == 1) { // Automatically open this result
                        selectedAttachment = lstImageAttachments[0];
                    } else { // More than one result
                        ImageAttachmentListDialog dlg = new ImageAttachmentListDialog(lstImageAttachments);
                        //dlg.Owner = FrameworkApplication.Current.MainWindow;
                        bool? result = dlg.ShowDialog();
                        if (result ?? false) selectedAttachment = dlg.SelectedItem;
                    }

                    if (selectedAttachment != null) {
                        // Open the panorama viewer
                        ProgressorSource ps = new ProgressorSource("Loading...", false);
                        QueuedTask.Run(() => {
                            IReadOnlyList<Attachment> selectedSingleAttachmentList = selectedAttachment.Row.GetAttachments(
                                    new List<long> { selectedAttachment.Attachment.GetAttachmentID() }, false);
                            MemoryStream memStream = selectedSingleAttachmentList[0].GetData();
                            FrameworkApplication.Current.Dispatcher.Invoke(() => {
                                MainWindow mw = new MainWindow();
                                mw.Show();
                                mw.Activate();
                                mw.Topmost = true;  // important

                                MainViewModel mvm = (MainViewModel)mw.DataContext;
                                mvm.Open(memStream);
                            });
                        }, ps.Progressor);
                    }

                    return true;
                });
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            return true;
        }
    }
}
