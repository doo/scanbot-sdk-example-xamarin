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
#if __IOS__
            // iOS has no simple MRZ scan
#else
            new ListItem { Title = "Scan MRZ", Code = ListItemCode.ScanMRZ },
#endif
            new ListItem { Title = "Scan MRZ + Image", Code = ListItemCode.ScanMRZImage },
            new ListItem { Title = "Scan MRZ + Front & Back Image", Code = ListItemCode.ScanMRZFrontBack },
            new ListItem { Title = "Scan SEPA Pay Form", Code = ListItemCode.ScanSEPA },
            new ListItem { Title = "Scan QR Code", Code = ListItemCode.ScanQRBar }
        };
    }
}
