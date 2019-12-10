using System;
using UIKit;
using PdfKit;
using Foundation;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class PdfViewController : UIViewController
    {
        public NSUrl Uri { get; private set; }

        public PdfViewController(NSUrl uri)
        {
            Uri = uri;
            ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = Uri.LastPathComponent;

            var view = new PdfView(View.Bounds);
            view.DisplayMode = PdfDisplayMode.SinglePage;
            view.AutoScales = true;
            view.Document = new PdfDocument(Uri);
            View = view;
        }
    }
}
