using System;
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

            new ListItem { Title = "Scan DC Form",                  Code = ListItemCode.WorkflowDC           },
            new ListItem { Title = "Scan MRZ + Image",              Code = ListItemCode.WorkflowMRZImage     },
            new ListItem { Title = "Scan MRZ + Front & Back Image", Code = ListItemCode.WorkflowMRZFrontBack },
            new ListItem { Title = "Generic Document Recognizer",   Code = ListItemCode.GenericDocumentRecognizer },
#if __IOS__
            new ListItem { Title = "Scan SEPA Pay Form", Code = ListItemCode.WorkflowSEPA },
            new ListItem { Title = "Scan QR Code", Code = ListItemCode.WorkflowQR }
#endif
            
        };
    }
}
