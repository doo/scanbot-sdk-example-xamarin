using System;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.View;
using ReadyToUseUIDemo.iOS.View.Collection;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class ImageListController : UIViewController
    {
        public ImageCollectionView ContentView { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new ImageCollectionView();
            View = ContentView;

            ContentView.Collection.Pages.AddRange(PageRepository.Items);
            ContentView.Collection.ReloadData();

            Title = "Choose an image";
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.Collection.Selected += OnImageSelected;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.Collection.Selected -= OnImageSelected;
        }

        void OnImageSelected(object sender, CollectionEventArgs e)
        {
            PageRepository.Current = e.Page;

            var controller = new FilterController();
            NavigationController.PushViewController(controller, true);
        }
    }
}
