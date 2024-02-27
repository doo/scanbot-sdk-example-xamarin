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
                CommonUtils.ShowAlert("Recognized check", result.StringRepresentation, this);
            }));
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

