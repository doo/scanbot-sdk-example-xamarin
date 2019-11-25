using System;
namespace ReadyToUseUIDemo.model
{
    public enum ListItemCode
    {
        ScanDocument,
        ImportImage,
        ViewImages,
        ScanQRBar,
        ScanMRZ,
        ScanMRZImage,
        ScanMRZFrontBack,
        ScanQRBarDocument,
        ScanDC,
        ScanSEPA,
        ScanEHIC,
    }

    public class ListItem
    {
        public string Title { get; set; }

        public ListItemCode Code { get; set; }
    }
}
