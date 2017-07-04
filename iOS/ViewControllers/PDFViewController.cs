using Foundation;
using System;
using UIKit;

namespace scanbotsdkexamplexamarin.iOS
{
    public partial class PDFViewController : UIViewController
    {
        NSUrl pdfUrl;

        public PDFViewController(IntPtr handle) : base(handle) { }

        public void ShowPDF(NSUrl url)
        {
            pdfUrl = url;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (pdfUrl != null)
            {
                NSUrlRequest request = NSUrlRequest.FromUrl(pdfUrl);
                webView.LoadRequest(request);
            }
        }
    }
}