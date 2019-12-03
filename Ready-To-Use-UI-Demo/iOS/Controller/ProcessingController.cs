using System;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.View;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class ProcessingController : UIViewController
    {
        public ProcessingView ContentView { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new ProcessingView();
            View = ContentView;

            Title = "Process Image";
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click += CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click += OpenFilterScreen;

            ContentView.ProcessingBar.DeleteButton.Click += DeleteImage;

            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click -= CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click -= OpenFilterScreen;

            ContentView.ProcessingBar.DeleteButton.Click -= DeleteImage;
        }

        private void CropAndRotate(object sender, EventArgs e)
        {
            var controller = new CroppingController(PageRepository.Current.DocumentImage);
            PresentViewController(controller, true, null);

            controller.Finished += CroppingFinished;
        }

        private void CroppingFinished(object sender, CroppingEventArgs e)
        {
            (sender as CroppingController).Finished = null;
            PageRepository.UpdateCurrent(e.Image, e.Polygon);
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
}
