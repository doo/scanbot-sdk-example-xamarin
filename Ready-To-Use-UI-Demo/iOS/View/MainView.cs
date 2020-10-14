
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
        
        public ButtonContainer BarcodeDetectors { get; private set; }

        public ButtonContainer DataDetectors { get; private set; }

        public MainView()
        {
            BackgroundColor = UIColor.White;

            LicenseIndicator = new UILabel();
            LicenseIndicator.TextColor = UIColor.White;
            LicenseIndicator.BackgroundColor = Colors.ScanbotRed;
            LicenseIndicator.Layer.CornerRadius = 5;
            LicenseIndicator.Font = UIFont.FromName("HelveticaNeue", 13);
            LicenseIndicator.Lines = 0;
            LicenseIndicator.ClipsToBounds = true;
            LicenseIndicator.TextAlignment = UITextAlignment.Center;

            AddSubview(LicenseIndicator);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat largePadding = 20;

            nfloat x = largePadding;
            nfloat y = largePadding;
            nfloat w = Frame.Width - 2 * x;
            nfloat h = Frame.Width / 6;

            if (SBSDK.IsLicenseValid())
            {
                h = 0;
            }

            LicenseIndicator.Frame = new CGRect(x, y, w, h);

            x = 0;
            y += h + largePadding;
            w = Frame.Width;
            h = DocumentScanner.Height;

            DocumentScanner.Frame = new CGRect(x, y, w, h);

            y += h + largePadding;
            h = BarcodeDetectors.Height;

            BarcodeDetectors.Frame = new CGRect(x, y, w, h);

            y += h + largePadding;
            h = DataDetectors.Height;

            DataDetectors.Frame = new CGRect(x, y, w, h);

            ContentSize = new CGSize(Frame.Width, DataDetectors.Frame.Bottom);
        }

        public void AddContent(DocumentScanner instance)
        {
            DocumentScanner = new ButtonContainer(instance.Title, instance.Items);
            AddSubview(DocumentScanner);
        }

        public void AddContent(BarcodeDetectors instance)
        {
            BarcodeDetectors = new ButtonContainer(instance.Title, instance.Items);
            AddSubview(BarcodeDetectors);
        }

        public void AddContent(DataDetectors instance)
        {
            DataDetectors = new ButtonContainer(instance.Title, instance.Items);
            AddSubview(DataDetectors);
        }
    }
}
