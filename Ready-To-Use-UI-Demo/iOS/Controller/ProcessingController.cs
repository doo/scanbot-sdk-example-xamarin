using System;
using Foundation;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.Utils;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.iOS;
using UIKit;
using PdfKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class ProcessingController : UIViewController
    {
        public ProcessingView ContentView { get; private set; }

        CroppingFinishedHandler handler;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new ProcessingView();
            View = ContentView;

            Title = "Process Image";
            
            handler = new CroppingFinishedHandler();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click += CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click += OpenFilterScreen;

            ContentView.ProcessingBar.DeleteButton.Click += DeleteImage;

            handler.Finished += CroppingFinished;

            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click -= CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click -= OpenFilterScreen;

            ContentView.ProcessingBar.DeleteButton.Click -= DeleteImage;

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
            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;
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
