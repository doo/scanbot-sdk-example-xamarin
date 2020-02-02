
using System;
using ReadyToUseUIDemo.iOS.Model;
using ReadyToUseUIDemo.iOS.Repository;
using ReadyToUseUIDemo.iOS.View;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class FilterController : UIViewController
    {
        FilterView ContentView;

        SBSDKImageFilterType Choice;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ContentView = new FilterView();
            View = ContentView;

            ContentView.SetPickerModel(Filters.List);
            ContentView.ImageView.Image = PageRepository.Current.DocumentImage;

            Title = "Choose filter";

            var button = new UIBarButtonItem("Apply", UIBarButtonItemStyle.Done, FilterChosen);
            NavigationItem.SetRightBarButtonItem(button, false);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ContentView.Model.SelectionChanged += OnFilterSelected;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ContentView.Model.SelectionChanged -= OnFilterSelected;
        }

        SBSDKUIPage Temp;

        private void OnFilterSelected(object sender, FilterEventArgs e)
        {
            Choice = e.Type;

            if (Temp == null)
            {
                Temp = PageRepository.DuplicateCurrent(Choice);
            }

            Temp.Filter = Choice;
            ContentView.ImageView.Image = Temp.DocumentImage;
        }

        private void FilterChosen(object sender, EventArgs e)
        {
            PageRepository.Apply(Choice, PageRepository.Current);
            NavigationController.PopViewController(true);
        }

    }
}
