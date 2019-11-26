
using System;
using CoreGraphics;
using ReadyToUseUIDemo.model;
using ScanbotSDK.Xamarin.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class MainView : UIScrollView
    {
        public UILabel LicenseIndicator { get; private set; }

        public ButtonContainer DocumentScanner { get; private set; }

        public ButtonContainer DataDetectors { get; private set; }

        public MainView()
        {
            BackgroundColor = UIColor.White;

            LicenseIndicator = new UILabel();
            AddSubview(LicenseIndicator);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat largePadding = 20;

            nfloat x = 0;
            nfloat y = 0;
            nfloat w = Frame.Width;
            nfloat h = Frame.Width / 5;

            if (SBSDK.IsLicenseValid())
            {
                h = 0;
            }

            LicenseIndicator.Frame = new CGRect(x, y, w, h);

            y += h + largePadding;
            h = DocumentScanner.Height;

            DocumentScanner.Frame = new CGRect(x, y, w, h);

            y += h + largePadding;
            h = DataDetectors.Height;

            DataDetectors.Frame = new CGRect(x, y, w, h);
        }

        public void AddContent(DocumentScanner instance)
        {
            DocumentScanner = new ButtonContainer(instance.Title, instance.Items);
            AddSubview(DocumentScanner);
        }

        public void AddContent(DataDetectors instance)
        {
            DataDetectors = new ButtonContainer(instance.Title, instance.Items);
            AddSubview(DataDetectors);
        }
    }
}
