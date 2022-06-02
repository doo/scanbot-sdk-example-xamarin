using System;
using System.Collections.Generic;

namespace ReadyToUseUIDemo.model
{
    public class BarcodeDetectors
    {
        public static BarcodeDetectors Instance = new BarcodeDetectors();

        public string Title { get => "BARCODE DETECTORS"; }

        public List<ListItem> Items = new List<ListItem>
        {
            new ListItem { Title = "Scan Barcodes", Code = ListItemCode.ScannerBarcode },
            new ListItem { Title = "Scan Batch Barcodes", Code = ListItemCode.ScannerBatchBarcode },
            new ListItem { Title = "Import and Detect Barcodes", Code = ListItemCode.ScannerImportBarcode }
        };
    }
}
