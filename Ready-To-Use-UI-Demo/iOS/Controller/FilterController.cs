using System;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.View;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class FilterController : UIViewController
    {
        public FilterView ContentView { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new FilterView();
            View = ContentView;

            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;

            Title = "Process Image";
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click += CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click += ApplyFilter;

            ContentView.ProcessingBar.DeleteButton.Click += DeleteImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.ProcessingBar.CropAndRotateButton.Click -= CropAndRotate;

            ContentView.ProcessingBar.FilterButton.Click -= ApplyFilter;

            ContentView.ProcessingBar.DeleteButton.Click -= DeleteImage;
        }

        private void CropAndRotate(object sender, EventArgs e)
        {
            Console.WriteLine("Crop & Rotate");
            var controller = new CroppingController(PageRepository.Current.DocumentImage);
            PresentViewController(controller, true, null);
        }

        private void ApplyFilter(object sender, EventArgs e)
        {
            Console.WriteLine("Apply Filter");
        }

        private void DeleteImage(object sender, EventArgs e)
        {
            PageRepository.Remove(PageRepository.Current);
            NavigationController.PopViewController(true);
        }

    }
}
