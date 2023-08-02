using System.Collections.Generic;

namespace ReadyToUseUIDemo.model
{
    public class DataDetectors
    {
        public static DataDetectors Instance = new DataDetectors();

        public string Title { get => "DATA DETECTORS"; }

        public List<ListItem> Items = new List<ListItem>
        {
            new ListItem { Title = "Scan MRZ",                      Code = ListItemCode.ScannerMRZ           },
            new ListItem { Title = "Scan Health Insurance card",    Code = ListItemCode.ScannerEHIC          },
            new ListItem { Title = "Generic Document Recognizer",   Code = ListItemCode.GenericDocumentRecognizer },
            new ListItem { Title = "Text Data Recognizer",          Code = ListItemCode.TextDataRecognizer },
            new ListItem { Title = "Check Recognizer",              Code = ListItemCode.CheckRecognizer },
        };
    }
}
