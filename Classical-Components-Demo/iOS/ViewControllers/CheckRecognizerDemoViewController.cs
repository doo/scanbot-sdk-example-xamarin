using System;
using ScanbotSDK.iOS;
using UIKit;

namespace ClassicalComponentsDemo.iOS.ViewControllers
{
    public class CheckRecognizerDemoViewController: UIViewController
    {
        private SBSDKCheckRecognizerViewController recognizerViewController;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            recognizerViewController = new SBSDKCheckRecognizerViewController(this, View, new Delegate((result) => {
                ShowResult(result);
            }));
        }

        private void ShowResult(SBSDKCheckRecognizerResult result)
        {
            var alert = UIAlertController.Create("Recognized check", result.StringRepresentation, UIAlertControllerStyle.Alert);
            var okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, delegate {
                if (alert.PresentedViewController is UIViewController controller)
                {
                    controller.DismissViewController(true, null);
                }
            });

            alert.AddAction(okAction);

            PresentViewController(alert, true, null);
        }

        private class Delegate : SBSDKCheckRecognizerViewControllerDelegate
        {
            private Action<SBSDKCheckRecognizerResult> action;
            public Delegate(Action<SBSDKCheckRecognizerResult> action) { this.action = action; }
            public override void DidRecognizeCheck(SBSDKCheckRecognizerViewController controller, SBSDKCheckRecognizerResult result)
            {
                action.Invoke(result);
            }
        }
    }
}

