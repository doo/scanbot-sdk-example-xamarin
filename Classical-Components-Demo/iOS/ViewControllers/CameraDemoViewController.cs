using UIKit;
using ScanbotSDK.iOS;
using CoreGraphics;
using System.Collections.Generic;

namespace ClassicalComponentsDemo.iOS
{
    public abstract class CameraDemoDelegate
    {
        public abstract void DidCaptureDocumentImage(UIImage documentImage);
        public abstract void DidCaptureOriginalImage(UIImage originalImage);
    }

    public interface IDocumentCaptureInteraction
    {
        void DidDetectDocument(UIImage documentImage, UIImage originalImage, SBSDKDocumentDetectorResult result, bool autoSnapped);
    }

    public class CameraDemoViewController : UIViewController, IDocumentCaptureInteraction
    {
        protected UIView scanningContainerView, bottomButtonsContainer;
        protected SBSDKDocumentScannerViewController documentScannerViewController;
        protected UIButton flashButton, autoSnapButton;

        protected bool autoSnappingEnabled = true;
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

            // Create a view as container to embed the Scanbot SDK SBSDKDocumentScannerViewController:
            scanningContainerView = new UIView(new CGRect(0, 0, screenSize.Width, screenSize.Height - buttonsContainerHeight));
            View.AddSubview(scanningContainerView);

            documentScannerViewController = new SBSDKDocumentScannerViewController(this, scanningContainerView, new DocumentScannerDelegate(this));

            // ==================================================================================================
            // Please see the API docs of our native Scanbot SDK for iOS, since all those methods and properties
            // are also available as Scanbot Xamarin Native bindings.
            // ==================================================================================================

            // We want unscaled images in full size:
            documentScannerViewController.ImageScale = 1.0f;

            // The minimum score in percent (0 - 100) of the perspective distortion to accept a detected document. 
            // Default is 75.0. Set lower values to accept more perspective distortion. Warning: Lower values result in more blurred document images.
            documentScannerViewController.AcceptedAngleScore = 70;

            // The minimum size in percent (0 - 100) of the screen size to accept a detected document. It is sufficient that height or width match the score. 
            // Default is 80.0. Warning: Lower values result in low resolution document images.
            documentScannerViewController.AcceptedSizeScore = 80;

            // Sensitivity factor for automatic capturing. Must be in the range [0.0...1.0]. Invalid values are threated as 1.0. 
            // Defaults to 0.66 (1 sec).s A value of 1.0 triggers automatic capturing immediately, a value of 0.0 delays the automatic by 3 seconds.
            documentScannerViewController.AutoSnappingSensitivity = 0.7f;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            AddAutosnapToggleButton();
            SetAutoSnapEnabled(autoSnappingEnabled);
            AddFlashToggleButton();
            SetupDefaultShutterButtonColors();
        }

        void SetupDefaultShutterButtonColors()
        {

            // CustomSnapButton: create a custom auto snap button in the View and pass it as a reference.
            var customButton = new UIButton(new CGRect(View.Center.X - 25, bottomButtonsContainer.Frame.Height - 80, 50, 50));
            customButton.BackgroundColor = UIColor.White;
            customButton.Layer.BorderColor = UIColor.Black.CGColor;
            customButton.Layer.BorderWidth = 5; 
            customButton.Layer.CornerRadius = 25;
            documentScannerViewController.CustomSnapButton = customButton;

            bottomButtonsContainer.AddSubview(customButton);
            bottomButtonsContainer.BringSubviewToFront(customButton);

            // uncomment below code to use the default snap button
            //var shutterButton = documentScannerViewController.SnapButton;
            //shutterButton.ButtonSearchingColor = UIColor.Red;
            //shutterButton.ButtonDetectedColor = UIColor.Green;
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
                documentScannerViewController.FlashLightEnabled = !documentScannerViewController.FlashLightEnabled;
                flashButton.Selected = documentScannerViewController.FlashLightEnabled;
            }, UIControlEvent.TouchUpInside);

            flashButton.SetImage(UIImage.FromBundle("ui_flash_off"), UIControlState.Normal);
            flashButton.SetImage(UIImage.FromBundle("ui_flash_on"), UIControlState.Selected);

            flashButton.Selected = documentScannerViewController.FlashLightEnabled;

            bottomButtonsContainer.AddSubview(flashButton);
            bottomButtonsContainer.BringSubviewToFront(flashButton);
        }

        void SetAutoSnapEnabled(bool enabled)
        {
            autoSnapButton.Selected = enabled;
            documentScannerViewController.AutoSnappingMode = enabled ? SBSDKAutosnappingMode.Enabled : SBSDKAutosnappingMode.Disabled;
            documentScannerViewController.SnapButton.ScannerStatus = enabled ? SBSDKScannerStatus.Scanning : SBSDKScannerStatus.Idle;

            // set the visibility for detection label.
            documentScannerViewController.SuppressDetectionStatusLabel = false;
        }

        public void DidDetectDocument(UIImage documentImage, UIImage originalImage, SBSDKDocumentDetectorResult result, bool autoSnapped)
        {
            if (documentImage != null)
            {
                cameraDelegate.DidCaptureDocumentImage(documentImage);
                this.NavigationController.PopViewController(true);
            }

            if (originalImage != null)
            {
                cameraDelegate.DidCaptureOriginalImage(originalImage);
            }
        }
    }

    // ================================================================================================
    // Implementation of a few delegate methods from "SBSDKDocumentScannerViewControllerDelegate"
    // ================================================================================================
    class DocumentScannerDelegate : SBSDKDocumentScannerViewControllerDelegate
    {
        private static UIColor successColor = UIColor.Green;
        private static UIColor warningColor = UIColor.Yellow;
        private static UIColor errorColor = UIColor.Red;
        private IDocumentCaptureInteraction documentCaptureInteraction;
        public DocumentScannerDelegate(IDocumentCaptureInteraction documentCaptureInteraction)
        {
            this.documentCaptureInteraction = documentCaptureInteraction;
        }

        // Validation method for allowing/restricting document detection.
        public override bool ShouldDetectDocument(SBSDKDocumentScannerViewController controller)
        {
            return true;
        }

        public override void WillSnapImage(SBSDKDocumentScannerViewController controller)
        {
            // this method is invoked before DidSnapDocumentImage(...)
        }

        // When the document is detected(image captured)
        public override void DidSnapDocumentImage(SBSDKDocumentScannerViewController controller, UIImage documentImage, UIImage originalImage, SBSDKDocumentDetectorResult result, bool autoSnapped)
        {
            // the "documentImage" is the detected image according to the polygon detection.
            // orignal image is the whole camera captured image, regardless of the polygon cropping.
            documentCaptureInteraction.DidDetectDocument(documentImage, originalImage, result, autoSnapped);
        }

        // Update the detection label on the Document polygon view according to the status.
        public override void ConfigureStatusDetectionLabel(SBSDKDocumentScannerViewController controller, SBSDKDetectionStatusLabel label, SBSDKDocumentDetectorResult result)
        {
            switch (result.Status)
            {
                case SBSDKDocumentDetectionStatus.Ok:
                    label.Text = "Don't move.\nCapturing...";
                    label.BackgroundColor = successColor;
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
            }
        }

        // Validation method, asking if auto snapping should be performed.
        public override bool ShouldAutoSnapImageWithForDetectionResult(SBSDKDocumentScannerViewController controller, SBSDKDocumentDetectorResult result)
        {
            return controller.AutoSnappingMode == SBSDKAutosnappingMode.Enabled;
        }

        // Fill color inside the detecting polygon view.
        public override UIColor PolygonFillColorForStatus(SBSDKDocumentScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            return statusDictionary.GetValueOrDefault(status).ColorWithAlpha(0.3f);
        }

        // Polygon Line/border color.
        public override UIColor PolygonLineColorForStatus(SBSDKDocumentScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            return statusDictionary.GetValueOrDefault(status);
        }

        public override void DidSampleVideoFrame(SBSDKDocumentScannerViewController controller, UIImage videoFrameImage, SBSDKDocumentDetectorResult result)
        {
            // Gets the video frame images. continuos callback
        }

        public Dictionary<SBSDKDocumentDetectionStatus, UIColor> statusDictionary = new Dictionary<SBSDKDocumentDetectionStatus, UIColor>
        {
            { SBSDKDocumentDetectionStatus.Ok,                      successColor },

            { SBSDKDocumentDetectionStatus.OK_SmallSize,            warningColor },
            { SBSDKDocumentDetectionStatus.OK_BadAngles,            warningColor },
            { SBSDKDocumentDetectionStatus.OK_BadAspectRatio,       warningColor },

            { SBSDKDocumentDetectionStatus.Error_NothingDetected,   errorColor },
            { SBSDKDocumentDetectionStatus.Error_Noise,             errorColor },
            { SBSDKDocumentDetectionStatus.Error_Brightness,        errorColor },
        };
    }
}
