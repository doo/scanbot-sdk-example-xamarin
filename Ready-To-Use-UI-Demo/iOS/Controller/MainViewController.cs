
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.Service;
using ReadyToUseUIDemo.iOS.Utils;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.iOS;
using UIKit;
using MobileCoreServices;

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

            WorkflowStepValidator.MainController = this;
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

        private void OnScannerButtonClick(object sender, EventArgs e)
        {
            if (!SBSDK.IsLicenseValid())
            {
                ContentView.LayoutSubviews();
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
                ImagePicker.Instance.Present(this);
                ImagePicker.Instance.Controller.FinishedPickingMedia += ImageImported;
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

                    var blur = new SBSDKBlurrinessEstimator().EstimateImageBlurriness(image);
                    Console.WriteLine("Blur of imported image: " + blur);
                    text += "(Additionally, blur: " + blur + ")";
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
                var controller = SBSDKUIBarcodeScannerViewController
                    .CreateNewWithAcceptedMachineCodeTypes(
                    SBSDKBarcodeType.AllTypes, configuration, Delegates.Barcode);
                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
            }
            else if (button.Data.Code == ListItemCode.ScannerBatchBarcode)
            {
                var configuration = SBSDKUIBarcodesBatchScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIBarcodesBatchScannerViewController
                    .CreateNewWithAcceptedMachineCodeTypes(
                    SBSDKBarcodeType.AllTypes, configuration, Delegates.BatchBarcode
                    );
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
                var config = SBSDKUIMRZScannerConfiguration.DefaultConfiguration;
                config.TextConfiguration.CancelButtonTitle = "Done";
                //config.TextConfiguration.FinderTextHint = "Custom finder text ..."
                // see further customization configs
                var viewSize = View.Frame.Size;
                var targetWidth = viewSize.Width - ((viewSize.Width * 0.058) * 2);
                var aspect = new SBSDKAspectRatio(targetWidth, targetWidth * 0.3);
                config.UiConfiguration.FinderAspectRatio = aspect;
                var controller = SBSDKUIMRZScannerViewController
                    .CreateNewWithConfiguration(config, Delegates.MRZ.WithViewController(this));
                PresentViewController(controller, true, null);
            }

            else if (button.Data.Code == ListItemCode.WorkflowMC)
            {
                var ratios = new SBSDKAspectRatio[]
                {
                    // MC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                    new SBSDKAspectRatio(148.0, 210.0),
                    // MC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
                    new SBSDKAspectRatio(148.0, 105.0)
                };

                var title = "Please align the MC form in the frame.";
                var name = "MedicalCertificateFlow";

                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanDisabilityCertificateWorkflowStep(
                        title, "", ratios, true, WorkflowStepValidator.OnMCFormStep
                    )
                };

                PresentController(name, steps);
            }
            else if (button.Data.Code == ListItemCode.WorkflowMRZImage)
            {
                var title = "Please align the Machine readable card with the form in the frame";
                var name = "MRZScanFlow";

                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanMachineReadableZoneWorkflowStep(
                        title, "", MRZRatios, true, WorkflowStepValidator.OnIDCardBackStep
                    )
                };

                PresentController(name, steps);
            }
            else if (button.Data.Code == ListItemCode.WorkflowMRZFrontBack)
            {
                var name = "MRZBackFrontScanFlow";

                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIWorkflowStep(
                        "Step 1/2", "Please scan the front side of your ID card",
                        MRZRatios, true, false, null, WorkflowStepValidator.OnIDCardFrontStep
                        ),
                    new SBSDKUIScanMachineReadableZoneWorkflowStep(
                        "Step 2/2", "Please scan the back side of your ID card",
                        MRZRatios, true, WorkflowStepValidator.OnIDCardBackStep
                    )
                };

                PresentController(name, steps);
            }
            else if (button.Data.Code == ListItemCode.WorkflowSEPA)
            {
                var name = "SEPAScanFlow";
                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanPayFormWorkflowStep(
                        "Please scan a SEPA PayForm", "", false, WorkflowStepValidator.OnPayFormStep
                    )
                };

                PresentController(name, steps);

            }
            else if (button.Data.Code == ListItemCode.WorkflowQR)
            {
                var name = "QRCodeScanFlow";
                var types = SBSDKUIMachineCodesCollection.TwoDimensionalBarcodes;
                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanBarCodeWorkflowStep("Scan your QR code", "",
                    types, new SBSDKAspectRatio(1, 1), WorkflowStepValidator.OnBarCodeStep)
                };

                PresentController(name, steps);
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
        }

        void PresentController(string name, SBSDKUIWorkflowStep[] steps,
            SBSDKUIWorkflowScannerConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
            }
            
            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(steps, name, null);

            var config = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
            
            var controller = SBSDKUIWorkflowScannerViewController.CreateNewWithWorkflow(workflow, config, null);
            WorkflowStepValidator.WorkflowController = controller;
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
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
        
        public override void DidFinish(SBSDKUIDocumentScannerViewController viewController, SBSDKUIPage[] pages)
        {
            if (pages.Length == 0)
            {
                return;
            }

            Selected?.Invoke(this, new PageEventArgs { Pages = pages.ToList() });
        }
    }
}
