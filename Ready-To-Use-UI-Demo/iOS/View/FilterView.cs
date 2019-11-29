
using System;
using CoreGraphics;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class FilterView : UIView
    {
        public UIImageView ImageView { get; private set; }

        public ImageProcessingBar ProcessingBar { get; private set; }

        public FilterView()
        {
            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.BackgroundColor = UIColor.White;
            AddSubview(ImageView);

            ProcessingBar = new ImageProcessingBar();
            AddSubview(ProcessingBar);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat barHeight = 50;

            nfloat x = 0;
            nfloat y = 0;
            nfloat w = Frame.Width;
            nfloat h = Frame.Height - barHeight;

            ImageView.Frame = new CGRect(x, y, w, h);

            y += h;
            h = barHeight;

            ProcessingBar.Frame = new CGRect(x, y, w, h);
        }
    }
}
