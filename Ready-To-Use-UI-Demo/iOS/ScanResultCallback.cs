
using ReadyToUseUIDemo.iOS.Controller;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS
{
    public class ScanResultCallback : SBSDKUIWorkflowScannerViewControllerDelegate
    {
        public UIViewController Parent { get; set; }

        public override void WorkflowScanViewController(SBSDKUIWorkflowScannerViewController viewController,
            SBSDKUIWorkflow workflow, SBSDKUIWorkflowStepResult[] results)
        {
            if (results.Length == 0)
            {
                string text = "No results to process, please try again";
                PresentResultPopup(viewController, text);
                return;
            }

            // Default content, if everything else fails
            var result = "Unable to process result, please try again";

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

            PresentResultPopup(viewController, result);
        }

        void PresentResultPopup(UIViewController current, string content)
        {
            var popover = new PopupController(content);
            current.DismissViewController(false, delegate
            {
                Parent.PresentViewController(popover, true, delegate {
                    popover.Content.CloseButton.Click += delegate {
                        popover.Dismiss();
                    };
                    popover.Content.CopyButton.Click += delegate {
                        // TODO Clipboard copy
                        popover.Dismiss();
                    };

                });
            });
        }

    }
}
