
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

                var steps = new SBSDKUIWorkflowStep[1];
                steps[0] = new SBSDKUIScanDisabilityCertificateWorkflowStep(
                    "Please align the DC form in the frame.",
                    "",
                    ratios.ToArray(),
                    true,
                    (action) =>
                        {
                            return null;
                        }
                    );
                var name = "DisabilityCertificateFlow";
                
                SBSDKUIWorkflow workflow = new SBSDKUIWorkflow(steps, name, (action) => {
                    return null;
                });
                
                var config = SBSDKUIWorkflowScannerConfiguration.DefaultConfiguration;
                var controller = SBSDKUIWorkflowScannerViewController.CreateNewWithWorkflow(workflow, config, Callback);
                controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(controller, false, null);
            }
        }
    }

    public class ScanResultCallback : SBSDKUIWorkflowScannerViewControllerDelegate
    {
        public UIViewController Parent { get; set; }

        public override void WorkflowScanViewController(SBSDKUIWorkflowScannerViewController viewController,
            SBSDKUIWorkflow workflow, SBSDKUIWorkflowStepResult[] results)
        {
            if (results.Length == 0)
            {
                return;
            }

            var result = "";

            if (results[0].DisabilityCertificateResult != null)
            {
                if (!results[0].DisabilityCertificateResult.RecognitionSuccessful)
                {
                    result = "Recognition failed";
                }
                else
                {
                    result = results[0].DisabilityCertificateResult.StringRepresentation;
                }
            }

            var popover = new PopupController(result);
            viewController.DismissViewController(false, delegate
            {
                Parent.PresentViewController(popover, true, delegate { });
            });
            
        }
    }

    public class PopupController : UIViewController
    {
        public UILabel Content { get; set; }
        string text;

        public PopupController(string text)
        {
            this.text = text;
            ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem
            {
                Title = "Close"
            };

            NavigationItem.RightBarButtonItem = new UIBarButtonItem
            {
                Title = "Copy"
            };

            Content = new UILabel();
            Content.Lines = 0;
            Content.LineBreakMode = UILineBreakMode.WordWrap;
            Content.BackgroundColor = UIColor.White;
            Content.Layer.CornerRadius = 5;

            nfloat hPadding = 20;
            nfloat vPadding = View.Frame.Height / 6;

            nfloat x = hPadding;
            nfloat y = vPadding;
            nfloat w = View.Frame.Width - 2 * hPadding;
            nfloat h = View.Frame.Height - 2 * vPadding;

            Content.Frame = new CGRect(x, y, w, h);
            Content.Text = text;

            View.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.5f);
            View.AddSubview(Content);
        }
    }
}
