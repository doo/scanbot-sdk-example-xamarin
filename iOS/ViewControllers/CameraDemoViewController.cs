using UIKit;
using Foundation;

using ScanbotSDK.iOS;
using CoreGraphics;

namespace scanbotsdkexamplexamarin.iOS
{
    public abstract class CameraDemoDelegate
    {
        public abstract void DidCaptureDocumentImage(UIImage documentImage);
        public abstract void DidCaptureOriginalImage(UIImage originalImage);
    }

    public class CameraDemoViewController : UIViewController
    {
        protected SBSDKScannerViewController scannerViewController;

        protected UIButton flashButton;

        protected bool viewAppeared;

        public CameraDemoDelegate cameraDelegate;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the SBSDKScannerViewController.
            // We want it to be embedded into self.
            // As we do not want automatic image storage we pass nil here for the image storage.
            scannerViewController = new SBSDKScannerViewController(this, null);

            // Set the delegate to self.
            scannerViewController.WeakDelegate = this;

            // Please check the API docs of our native Scanbot SDK for iOS, since all methods and properties also also available in Xamarin bindings:
            // https://scanbotsdk.github.io/documentation/ios/html/interface_s_b_s_d_k_scanner_view_controller.html

            // We want unscaled images in full size:
            scannerViewController.ImageScale = 1.0f;

            // The minimum score in percent (0 - 100) of the perspective distortion to accept a detected document. 
            // Default is 75.0. Set lower values to accept more perspective distortion. Warning: Lower values result in more blurred document images.
            scannerViewController.AcceptedAngleScore = 65;

            // The minimum size in percent (0 - 100) of the screen size to accept a detected document. It is sufficient that height or width match the score. 
            // Default is 80.0. Warning: Lower values result in low resolution document images.
            scannerViewController.AcceptedSizeScore = 70;

            // Sensitivity factor for automatic capturing. Must be in the range [0.0...1.0]. Invalid values are threated as 1.0. 
            // Defaults to 0.66 (1 sec).s A value of 1.0 triggers automatic capturing immediately, a value of 0.0 delays the automatic by 3 seconds.
            scannerViewController.AutoCaptureSensitivity = 0.7f;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            PlaceFlashButton();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            viewAppeared = false;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            viewAppeared = true;
        }

        public override bool ShouldAutorotate()
        {
            // No autorotations
            return false;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // Only portrait
            return UIInterfaceOrientationMask.Portrait;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            // White statusbar
            return UIStatusBarStyle.LightContent;
        }

        void PlaceFlashButton()
        {
            CGSize screenSize = UIScreen.MainScreen.Bounds.Size;
            CGRect buttonFrame = new CGRect(screenSize.Width - 80, screenSize.Height - 100, 40, 40);

		    if (flashButton == null)
            {
                flashButton = new UIButton(buttonFrame);
                flashButton.AddTarget(delegate {
                    scannerViewController.CameraSession.TorchLightEnabled = !scannerViewController.CameraSession.TorchLightEnabled;
                    flashButton.Selected = scannerViewController.CameraSession.TorchLightEnabled;
                }, UIControlEvent.TouchUpInside);

                flashButton.SetImage(UIImage.FromBundle("ui_flash_off"), UIControlState.Normal);
                flashButton.SetImage(UIImage.FromBundle("ui_flash_on"), UIControlState.Selected);

                flashButton.Selected = scannerViewController.CameraSession.TorchLightEnabled;
            }
            else 
            {
                flashButton.Frame = buttonFrame;
		    }
            View.AddSubview(flashButton);
            View.BringSubviewToFront(flashButton);
        }


        // Exports of SBSDKScannerViewControllerDelegate for WeakDelegate ...
        #region SBSDKScannerViewControllerDelegate

        [Export("scannerControllerShouldAnalyseVideoFrame:")]
        public bool ScannerControllerShouldAnalyseVideoFrame(SBSDKScannerViewController controller)
        {
            // We want to only process video frames when self is visible on screen and front most view controller
            return viewAppeared && PresentedViewController == null;
        }

        [Export("scannerController:didCaptureDocumentImage:")]
        public void ScannerControllerDidCaptureDocumentImage(SBSDKScannerViewController controller, UIImage documentImage)
        {
            // Here we get the perspective corrected and cropped document image after the shutter was (auto)released.
            if (cameraDelegate != null)
            {
                cameraDelegate.DidCaptureDocumentImage(documentImage);
            }

            NavigationController.PopToRootViewController(true);
        }

        [Export("scannerController:didCaptureImage:")]
        public void ScannerControllerDidCaptureImage(SBSDKScannerViewController controller, UIImage image)
        {
            // Here we get the full image from the camera. We could run another manual detection here or use the latest
            // detected polygon from the video stream to process the image with.

            if (cameraDelegate != null)
            {
                cameraDelegate.DidCaptureOriginalImage(image);
            }
        }

        [Export("scannerController:didDetectPolygon:withStatus:")]
        public void ScannerControllerDidDetectPolygonWithStatus(SBSDKScannerViewController controller, SBSDKPolygon polygon, SBSDKDocumentDetectionStatus status)
        {
            // Everytime the document detector finishes detection it calls this delegate method.
        }

        [Export("scannerController:viewForDetectionStatus:")]
        public UIView ScannerControllerViewForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // Here we can return a custom view that we want to use to visualize the latest detection status.
            // We return null for now to use the standard label.
            return null;
        }

        [Export("scannerController:polygonColorForDetectionStatus:")]
        public UIColor ScannerControllerPolygonColorForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // If the detector has found an acceptable polygon we show it with green color
            if (status == SBSDKDocumentDetectionStatus.Ok)
            {
                return UIColor.Green;
            }

            return UIColor.Red;
        }

        [Export("scannerController:localizedTextForDetectionStatus:")]
        public string ScannerControllerLocalizedTextForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // here we can return localized text for the status label

            switch (status)
            {
                case SBSDKDocumentDetectionStatus.Ok:
                    return "Don't move";
                case SBSDKDocumentDetectionStatus.OK_SmallSize:
                    return "Move closer";
                case SBSDKDocumentDetectionStatus.OK_BadAngles:
                    return "Perspective";
                case SBSDKDocumentDetectionStatus.Error_NothingDetected:
                    return "No Document";
                case SBSDKDocumentDetectionStatus.Error_Noise:
                    return "Background too noisy";
                case SBSDKDocumentDetectionStatus.Error_Brightness:
                    return "Poor light";
                default:
                    return null;
            }
        }

        [Export("scannerController:shouldAutocropCapturedImageWithMode:manualShutter:")]
        public bool ScannerControllerShouldAutocropCapturedImageWithModeManualShutter(SBSDKScannerViewController controller, SBSDKShutterMode mode, bool manual)
        {
            // Here you control whether to automatically crop the document image or not, 
            // depending on the current shutter mode and how the shutter was released: manually or automatically.
            // Return true, if the detected polygon should be applied to the captured document image, false otherwise.
            return true;
        }

        #endregion

    }
}
