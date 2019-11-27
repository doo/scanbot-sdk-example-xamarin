
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.iOS.Service;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class MainViewController : UIViewController
    {
        public MainView ContentView { get; set; }

        public ScanResultCallback Callback { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new MainView();
            View = ContentView;

            Title = "Scanbot SDK RTU UI Example";

            ContentView.AddContent(DocumentScanner.Instance);
            ContentView.AddContent(DataDetectors.Instance);

            Callback = new ScanResultCallback();
            Callback.Parent = this;

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

            foreach (ScannerButton button in ContentView.DataDetectors.Buttons)
            {
                button.Click += OnDataButtonClick;
            }

            ImagePicker.Instance.Controller.FinishedPickingMedia += ImageImported;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            foreach (ScannerButton button in ContentView.DocumentScanner.Buttons)
            {
                button.Click -= OnScannerButtonClick;
            }

            foreach (ScannerButton button in ContentView.DataDetectors.Buttons)
            {
                button.Click -= OnDataButtonClick;
            }

            ImagePicker.Instance.Controller.FinishedPickingMedia -= ImageImported;
        }

        private void OnScannerButtonClick(object sender, EventArgs e)
        {
            var button = (ScannerButton)sender;

            if (button.Data.Code == ListItemCode.ScanDocument)
            {

            }
            else if (button.Data.Code == ListItemCode.ImportImage)
            {
                ImagePicker.Instance.Present(this);
            }
            else if (button.Data.Code == ListItemCode.ViewImages)
            {

            }
        }

        private void ImageImported(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            var image = e.OriginalImage;
            ImagePicker.Instance.Dismiss();

            var controller = new CroppingController(image);
            PresentViewController(controller, true, null);

            controller.Finished += CroppingFinished;
        }

        private void CroppingFinished(object sender, CroppingEventArgs e)
        {
            (sender as CroppingController).Finished = null;
            var image = e.Image;
            Console.WriteLine("Image processed");
        }

        SBSDKPageAspectRatio[] MRZRatios = {
            new SBSDKPageAspectRatio(85.0, 54.0),
            new SBSDKPageAspectRatio(125.0, 88.0)
        };

        private void OnDataButtonClick(object sender, EventArgs e)
        {
            var button = (ScannerButton)sender;

            if (button.Data.Code == ListItemCode.ScanDC)
            {
                var ratios = new SBSDKPageAspectRatio[]
                {
                    // DC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                    new SBSDKPageAspectRatio(148.0, 210.0),
                    // DC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
                    new SBSDKPageAspectRatio(148.0, 105.0)
                };

                var title = "Please align the DC form in the frame.";
                var name = "DisabilityCertificateFlow";

                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanDisabilityCertificateWorkflowStep(
                        title, "", ratios, true, OnWorkflowStepResult
                    )
                };

                PresentController(name, steps);
            }
            else if (button.Data.Code == ListItemCode.ScanMRZImage)
            {
                var title = "Please align the Machine readable card with the form in the frame";
                var name = "MRZScanFlow";

                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanMachineReadableZoneWorkflowStep(
                        title, "", MRZRatios, true, OnWorkflowStepResult
                    )
                };

                PresentController(name, steps);
            }
            else if (button.Data.Code == ListItemCode.ScanMRZFrontBack)
            {
                var name = "MRZBackFrontScanFlow";

                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanMachineReadableZoneWorkflowStep(
                        "Scan 1/2", "Please scan the back side of your ID card", MRZRatios, true, OnWorkflowStepResult
                    ),
                    new SBSDKUIScanDocumentPageWorkflowStep(
                        "Scan 2/2", "Please scan the front side of your ID card", null, null, null
                    )
                };

                PresentController(name, steps);
            }
            else if (button.Data.Code == ListItemCode.ScanSEPA)
            {
                var name = "SEPAScanFlow";
                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanPayFormWorkflowStep(
                        "Please scan a SEPA PayForm", null, true, null
                    )
                };

                PresentController(name, steps);

            }
            else if (button.Data.Code == ListItemCode.ScanQRBar)
            {
                var name = "QRCodeScanFlow";
                var types = new string[] { AVConstants.AVMetadataObjectTypeQRCode.ToString() };
                var steps = new SBSDKUIWorkflowStep[]
                {
                    new SBSDKUIScanBarCodeWorkflowStep(
                        "Scan your QR code", "", types, new CGSize(1, 1), null)
                };

                PresentController(name, steps);
            }
        }

        void PresentController(string name, SBSDKUIWorkflowStep[] steps, SBSDKUIWorkflowScannerConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
            }

            SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(steps, name, null);

            var config = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
            var controller = SBSDKUIWorkflowScannerViewController.CreateNewWithWorkflow(workflow, config, Callback);
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

        // Results (both success and error) can be handled here or in the callback class.
        // To separate logic, we use ScanResultCallback.cs for processing results
        private NSError OnWorkflowStepResult(SBSDKUIWorkflowStepResult result)
        {
            return null;
        }
    }

}
