using System;
using System.Collections.Generic;
using System.Linq;
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

        public static BatchBarcodeHandler BatchBarcode = new BatchBarcodeHandler();

        public static GenericDocumentRecognizerDelegate GDR = new GenericDocumentRecognizerDelegate();

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

            private UIViewController rootViewController;

            public MRZDelegate WithViewController(UIViewController rootViewController) {
                this.rootViewController = rootViewController;
                return this;
            }

            public override void DidDetect(
                SBSDKUIMRZScannerViewController viewController, SBSDKMachineReadableZoneRecognizerResult zone)
            {
                viewController.Delegate = null;
                viewController.RecognitionEnabled = false;
                viewController.DismissViewController(true, null);
                ShowPopup(rootViewController, zone.StringRepresentation);
            }
        }

        public class HealthInsuranceCardHandler : SBSDKUIHealthInsuranceCardScannerViewControllerDelegate
        {
            public override void HealthInsuranceCardDetectionViewController(SBSDKUIHealthInsuranceCardScannerViewController viewController, SBSDKHealthInsuranceCardRecognitionResult card)
            {
                ShowPopup(viewController, card.StringRepresentation);
            }
        }

        public class BarcodeHandler : SBSDKUIBarcodeScannerViewControllerDelegate
        {
            public override void DidDetectResults(SBSDKUIBarcodeScannerViewController viewController,
                SBSDKBarcodeScannerResult[] barcodeResults)
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
            public override void DidFinish(SBSDKUIBarcodesBatchScannerViewController viewController,
                SBSDKUIBarcodeMappedResult[] barcodeResults)
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

        public class GenericDocumentRecognizerDelegate : SBSDKUIGenericDocumentRecognizerViewControllerDelegate
        {
            private WeakReference<UIViewController> viewController = new WeakReference<UIViewController>(null);

            public GenericDocumentRecognizerDelegate WithPresentingViewController(UIViewController viewController) {
                this.viewController = new WeakReference<UIViewController>(viewController);
                return this;
            }

            public override void GenericDocumentRecognizerViewController(SBSDKUIGenericDocumentRecognizerViewController viewController, SBSDKGenericDocument[] documents)
            {
                if (documents == null || documents.Length == 0)
                {
                    return;
                }

                // We only take the first document for simplicity
                var firstDocument = documents.First();
                var fields = firstDocument.Fields
                    .Where((f) => f != null && f.Type != null && f.Type.Name != null && f.Value != null && f.Value.Text != null)
                    .Select((f) => string.Format("{0}: {1}", f.Type.Name, f.Value.Text))
                    .ToList();
                var description = string.Join("\n", fields);
                Console.WriteLine(description);

                this.viewController.TryGetTarget(out UIViewController vc);
                if (vc != null) { 
                    ShowPopup(vc, description);
                }
            }
        }
    }
}
