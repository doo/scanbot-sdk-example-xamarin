using UIKit;
using Foundation;

using ScanbotSDK.iOS;
using CoreGraphics;
using System;
using System.Reflection;
using AVFoundation;
using CoreMedia;

namespace ClassicalComponentsDemo.iOS
{
    public abstract class CameraDemoDelegate
    {
        public abstract void DidCaptureDocumentImage(UIImage documentImage);
        public abstract void DidCaptureOriginalImage(UIImage originalImage);
    }

    public class CameraDemoViewController : UIViewController
    {
        protected UIView scanningContainerView;
        protected UIView bottomButtonsContainer;

        protected SBSDKScannerViewController scannerViewController;

        protected UIButton flashButton;
        protected UIButton autoSnapButton;
        protected bool autoSnappingEnabled = true;

        protected bool viewAppeared;

        public CameraDemoDelegate cameraDelegate;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var screenSize = UIScreen.MainScreen.Bounds.Size;

            // Create a view as container for bottom buttons:
            var buttonsContainerHeight = 120;
            bottomButtonsContainer = new UIView(new CGRect(0, screenSize.Height - buttonsContainerHeight, screenSize.Width, buttonsContainerHeight));
            bottomButtonsContainer.BackgroundColor = UIColor.Blue;
            View.AddSubview(bottomButtonsContainer);

            // Create a view as container to embed the Scanbot SDK SBSDKScannerViewController:
            scanningContainerView = new UIView(new CGRect(0, 0, screenSize.Width, screenSize.Height - buttonsContainerHeight));
            View.AddSubview(scanningContainerView);

            // Create the SBSDKScannerViewController, embedded into our custom scanningContainerView.
            // As we do not want automatic image storage we pass null here as image storage.
            scannerViewController = new SBSDKScannerViewController(this, scanningContainerView, null, false);

            // =================================================================
            //
            // UI customizations can be implemented via delegate methods from "SBSDKScannerViewControllerDelegate".
            // See some examples below the #region SBSDKScannerViewControllerDelegate
            //
            // Please see the API docs of our native Scanbot SDK for iOS, since all those methods and properties
            // are also available as Scanbot Xamarin bindings.
            //
            // =================================================================

            // Set the delegate to self.
            scannerViewController.WeakDelegate = this;

            // We want unscaled images in full size:
            scannerViewController.ImageScale = 1.0f;

            // The minimum score in percent (0 - 100) of the perspective distortion to accept a detected document. 
            // Default is 75.0. Set lower values to accept more perspective distortion. Warning: Lower values result in more blurred document images.
            scannerViewController.AcceptedAngleScore = 70;

            // The minimum size in percent (0 - 100) of the screen size to accept a detected document. It is sufficient that height or width match the score. 
            // Default is 80.0. Warning: Lower values result in low resolution document images.
            scannerViewController.AcceptedSizeScore = 80;

            // Sensitivity factor for automatic capturing. Must be in the range [0.0...1.0]. Invalid values are threated as 1.0. 
            // Defaults to 0.66 (1 sec).s A value of 1.0 triggers automatic capturing immediately, a value of 0.0 delays the automatic by 3 seconds.
            scannerViewController.AutoCaptureSensitivity = 0.7f;

            // The orientation captured images are locked onto. By default it is SBSDKOrientationLockNone. 
            // Setting this property to any other value will suppress the document detection status ‘SBSDKDocumentDetectionStatusOK_BadAspectRatio’. 
            // If the lock is enabled the detection status UI will be orientation-locked too.
            scannerViewController.ImageOrientationLock = SBSDKOrientationLock.Portrait;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            AddAutosnapToggleButton();
            SetAutoSnapEnabled(autoSnappingEnabled);
            AddFlashToggleButton();
            SetupDefaultShutterButtonColors();
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
            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.AllButUpsideDown;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            // White statusbar
            return UIStatusBarStyle.LightContent;
        }

        void SetupDefaultShutterButtonColors()
        {
            var shutterButton = scannerViewController.DefaultShutterButton as SBSDKShutterButton;
            shutterButton.ButtonSearchingColor = UIColor.Red;
            shutterButton.ButtonDetectedColor = UIColor.Green;
        }

        void AddAutosnapToggleButton()
        {
            autoSnapButton = new UIButton(new CGRect(40, bottomButtonsContainer.Frame.Height - 80, 40, 40));
            autoSnapButton.AddTarget(delegate
            {
                autoSnappingEnabled = !autoSnappingEnabled;
                SetAutoSnapEnabled(autoSnappingEnabled);
            }, UIControlEvent.TouchUpInside);

            autoSnapButton.SetImage(UIImage.FromBundle("ui_autosnap_off"), UIControlState.Normal);
            autoSnapButton.SetImage(UIImage.FromBundle("ui_autosnap_on"), UIControlState.Selected);

            bottomButtonsContainer.AddSubview(autoSnapButton);
            bottomButtonsContainer.BringSubviewToFront(autoSnapButton);
        }

        void AddFlashToggleButton()
        {
            flashButton = new UIButton(new CGRect(bottomButtonsContainer.Frame.Width - 80, bottomButtonsContainer.Frame.Height - 80, 40, 40));
            flashButton.AddTarget(delegate
            {
                scannerViewController.CameraSession.TorchLightEnabled = !scannerViewController.CameraSession.TorchLightEnabled;
                flashButton.Selected = scannerViewController.CameraSession.TorchLightEnabled;
            }, UIControlEvent.TouchUpInside);

            flashButton.SetImage(UIImage.FromBundle("ui_flash_off"), UIControlState.Normal);
            flashButton.SetImage(UIImage.FromBundle("ui_flash_on"), UIControlState.Selected);

            flashButton.Selected = scannerViewController.CameraSession.TorchLightEnabled;

            bottomButtonsContainer.AddSubview(flashButton);
            bottomButtonsContainer.BringSubviewToFront(flashButton);
        }

        void SetAutoSnapEnabled(bool enabled)
        {
            autoSnapButton.Selected = enabled;
            scannerViewController.ShutterMode = enabled ? SBSDKShutterMode.Smart : SBSDKShutterMode.AlwaysManual;
            scannerViewController.DetectionStatusHidden = !enabled;
            (scannerViewController.DefaultShutterButton as SBSDKShutterButton).ScannerStatus = enabled ? SBSDKScannerStatus.Scanning : SBSDKScannerStatus.Idle;
        }


        // =====================================================================
        // 
        // Implementation of some delegate methods from "SBSDKScannerViewControllerDelegate":
        // 
        #region SBSDKScannerViewControllerDelegate

        [Export("scannerControllerShouldAnalyseVideoFrame:")]
        public bool ShouldAnalyseVideoFrame(SBSDKScannerViewController controller)
        {
            return autoSnappingEnabled && viewAppeared && PresentedViewController == null;
        }

        [Export("scannerController:didCaptureDocumentImage:")]
        public void DidCaptureDocumentImage(SBSDKScannerViewController controller, UIImage documentImage)
        {
            // Here we get the cropped and perspective corrected document image.
            if (cameraDelegate != null)
            {
                cameraDelegate.DidCaptureDocumentImage(documentImage);
            }

            NavigationController.PopToRootViewController(true);
        }

        [Export("scannerController:didCaptureImage:withDetectedPolygon:lensCameraProperties:")]
        public void DidCaptureImageWithDetectedPolygon(SBSDKScannerViewController controller, UIImage originalImage, SBSDKPolygon polygon, SBSDKLensCameraProperties properties)
        {
            // Here we get the original (uncropped) image from the camera and an optional polygon that was detected on the image.
            if (cameraDelegate != null)
            {
                cameraDelegate.DidCaptureOriginalImage(originalImage);
            }
        }

        [Export("scannerController:didDetectPolygon:withStatus:")]
        public void DidDetectPolygonWithStatus(SBSDKScannerViewController controller, SBSDKPolygon polygon, SBSDKDocumentDetectionStatus status)
        {
            // Everytime the document detector finishes detection it calls this delegate method.
        }

        [Export("scannerController:viewForDetectionStatus:")]
        public UIView GetViewForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // Alternative method to "scannerController:localizedTextForDetectionStatus:".
            // Here you can return a custom view that you want to use to visualize the latest detection status.

            var label = new SBSDKDetectionStatusLabel();
            label.BackgroundColor = UIColor.Red;
            label.TextColor = UIColor.White;

            if (scannerViewController.EnergySavingActive)
            {
                label.Text = "Energy Saving active.\nMove your device.";
                label.BackgroundColor = UIColor.Orange;
            }
            else
            {
                switch (status)
                {
                    case SBSDKDocumentDetectionStatus.Ok:
                        label.Text = "Don't move.\nCapturing...";
                        label.BackgroundColor = UIColor.Green;
                        break;
                    case SBSDKDocumentDetectionStatus.OK_SmallSize:
                        label.Text = "Move closer";
                        break;
                    case SBSDKDocumentDetectionStatus.OK_BadAngles:
                        label.Text = "Perspective";
                        break;
                    case SBSDKDocumentDetectionStatus.Error_NothingDetected:
                        label.Text = "No Document";
                        break;
                    case SBSDKDocumentDetectionStatus.Error_Noise:
                        label.Text = "Background too noisy";
                        break;
                    case SBSDKDocumentDetectionStatus.Error_Brightness:
                        label.Text = "Poor light";
                        break;
                    case SBSDKDocumentDetectionStatus.OK_BadAspectRatio:
                        label.Text = "Wrong aspect ratio.\n Rotate your device";
                        break;
                    default:
                        return null;
                }
            }

            label.SizeToFit();
            return label;
        }

        [Export("scannerController:polygonColorForDetectionStatus:")]
        public UIColor GetPolygonColorForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // If the detector has found an acceptable polygon we show it with green color
            if (status == SBSDKDocumentDetectionStatus.Ok)
            {
                return UIColor.Green;
            }

            return UIColor.Red;
        }

        [Export("scannerController:localizedTextForDetectionStatus:")]
        public string GetLocalizedTextForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // Alternative method to "scannerController:viewForDetectionStatus:"
            // Here you can return just the localized text for the status label depending on the detection status.
            return null;
        }

        [Export("scannerController:shouldAutocropCapturedImageWithMode:manualShutter:")]
        public bool ShouldAutocropCapturedImageWithModeManualShutter(SBSDKScannerViewController controller, SBSDKShutterMode mode, bool manual)
        {
            // Here you control whether to automatically crop the document image or not, 
            // depending on the current shutter mode and how the shutter was released: manually or automatically.
            // Return true, if the detected polygon should be applied to the captured document image, false otherwise.
            return true;
        }

        [Export("scannerControllerCustomShutterButton:")]
        public UIButton GetCustomShutterButton(SBSDKScannerViewController controller)
        {
            // Optional delegate for returning a custom shutter button.
            //return customShutterButton;
            return null;
        }

        [Export("scannerControllerSuperViewForShutterButton:")]
        public UIView GetSuperViewForShutterButton(SBSDKScannerViewController controller)
        {
            // Optional delegate for returning a custom view on which to place the shutter button.
            return bottomButtonsContainer;
        }

        [Export("scannerControllerCenterForShutterButton:")]
        public CGPoint GetCenterForShutterButton(SBSDKScannerViewController controller)
        {
            return new CGPoint(bottomButtonsContainer.Frame.Width / 2, bottomButtonsContainer.Frame.Height / 2);
        }

        #endregion

    }
}
