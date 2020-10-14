using System;
namespace ReadyToUseUIDemo.model
{
    public enum ListItemCode
    {
        ScanDocument,
        ImportImage,
        ViewImages,

        ScannerText,

        ScannerMRZ,
        ScannerEHIC,
        ScannerNFC,

        ScannerBarcode,
        ScannerBatchBarcode,
        ScannerImportBarcode,

        WorkflowQR,
        WorkflowMRZImage,
        WorkflowMRZFrontBack,
        WorkflowDC,
        WorkflowSEPA,
        
    }

    public class ListItem
    {
        public string Title { get; set; }

        public ListItemCode Code { get; set; }
    }
}
