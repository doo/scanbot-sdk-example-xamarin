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
            new ListItem { Title = "Scan DC Form", Code = ListItemCode.ScanDC },
            new ListItem { Title = "Scan MRZ", Code = ListItemCode.ScanMRZ },
            new ListItem { Title = "Scan MRZ + Image", Code = ListItemCode.ScanMRZImage },
            new ListItem { Title = "Scan MRZ + Front & Back Image", Code = ListItemCode.ScanMRZFrontBack },
            new ListItem { Title = "Scan SEPA Pay Form", Code = ListItemCode.ScanSEPA }
        };
    }
}
