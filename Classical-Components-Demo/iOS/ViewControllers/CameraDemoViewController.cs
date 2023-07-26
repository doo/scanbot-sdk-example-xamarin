using UIKit;
using ScanbotSDK.iOS;
using CoreGraphics;

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
        protected UIView scanningContainerView;
        protected UIView bottomButtonsContainer;

        protected SBSDKDocumentScannerViewController documentScannerViewController;

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
            documentScannerViewController = new SBSDKDocumentScannerViewController(this, scanningContainerView, new DocumentScannerDelegate(this));

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
            //documentScannerViewController.WeakDelegate = this;

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
            var shutterButton = documentScannerViewController.SnapButton;
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
            documentScannerViewController.HideDetectionStatusLabel = !enabled;
            documentScannerViewController.SnapButton.ScannerStatus = enabled ? SBSDKScannerStatus.Scanning : SBSDKScannerStatus.Idle;
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
                cameraDelegate.DidCaptureOriginalImage(documentImage);
            }
        }
    }

    // =====================================================================
    // 
    // Implementation of some delegate methods from "SBSDKScannerViewControllerDelegate":
    // 
    #region
    #endregion
    class DocumentScannerDelegate : SBSDKDocumentScannerViewControllerDelegate
    {
        private IDocumentCaptureInteraction documentCaptureInteraction;
        public DocumentScannerDelegate(IDocumentCaptureInteraction documentCaptureInteraction)
        {
            this.documentCaptureInteraction = documentCaptureInteraction;
        }

        public override void DidSnapDocumentImage(SBSDKDocumentScannerViewController controller, UIImage documentImage, UIImage originalImage, SBSDKDocumentDetectorResult result, bool autoSnapped)
        {
            documentCaptureInteraction.DidDetectDocument(documentImage, originalImage, result, autoSnapped);
        }
    }
}

