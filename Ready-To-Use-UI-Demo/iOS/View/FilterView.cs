
using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.iOS.Model;
using ReadyToUseUIDemo.model;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class FilterView : UIView
    {
        public UIImageView ImageView { get; private set; }

        public UIPickerView Filters { get; private set; }

        public FilterPickerModel Model { get; private set; }

        public FilterView()
        {
            BackgroundColor = UIColor.White;

            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Layer.BorderColor = Colors.LightGray.CGColor;
            ImageView.Layer.BorderWidth = 1;
            ImageView.BackgroundColor = Colors.NearWhite;
            AddSubview(ImageView);

            Filters = new UIPickerView();
            AddSubview(Filters);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat padding = 5;
            nfloat pickerHeight = Frame.Width / 3 * 2;

            nfloat x = padding;
            nfloat y = padding;

            nfloat w = Frame.Width - 2 * padding;
            nfloat h = Frame.Height - (pickerHeight + 2 * padding);

            ImageView.Frame = new CGRect(x, y, w, h);

            x = 0;
            y += h + padding;
            w = Frame.Width;
            h = pickerHeight;

            Filters.Frame = new CGRect(x, y, w, h);
        }

        public void SetPickerModel(List<Filter> filters)
        {
            Model = new FilterPickerModel();
            Model.Items = filters;
            Filters.Model = Model;
        }
    }

    public class FilterEventArgs : EventArgs
    {
        public SBSDKImageFilterType Type { get; set; }
    }

    public class FilterPickerModel : UIPickerViewModel
    {
        public EventHandler<FilterEventArgs> SelectionChanged;

        public List<Filter> Items = new List<Filter>();

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return Items.Count;
        }

        [Export("pickerView:attributedTitleForRow:forComponent:")]
        public NSAttributedString GetAttributedTitle(UIPickerView pickerView, nint row, nint component)
        {
            var text = Items[(int)row].Title;
            var attributed = new NSAttributedString(text, null, Colors.AppleBlue);
            return attributed;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            var filter = Items[(int)row];
            SelectionChanged?.Invoke(this, new FilterEventArgs { Type = filter.Type });
        }
    }
}
