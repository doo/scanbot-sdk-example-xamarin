using Foundation;
using UIKit;

using ScanbotSDK.iOS;

namespace scanbotsdkexamplexamarin.iOS
{
    public abstract class CroppingDemoDelegate
    {
        public abstract void CropViewControllerDidFinish(UIImage croppedImage);
    }

    public class CroppingDemoNavigationController : UINavigationController
    {
        UIImage Image;

        SBSDKCropViewController sdkCropViewController;

        public CroppingDemoDelegate croppingDelegate;

        public CroppingDemoNavigationController(UIImage image)
        {
            Image = image;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            sdkCropViewController = new SBSDKCropViewController();
            sdkCropViewController.Image = Image;
            sdkCropViewController.WeakDelegate = this;

            if (sdkCropViewController.Polygon == null)
            {
                // if no polygon was detected, we set a default polygon
                sdkCropViewController.Polygon = new SBSDKPolygon(); // {0,0}, {1,0}, {1,1}, {0,1}
            }

            PushViewController(sdkCropViewController, false);
        }


        #region SBSDKCropViewControllerDelegate

        [Export("cropViewController:didApplyChangesWithPolygon:croppedImage:")]
        public void CropViewControllerDidApplyChangesWithPolygon(SBSDKCropViewController cropViewController, SBSDKPolygon polygon, UIImage croppedImage)
        {
            if (croppingDelegate != null)
            {
                croppingDelegate.CropViewControllerDidFinish(croppedImage);
            }

            DismissViewController(true, null);
        }

        [Export("cropViewControllerDidCancelChanges:")]
        public void CropViewControllerDidCancelChanges(SBSDKCropViewController cropViewController)
        {
            DismissViewController(true, null);
        }

        [Export("cancelButtonImageForCropViewController:")]
        public UIImage CancelButtonImageForCropViewController(SBSDKCropViewController cropViewController)
        {
            return UIImage.FromBundle("ui_action_close");
        }

        [Export("applyButtonImageForCropViewController:")]
        public UIImage ApplyButtonImageForCropViewController(SBSDKCropViewController cropViewController)
        {
            return UIImage.FromBundle("ui_action_checkmark");
        }

        #endregion

    }
}
