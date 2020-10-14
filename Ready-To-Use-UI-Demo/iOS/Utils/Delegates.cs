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

        public static BatchBarcodeHandler BatchBarcode= new BatchBarcodeHandler();

        public static bool IsPresented { get; set; }

        public static void ShowPopup(UIViewController controller, string text, Action onClose = null)
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
                    onClose?.Invoke();
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
                    viewController.RecognitionEnabled = false; // stop recognition
                    var result = barcodeResults[0];
                    text = $"Found Barcode(s):\n\n";

                    foreach (var code in barcodeResults)
                    {
                        text += code.Type.Name + ": " + code.RawTextString + "\n";
                    }
                }
                
                ShowPopup(viewController, text, delegate {
                    viewController.RecognitionEnabled = true; // continue recognition
                });
            }
        }

        public class BatchBarcodeHandler : SBSDKUIBarcodesBatchScannerViewControllerDelegate
        {
            public override void DidFinish(SBSDKUIBarcodesBatchScannerViewController viewController, SBSDKUIBarcodeMappedResult[] barcodeResults)
            {
                string text = "No barcode detected";
                if (barcodeResults.Length > 0)
                {
                    viewController.RecognitionEnabled = false; // stop recognition
                    var result = barcodeResults[0];
                    text = $"Found Barcode(s):\n\n";

                    foreach (var code in barcodeResults)
                    {
                        text += code.Barcode.Type.Name + ": " + code.Barcode.RawTextString + "\n";
                    }
                }
                ShowPopup(viewController, text);
            }
        }

    }
}
