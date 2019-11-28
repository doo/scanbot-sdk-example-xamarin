using System;
using Foundation;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class CroppingEventArgs : EventArgs
    {
        public UIImage Image { get; set; }

        public SBSDKPolygon Polygon { get; set; }
    }

    public class CroppingController : UINavigationController
    {
        public EventHandler<CroppingEventArgs> Finished;

        UIImage Image;

        SBSDKImageEditingViewController imageEditingViewController;

        public CroppingController(UIImage image)
        {
            Image = image;

            ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            NavigationBar.BarStyle = UIBarStyle.Black;
            NavigationBar.TintColor = UIColor.White;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            imageEditingViewController = new SBSDKImageEditingViewController();
            imageEditingViewController.Image = Image;
            imageEditingViewController.WeakDelegate = this;

            if (imageEditingViewController.Polygon == null)
            {
                // if no polygon was detected, we set a default polygon
                imageEditingViewController.Polygon = new SBSDKPolygon(); // {0,0}, {1,0}, {1,1}, {0,1}
            }

            PushViewController(imageEditingViewController, false);
        }


        #region SBSDKImageEditingViewControllerDelegate

        [Export("imageEditingViewControllerToolbarStyle:")]
        public UIBarStyle ImageEditingViewControllerToolbarStyle(SBSDKImageEditingViewController editingViewController)
        {
            return UIBarStyle.Default;
        }

        [Export("imageEditingViewControllerToolbarItemTintColor:")]
        public UIColor ImageEditingViewControllerToolbarItemTintColor(SBSDKImageEditingViewController editingViewController)
        {
            return UIColor.White;
        }

        [Export("imageEditingViewControllerToolbarTintColor:")]
        public UIColor ImageEditingViewControllerToolbarTintColor(SBSDKImageEditingViewController editingViewController)
        {
            return UIColor.Black;
        }

        [Export("imageEditingViewController:didApplyChangesWithPolygon:croppedImage:")]
        public void ImageEditingViewController(SBSDKImageEditingViewController editingViewController, SBSDKPolygon polygon, UIImage croppedImage)
        {
            Finished?.Invoke(this, new CroppingEventArgs { Image = croppedImage, Polygon = polygon });
            DismissViewController(true, null);
        }

        [Export("imageEditingViewControllerDidCancelChanges:")]
        public void ImageEditingViewControllerDidCancelChanges(SBSDKImageEditingViewController editingViewController)
        {
            DismissViewController(true, null);
        }

        [Export("imageEditingViewControllerApplyButtonItem:")]
        UIBarButtonItem ImageEditingViewControllerApplyButtonItem(SBSDKImageEditingViewController editingViewController)
        {
            return new UIBarButtonItem("Apply", UIBarButtonItemStyle.Plain, null);
        }

        [Export("imageEditingViewControllerCancelButtonItem:")]
        UIBarButtonItem ImageEditingViewControllerCancelButtonItem(SBSDKImageEditingViewController editingViewController)
        {
            return new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Plain, null);
        }

        [Export("imageEditingViewControllerRotateClockwiseToolbarItem:")]
        public UIBarButtonItem ImageEditingViewControllerRotateClockwiseToolbarItem(SBSDKImageEditingViewController editingViewController)
        {
            return new UIBarButtonItem("Rotate", UIBarButtonItemStyle.Plain, (sender, e) => {
                imageEditingViewController.RotateInputImageClockwise(true, true);
            });
        }

        #endregion

    }
}
