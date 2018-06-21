using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProIdTool {
    public class ImageAttachment {
        public Attachment Attachment { get;  set; }
        public Row Row { get; set; }

        public ImageAttachment(Attachment attachment, Row row) {
            this.Attachment = attachment;
            this.Row = row;
        }

        public override string ToString() {
            return Attachment != null ? Attachment.GetName() : "<no attachment>";
        }
    }
}
