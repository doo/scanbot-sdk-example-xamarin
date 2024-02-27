using System;
using System.Collections.Generic;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.Service;
using ReadyToUseUIDemo.iOS.Utils;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.iOS;
using UIKit;
using System.Diagnostics;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class MainViewController : UIViewController
    {
        public MainView ContentView { get; set; }

        public SimpleScanCallback CameraCallback { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new MainView();
            View = ContentView;

            Title = "Scanbot SDK RTU UI Example";

            ContentView.AddContent(DocumentScanner.Instance);
            ContentView.AddContent(DataDetectors.Instance);
            ContentView.AddContent(BarcodeDetectors.Instance);
            CameraCallback = new SimpleScanCallback();

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

            CameraCallback.Selected += OnScanComplete;
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

            CameraCallback.Selected -= OnScanComplete;
        }

        private void OnScanComplete(object sender, PageEventArgs e)
        {
            foreach (var page in e.Pages)
            {
                var result = page.DetectDocument(true);
                //Console.WriteLine("Attempted document detection on imported page: " + result.Status);
            }
            PageRepository.Add(e.Pages);
            OpenImageListController();
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
                var config = SBSDKUIDocumentScannerConfiguration.DefaultConfiguration;
                config.BehaviorConfiguration.MultiPageEnabled = true;
                config.BehaviorConfiguration.IgnoreBadAspectRatio = true;
                config.TextConfiguration.PageCounterButtonTitle = "%d Page(s)";
                config.TextConfiguration.TextHintOK = "Don't move.\nCapturing document...";
                config.UiConfiguration.BottomBarBackgroundColor = UIColor.Blue;
                config.UiConfiguration.BottomBarButtonsColor = UIColor.White;
                // see further customization configs...

                var controller = SBSDKUIDocumentScannerViewController
                    .CreateNewWithConfiguration(config, CameraCallback);
                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
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

        private void ImageImported(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            ImagePicker.Instance.Dismiss();
            ImagePicker.Instance.Controller.FinishedPickingMedia -= ImageImported;

            var page = PageRepository.Add(e.OriginalImage, new SBSDKPolygon());
            var result = page.DetectDocument(true);
            Console.WriteLine("Attempted document detection on imported page: " + result.Status);

            OpenImageListController();
        }

        void OpenImageListController()
        {
            var controller = new ImageListController();
            NavigationController.PushViewController(controller, true);
        }

        private void BarcodeImported(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            ImagePicker.Instance.Dismiss();
            ImagePicker.Instance.Controller.FinishedPickingMedia -= BarcodeImported;

            var text = "No Barcode detected.";

            if (e.OriginalImage is UIImage image)
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

        void OnBarcodeButtonClick(object sender, EventArgs e)
        {
            var button = (ScannerButton)sender;
            if (button.Data.Code == ListItemCode.ScannerBarcode)
            {
                var configuration = SBSDKUIBarcodeScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIBarcodeScannerViewController.CreateNewWithConfiguration(configuration, Delegates.Barcode);
                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
            }
            else if (button.Data.Code == ListItemCode.ScannerBatchBarcode)
            {
                var configuration = SBSDKUIBarcodesBatchScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIBarcodesBatchScannerViewController.CreateNewWithConfiguration(configuration, Delegates.BatchBarcode);
                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
            }
            else if (button.Data.Code == ListItemCode.ScannerImportBarcode)
            {
                ImagePicker.Instance.Controller.FinishedPickingMedia += BarcodeImported;
                ImagePicker.Instance.Present(this);
            }
        }

        SBSDKAspectRatio[] MRZRatios = { new SBSDKAspectRatio(85.0, 54.0) };

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
                var config = new SBSDKUIMRZScannerConfiguration();
                config.TextConfiguration.CancelButtonTitle = "Done";
                var controller = SBSDKUIMRZScannerViewController
                    .CreateNewWithConfiguration(config, Delegates.MRZ.WithViewController(this));
                PresentViewController(controller, true, null);
            }
            else if (button.Data.Code == ListItemCode.ScannerEHIC)
            {
                var configuration = SBSDKUIHealthInsuranceCardScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIHealthInsuranceCardScannerViewController
                    .CreateNewWithConfiguration(configuration, Delegates.EHIC);

                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
            }
            else if (button.Data.Code == ListItemCode.GenericDocumentRecognizer)
            {
                var configuration = SBSDKUIGenericDocumentRecognizerConfiguration.DefaultConfiguration;
                configuration.BehaviorConfiguration.DocumentType = SBSDKUIDocumentType.IdCardFrontBackDE;
                var controller = SBSDKUIGenericDocumentRecognizerViewController.CreateNewWithConfiguration(configuration, Delegates.GDR.WithPresentingViewController(this));
                PresentViewController(controller, false, null);
            }
            else if (button.Data.Code == ListItemCode.CheckRecognizer)
            {
                var configuration = SBSDKUICheckRecognizerConfiguration.DefaultConfiguration;
                configuration.BehaviorConfiguration.AcceptedCheckStandards = new SBSDKCheckDocumentRootType[] {
                    SBSDKCheckDocumentRootType.AusCheck(),
                    SBSDKCheckDocumentRootType.FraCheck(),
                    SBSDKCheckDocumentRootType.IndCheck(),
                    SBSDKCheckDocumentRootType.KwtCheck(),
                    SBSDKCheckDocumentRootType.UsaCheck(),
                };
                var controller = SBSDKUICheckRecognizerViewController.CreateNewWithConfiguration(configuration, Delegates.Check.WithPresentingViewController(this));
                PresentViewController(controller, false, null);
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

        private void TextDataScannerTapped()
        {
            Debug.WriteLine("ScanbotSDK Demo: Starting text recognizer controller ...");

            var configuration = SBSDKUITextDataScannerConfiguration.DefaultConfiguration;
            configuration.TextConfiguration.CancelButtonTitle = "Done";
            var scanner = SBSDKUITextDataScannerViewController.CreateNewWithConfiguration(configuration, new TextDataScannerDelegate(successHandler: (text) =>
            {
                Alert.Show(this, "Result Text:", text);
            }));

            PresentViewController(scanner, true, null);
        }

        private void VinRecognizerTapped()
        {
            Debug.WriteLine("ScanbotSDK Demo: Starting VIN recognizer controller ...");

            var configuration = SBSDKUIVINScannerConfiguration.DefaultConfiguration;
            configuration.TextConfiguration.CancelButtonTitle = "Done";
            configuration.TextConfiguration.GuidanceText = "Please place the finder over the VIN.";
            configuration.UiConfiguration.FinderAspectRatio = new SBSDKAspectRatio(7, 1);
            var scanner = SBSDKUIVINScannerViewController.CreateNewWithConfiguration(configuration, new VINScannerDelegate(successHandler: (text) =>
            {
                Alert.Show(this, "Result Text:", text);
            }));

            PresentViewController(scanner, true, null);
        }
    }

    internal class VINScannerDelegate : SBSDKUIVINScannerViewControllerDelegate
    {
        private Action<string> successHandler;
        public VINScannerDelegate(Action<string> successHandler)
        {
            this.successHandler = successHandler;
        }

        public override void DidFinishWithResult(SBSDKUIVINScannerViewController viewController, SBSDKVehicleIdentificationNumberScannerResult result)
        {
            if (viewController.RecognitionEnabled && result?.ValidationSuccessful == true)
            {
                viewController.RecognitionEnabled = false;
                viewController.DismissViewController(true, () => successHandler?.Invoke(result.Text ?? string.Empty));
            }
        }
    }

    public class PageEventArgs : EventArgs
    {
        public bool IsMultiPage => Pages.Count > 1;

        public SBSDKUIPage Page => Pages[0];

        public List<SBSDKUIPage> Pages { get; set; }
    }

    public class SimpleScanCallback : SBSDKUIDocumentScannerViewControllerDelegate
    {
        public EventHandler<PageEventArgs> Selected;
        public override void DidFinishWithDocument(SBSDKUIDocumentScannerViewController viewController, SBSDKUIDocument document)
        {
            if (document.NumberOfPages == 0)
            {
                return;
            }

            var pages = new List<SBSDKUIPage>();
            for (var i = 0; i < document.NumberOfPages; ++i)
            {
                pages.Add(document.PageAtIndex(i));
            }

            Selected?.Invoke(this, new PageEventArgs { Pages = pages });
        }
    }

    class TextDataScannerDelegate : SBSDKUITextDataScannerViewControllerDelegate
    {
        Action<string> RecognitionSuccess;
        public TextDataScannerDelegate(Action<string> successHandler)
        {
            this.RecognitionSuccess = successHandler;
        }
        public override void DidFinishStepWithResult(SBSDKUITextDataScannerViewController viewController, SBSDKUITextDataScannerStep step, SBSDKUITextDataScannerStepResult result)
        {
            if (viewController.RecognitionEnabled && !string.IsNullOrEmpty(result?.Text))
            {
                viewController.RecognitionEnabled = false;
                viewController.DismissViewController(true, () => RecognitionSuccess?.Invoke(result?.Text));
            }
        }
    }
}
