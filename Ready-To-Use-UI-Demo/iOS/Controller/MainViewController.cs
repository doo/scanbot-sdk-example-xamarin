
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

            foreach (ScannerButton button in ContentView.DataDetectors.Buttons)
            {
                button.Click -= OnDataButtonClick;
            }

            CameraCallback.Selected -= OnScanComplete;
        }

        private void OnScanComplete(object sender, PageEventArgs e)
        {
            PageRepository.Add(e.Page);
            OpenImageListController();
        }

        private void ShowPage(SBSDKUIPage changedPage)
        {
            PageRepository.Update(changedPage);
        }

        class CroppingDelegate : SBSDKUICroppingViewControllerDelegate
        {
            MainViewController parent;

            public CroppingDelegate(MainViewController parent)
            {
                this.parent = parent;
            }
            public override void DidFinish(SBSDKUICroppingViewController viewController, SBSDKUIPage changedPage)
            {
                parent.ShowPage(changedPage);
            }
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
                // Hide multi page button to keep this example simpler
                config.UiConfiguration.MultiPageButtonHidden = true;

                var controller = SBSDKUIDocumentScannerViewController.CreateNewWithConfiguration(config, CameraCallback);
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

        SBSDKPageAspectRatio[] MRZRatios = {
            new SBSDKPageAspectRatio(85.0, 54.0),
            //new SBSDKPageAspectRatio(125.0, 88.0)
        };

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
                var config = SBSDKUIMachineCodeScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIMRZScannerViewController.CreateNewWithConfiguration(config, Delegates.MRZ);
                PresentViewController(controller, true, null);
            }
            else if (button.Data.Code == ListItemCode.WorkflowDC)
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
                        title, "", ratios, true, WorkflowStepValidator.OnDCFormStep
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
                        "Step 1/2", "Please scan the front side of your ID card", MRZRatios, true, false, null, WorkflowStepValidator.OnIDCardFrontStep
                        ),
                    new SBSDKUIScanMachineReadableZoneWorkflowStep(
                        "Step 2/2", "Please scan the back side of your ID card", MRZRatios, true, WorkflowStepValidator.OnIDCardBackStep
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
                    new SBSDKUIScanBarCodeWorkflowStep(
                        "Scan your QR code", "", types, new CGSize(1, 1), WorkflowStepValidator.OnBarCodeStep)
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
            
            var controller = SBSDKUIWorkflowScannerViewController.CreateNewWithWorkflow(workflow, config, null);
            WorkflowStepValidator.WorkflowController = controller;
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

    }

    public class PageEventArgs : EventArgs
    {
        public SBSDKUIPage Page { get; set; }
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

            Selected?.Invoke(this, new PageEventArgs { Page = pages[0] });
        }
    }
}
