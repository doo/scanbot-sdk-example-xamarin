using System;
namespace ReadyToUseUIDemo.model
{
    public enum ListItemCode
    {
        ScanDocument,
        ImportImage,
        ViewImages,

        ScannerMRZ,
        ScannerEHIC,
        
        ScannerBarcode,
        ScannerBatchBarcode,
        ScannerImportBarcode,
        ScannerImportImagesFromBarcode,


        WorkflowQR,
        WorkflowMRZImage,
        WorkflowMRZFrontBack,
        WorkflowMC,
        WorkflowSEPA,

        GenericDocumentRecognizer
    }

    public class ListItem
    {
        public string Title { get; set; }

        public ListItemCode Code { get; set; }
    }
}
