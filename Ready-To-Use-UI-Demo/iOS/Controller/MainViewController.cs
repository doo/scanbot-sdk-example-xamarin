
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
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
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            foreach (ScannerButton button in ContentView.DocumentScanner.Buttons)
            {
                button.Click += OnScannerButtonClick;
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

            foreach (ScannerButton button in ContentView.DataDetectors.Buttons)
            {
                button.Click -= OnDataButtonClick;
            }
        }

        private void OnScannerButtonClick(object sender, EventArgs e)
        {

        }

        private void OnDataButtonClick(object sender, EventArgs e)
        {
            var button = (ScannerButton)sender;

            if (button.Data.Code == ListItemCode.ScanDC)
            {                
                var ratios = new List<SBSDKPageAspectRatio>
                {
                    // DC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                    new SBSDKPageAspectRatio(148.0, 210.0),
                    // DC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
                    new SBSDKPageAspectRatio(148.0, 105.0)
                };

                var title = "Please align the DC form in the frame.";
                var body = "";
                var name = "DisabilityCertificateFlow";

                var steps = new List<SBSDKUIWorkflowStep>
                {
                    new SBSDKUIScanDisabilityCertificateWorkflowStep(
                        title, body, ratios.ToArray(), true, OnWorkflowStepResult
                    )
                }.ToArray();

                SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(steps, name, OnWorkflowStepListResult);
                
                var config = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIWorkflowScannerViewController.CreateNewWithWorkflow(workflow, config, Callback);
                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
            }
        }

        // Errors are handled in ScanResultCallback, can leave this empty
        private NSError OnWorkflowStepResult(SBSDKUIWorkflowStepResult result)
        {
            return null;
        }

        // Errors are handled in ScanResultCallback, can leave this empty
        private NSError OnWorkflowStepListResult(SBSDKUIWorkflowStepResult[] result)
        {
            return null;
        }

    }

}
