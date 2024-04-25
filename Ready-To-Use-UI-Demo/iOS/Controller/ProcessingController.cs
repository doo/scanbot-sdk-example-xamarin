using System;
using ReadyToUseUIDemo.iOS.Repository;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class ProcessingController : UIViewController
    {
        public UIImageView ImageView { get; private set; }

        private CroppingFinishedHandler handler;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Process Image";

            handler = new CroppingFinishedHandler();

            SetUpPreview();
            SetupToolbar();
        }

        private void SetUpPreview()
        {
            View.BackgroundColor = UIColor.White;

            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Image = PageRepository.Current.DocumentImage;
            ImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(ImageView);

            ImageView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor, 0.9f).Active = true;
            ImageView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor, 0.75f).Active = true;
            ImageView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;
            ImageView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
        }

        private void SetupToolbar()
        {
            this.SetToolbarItems(new UIBarButtonItem[]
            {
                new UIBarButtonItem("Crop & Rotate", UIBarButtonItemStyle.Plain, CropAndRotate),
                new UIBarButtonItem("Filter", UIBarButtonItemStyle.Plain, OpenFilterScreen),
                new UIBarButtonItem("Delete", UIBarButtonItemStyle.Plain, DeleteImage),
                new UIBarButtonItem("Quality", UIBarButtonItemStyle.Plain, CheckQuality)
            }, true);
            this.NavigationController.SetToolbarHidden(false, false);
        }

        private void CheckQuality(object sender, EventArgs e)
        {
            var quality = new SBSDKDocumentQualityAnalyzer().AnalyzeOnImage(PageRepository.Current.DocumentImage);
            Utils.Alert.Show(this, "Document Quality", quality.ToString());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            handler.Finished += CroppingFinished;

            ImageView.Image = PageRepository.Current.DocumentImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            handler.Finished -= CroppingFinished;
        }

        private void CropAndRotate(object sender, EventArgs e)
        {
            var config = SBSDKUICroppingScreenConfiguration.DefaultConfiguration;
            var controller = SBSDKUICroppingViewController.CreateNewWithPage(PageRepository.Current, config, handler);
            controller.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(controller, false, null);
        }

        private void CroppingFinished(object sender, CroppingFinishedArgs e)
        {
            PageRepository.Current = e.Page;
            ImageView.Image = PageRepository.Current.DocumentImage;
        }

        private void OpenFilterScreen(object sender, EventArgs e)
        {
            var controller = new FilterController();
            NavigationController.PushViewController(controller, true);
        }

        private void DeleteImage(object sender, EventArgs e)
        {
            PageRepository.Remove(PageRepository.Current);
            NavigationController.PopViewController(true);
        }
    }

    public class CroppingFinishedArgs : EventArgs
    {
        public SBSDKUIPage Page { get; set; }
    }

    public class CroppingFinishedHandler : SBSDKUICroppingViewControllerDelegate
    {
        public EventHandler<CroppingFinishedArgs> Finished;

        public override void DidFinish(SBSDKUICroppingViewController viewController, SBSDKUIPage changedPage)
        {
            Finished?.Invoke(this, new CroppingFinishedArgs { Page = changedPage });
        }
    }

}
