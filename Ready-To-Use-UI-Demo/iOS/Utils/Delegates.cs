using System;
using System.Collections.Generic;
using ReadyToUseUIDemo.iOS.Controller;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Utils
{
    public class Delegates
    {
        public static MRZDelegate MRZ = new MRZDelegate();

        public static HealthInsuranceCardHandler EHIC = new HealthInsuranceCardHandler();

        public static BarcodeHandler Barcode = new BarcodeHandler();

        public static bool IsPresented { get; set; }

        public static void ShowPopup(UIViewController controller, string text)
        {
            if (IsPresented)
            {
                return;
            }

            IsPresented = true;

            var images = new List<UIImage>();
            var popover = new PopupController(text, images);

            controller.PresentViewController(popover, true, delegate
            {
                popover.Content.CloseButton.Click += delegate
                {
                    IsPresented = false;
                    popover.Dismiss();
                };
            });
        }

        public class MRZDelegate : SBSDKUIMRZScannerViewControllerDelegate
        {
            
            public override void DidDetect(
                SBSDKUIMRZScannerViewController viewController, SBSDKMachineReadableZoneRecognizerResult zone)
            {
                ShowPopup(viewController, zone.StringRepresentation);
            }
        }

        public class HealthInsuranceCardHandler : SBSDKUIHealthInsuranceCardScannerViewControllerDelegate
        {
            public override void DidDetect(SBSDKUIHealthInsuranceCardScannerViewController viewController,
                SBSDKHealthInsuranceCardRecognitionResult card)
            {
                ShowPopup(viewController, card.StringRepresentation);
            }
        }

        public class BarcodeHandler : SBSDKUIBarcodeScannerViewControllerDelegate
        {
            public override void DidDetectResults(SBSDKUIBarcodeScannerViewController viewController, SBSDKBarcodeScannerResult[] barcodeResults)
            {

                string text = "No barcode detected";
                if (barcodeResults.Length > 0)
                {
                    var result = barcodeResults[0];
                    text = $"Found Barcode!\nText: {result.RawTextString}\nType: {result.Type.Name}";
                }
                
                ShowPopup(viewController, text);
            }
        }
    }
}
