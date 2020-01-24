using System;
using System.Collections.Generic;
using ReadyToUseUIDemo.iOS.Controller;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Utils
{
    public class Delegates
    {
        public static MRZDelegate MRZ = new MRZDelegate();

        public class MRZDelegate : SBSDKUIMRZScannerViewControllerDelegate
        {
            
            public override void DidDetect(
                SBSDKUIMRZScannerViewController viewController, SBSDKMachineReadableZoneRecognizerResult zone)
            {
                string content = zone.StringRepresentation;
                var images = new List<UIImage>();
                var popover = new PopupController(content, images);

                viewController.PresentViewController(popover, true, delegate
                {
                    popover.Content.CloseButton.Click += delegate
                    {
                        popover.Dismiss();
                    };
                });
            }
        }
    }
}
