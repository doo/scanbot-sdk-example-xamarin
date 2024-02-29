using System;
using System.Collections.Generic;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.Utils;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.iOS;
using UIKit;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class MainViewController : UIViewController
    {
        public MainView ContentView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new MainView();
            View = ContentView;

            Title = "Scanbot SDK RTU UI Example";

            ContentView.AddContent(DocumentScanner.Instance);
            ContentView.AddContent(DataDetectors.Instance);
            ContentView.AddContent(BarcodeDetectors.Instance);

            ContentView.LicenseIndicator.Text = Texts.no_license_found_the_app_will_terminate_after_one_minute;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!SBSDK.IsLicenseValid())
            {
                ContentView.LayoutSubviews();
            }

            foreach (ScannerButton button in ContentView.DocumentScanner.Buttons)
            {
                button.Click += OnScannerButtonClick;
            }

            foreach (ScannerButton button in ContentView.BarcodeDetectors.Buttons)
            {
                button.Click += OnBarcodeButtonClick;
            }

            foreach (ScannerButton button in ContentView.DataDetectors.Buttons)
            {
                button.Click += OnDataButtonClick;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            foreach (ScannerButton button in ContentView.DocumentScanner.Buttons)
            {
                button.Click -= OnScannerButtonClick;
            }

            foreach (ScannerButton button in ContentView.BarcodeDetectors.Buttons)
            {
                button.Click -= OnBarcodeButtonClick;
            }

            foreach (ScannerButton button in ContentView.DataDetectors.Buttons)
            {
                button.Click -= OnDataButtonClick;
            }
        }

        private async void OnScannerButtonClick(object sender, EventArgs e)
        {
            if (!SBSDK.IsLicenseValid())
            {
                ContentView.LayoutSubviews();
                Alert.Show(this, "License Info!", "Invalid or missing license");
                return;
            }

            var button = (ScannerButton)sender;

            if (button.Data.Code == ListItemCode.ScanDocument)
            {
                ScanDocumentTapped();
            }
            else if (button.Data.Code == ListItemCode.ImportImage)
            {
                var image = await Scanbot.ImagePicker.iOS.ImagePicker.Instance.Pick();
                var page = PageRepository.Add(image, new SBSDKPolygon());
                var result = page.DetectDocument(true);
                Console.WriteLine("Attempted document detection on imported page: " + result.Status);
                OpenImageListController();
            }
            else if (button.Data.Code == ListItemCode.ViewImages)
            {
                OpenImageListController();
            }
        }

        void OpenImageListController()
        {
            var controller = new ImageListController();
            NavigationController.PushViewController(controller, true);
        }

        void OnBarcodeButtonClick(object sender, EventArgs e)
        {
            var button = (ScannerButton)sender;
            if (button.Data.Code == ListItemCode.ScannerBarcode)
            {
                ScanBarcodeTapped();
            }
            else if (button.Data.Code == ListItemCode.ScannerBatchBarcode)
            {
                ScanBatchBarcodeTapped();
            }
            else if (button.Data.Code == ListItemCode.ScannerImportBarcode)
            {
                _ = ImportBarcodeAndScanTapped();
            }
        }

        private void ScanBarcodeTapped()
        {
            var configuration = SBSDKUIBarcodeScannerConfiguration.DefaultConfiguration;
            var controller = SBSDKUIBarcodeScannerViewController.CreateNewWithConfiguration(configuration, null);
            controller.DidDetectResults += (viewController, args) =>
            {
                var barcodeController = viewController as SBSDKUIBarcodeScannerViewController;
                var barcodeResults = args.BarcodeResults;
                string text = "No barcode detected";

                if (barcodeResults.Length > 0)
                {
                    barcodeController.RecognitionEnabled = false; // stop recognition
                    var result = barcodeResults[0];
                    text = $"Found Barcode(s):\n\n";

                    foreach (var code in barcodeResults)
                    {
                        text += code.Type.Name + ": " + code.RawTextString + "\n";
                    }
                }

                Alert.ShowPopup(barcodeController, text, delegate
                {
                    barcodeController.RecognitionEnabled = true; // continue recognition
                });
            };
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

        private void ScanBatchBarcodeTapped()
        {
            var configuration = SBSDKUIBarcodesBatchScannerConfiguration.DefaultConfiguration;
            var controller = SBSDKUIBarcodesBatchScannerViewController.CreateNewWithConfiguration(configuration, null);
            controller.DidFinish += (viewController, args) =>
            {
                var barcodeController = viewController as SBSDKUIBarcodesBatchScannerViewController;
                var barcodeResults = args.BarcodeResults;
                string text = "No barcode detected";

                if (barcodeResults.Length > 0)
                {
                    barcodeController.RecognitionEnabled = false; // stop recognition
                    var result = barcodeResults[0];
                    text = $"Found Barcode(s):\n\n";

                    foreach (var code in barcodeResults)
                    {
                        text += code.Barcode.Type.Name + ": " + code.Barcode.RawTextString + "\n";
                    }
                }

                Alert.ShowPopup(this, text);
            };
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

        private async Task ImportBarcodeAndScanTapped()
        {
            var image = await Scanbot.ImagePicker.iOS.ImagePicker.Instance.Pick();
            var text = "No Barcode detected.";
            if (image != null)
            {
                SBSDKBarcodeScannerResult[] results = new SBSDKBarcodeScanner().DetectBarCodesOnImage(image);

                if (results != null && results.Length > 0)
                {
                    text = "";
                    foreach (var item in results)
                    {
                        text += item.Type.ToString() + ": " + item.RawTextString + "\n";
                    }

                    var quality = new SBSDKDocumentQualityAnalyzer().AnalyzeOnImage(image);
                    Console.WriteLine("Quality of the imported image: " + quality);
                    text += "(Additionally, Quality: " + quality + ")";
                }
            }
            else
            {
                text = "Image format not recognized";
            }

            Alert.Show(this, "Detected Barcodes", text);
        }

        private void OnDataButtonClick(object sender, EventArgs e)
        {
            if (!SBSDK.IsLicenseValid())
            {
                ContentView.LayoutSubviews();
                return;
            }

            var button = (ScannerButton)sender;

            if (button.Data.Code == ListItemCode.ScannerMRZ)
            {
                MRZScannerTapped();
            }
            else if (button.Data.Code == ListItemCode.ScannerEHIC)
            {
                HealthInsuranceCardScannerTapped();
            }
            else if (button.Data.Code == ListItemCode.GenericDocumentRecognizer)
            {
                GenericDocumentRecognizerTapped();
            }
            else if (button.Data.Code == ListItemCode.CheckRecognizer)
            {
                CheckRecognizerTapped();
            }
            else if (button.Data.Code == ListItemCode.TextDataRecognizer)
            {
                TextDataScannerTapped();
            }
            else if (button.Data.Code == ListItemCode.VinRecognizer)
            {
                VinRecognizerTapped();
            }
        }

        private void MRZScannerTapped()
        {
            var config = new SBSDKUIMRZScannerConfiguration();
            config.TextConfiguration.CancelButtonTitle = "Done";
            var controller = SBSDKUIMRZScannerViewController
                .CreateNewWithConfiguration(config, null);
            controller.DidDetect += (viewController, args) =>
            {
                var mrzController = viewController as SBSDKUIMRZScannerViewController;
                mrzController.Delegate = null;
                mrzController.RecognitionEnabled = false;
                mrzController.DismissViewController(true, delegate
                {
                    Alert.ShowPopup(this, args.Zone.StringRepresentation);
                });
            };

            PresentViewController(controller, true, null);
        }

        private void HealthInsuranceCardScannerTapped()
        {
            var configuration = SBSDKUIHealthInsuranceCardScannerConfiguration.DefaultConfiguration;
            var controller = SBSDKUIHealthInsuranceCardScannerViewController
                .CreateNewWithConfiguration(configuration, null);

            controller.DidDetectCard += (sender, args) =>
            {
                Alert.ShowPopup(this, args.Card.StringRepresentation);
            };

            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

        private void CheckRecognizerTapped()
        {
            var configuration = SBSDKUICheckRecognizerConfiguration.DefaultConfiguration;
            configuration.BehaviorConfiguration.AcceptedCheckStandards = new SBSDKCheckDocumentRootType[] {
                    SBSDKCheckDocumentRootType.AusCheck(),
                    SBSDKCheckDocumentRootType.FraCheck(),
                    SBSDKCheckDocumentRootType.IndCheck(),
                    SBSDKCheckDocumentRootType.KwtCheck(),
                    SBSDKCheckDocumentRootType.UsaCheck(),
                };
            var controller = SBSDKUICheckRecognizerViewController.CreateNewWithConfiguration(configuration, null);
            controller.DidRecognizeCheck += (viewController, args) =>
            {
                var checkController = viewController as SBSDKUICheckRecognizerViewController;
                if (args.Result == null || args.Result.Document == null)
                {
                    return;
                }

                var fields = args.Result.Document.Fields
                    .Where((f) => f != null && f.Type != null && f.Type.Name != null && f.Value != null && f.Value.Text != null)
                    .Select((f) => string.Format("{0}: {1}", f.Type.Name, f.Value.Text))
                    .ToList();
                var description = string.Join("\n", fields);
                Console.WriteLine(description);
                checkController.DismissViewController(true, () => { Alert.ShowPopup(this, description); });
            };
            PresentViewController(controller, false, null);
        }

        private void ScanDocumentTapped()
        {
            var config = SBSDKUIDocumentScannerConfiguration.DefaultConfiguration;
            config.BehaviorConfiguration.MultiPageEnabled = true;
            config.BehaviorConfiguration.IgnoreBadAspectRatio = true;
            config.TextConfiguration.PageCounterButtonTitle = "%d Page(s)";
            config.TextConfiguration.TextHintOK = "Don't move.\nCapturing document...";
            config.UiConfiguration.BottomBarBackgroundColor = UIColor.Blue;
            config.UiConfiguration.BottomBarButtonsColor = UIColor.White;
            // see further customization configs...

            var controller = SBSDKUIDocumentScannerViewController
                .CreateNewWithConfiguration(config, null);
            controller.DidFinishWithDocument += (sender, args) =>
            {
                if (args.Document.NumberOfPages == 0)
                {
                    return;
                }

                var pages = new List<SBSDKUIPage>();
                for (var i = 0; i < args.Document.NumberOfPages; ++i)
                {
                    var page = args.Document.PageAtIndex(i);
                    page.DetectDocument(true);
                    pages.Add(page);
                }
                PageRepository.Add(pages);
                OpenImageListController();
            };

            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

        private void GenericDocumentRecognizerTapped()
        {
            var configuration = SBSDKUIGenericDocumentRecognizerConfiguration.DefaultConfiguration;
            configuration.BehaviorConfiguration.DocumentType = SBSDKUIDocumentType.IdCardFrontBackDE;
            var controller = SBSDKUIGenericDocumentRecognizerViewController.CreateNewWithConfiguration(configuration, null);
            controller.DidFinishWithDocuments += (viewController, args) =>
            {
                var documents = args.Documents;

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

                Alert.ShowPopup(this, description);
            };
            PresentViewController(controller, false, null);
        }

        private void TextDataScannerTapped()
        {
            Debug.WriteLine("ScanbotSDK Demo: Starting text recognizer controller ...");

            var configuration = SBSDKUITextDataScannerConfiguration.DefaultConfiguration;
            configuration.TextConfiguration.CancelButtonTitle = "Done";
            var scanner = SBSDKUITextDataScannerViewController.CreateNewWithConfiguration(configuration, null);
            scanner.DidFinishStepWithResult += (controller, args) =>
            {
                var viewController = controller as SBSDKUITextDataScannerViewController;
                if (viewController.RecognitionEnabled && !string.IsNullOrEmpty(args.Result.Text))
                {
                    viewController.RecognitionEnabled = false;
                    viewController.DismissViewController(true, () => Alert.Show(this, "Result Text:", args.Result.Text));
                }
            };

            PresentViewController(scanner, true, null);
        }

        private void VinRecognizerTapped()
        {
            Debug.WriteLine("ScanbotSDK Demo: Starting VIN recognizer controller ...");

            var configuration = SBSDKUIVINScannerConfiguration.DefaultConfiguration;
            configuration.TextConfiguration.CancelButtonTitle = "Done";
            configuration.TextConfiguration.GuidanceText = "Please place the finder over the VIN.";
            configuration.UiConfiguration.FinderAspectRatio = new SBSDKAspectRatio(7, 1);
            var scanner = SBSDKUIVINScannerViewController.CreateNewWithConfiguration(configuration, null);
            scanner.DidFinishWithResult += (controller, args) =>
            {
                var viewController = controller as SBSDKUIVINScannerViewController;
                if (viewController.RecognitionEnabled && !string.IsNullOrEmpty(args.Result.Text))
                {
                    viewController.RecognitionEnabled = false;
                    viewController.DismissViewController(true, () => Alert.Show(this, "Result Text:", args.Result.Text));
                }
            };

            PresentViewController(scanner, true, null);
        }
    }
}
