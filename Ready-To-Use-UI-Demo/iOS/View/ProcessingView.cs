
using System;
using CoreGraphics;
using ReadyToUseUIDemo.model;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class ProcessingView : UIView
    {
        public UIImageView ImageView { get; private set; }

        public ImageProcessingBar ProcessingBar { get; private set; }

        public ProcessingView()
        {
            BackgroundColor = UIColor.White;
            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Layer.BorderColor = Colors.LightGray.CGColor;
            ImageView.Layer.BorderWidth = 1;
            ImageView.BackgroundColor = Colors.NearWhite;
            AddSubview(ImageView);

            ProcessingBar = new ImageProcessingBar();
            AddSubview(ProcessingBar);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat padding = 5;
            nfloat barHeight = 50;

            nfloat x = padding;
            nfloat y = padding + AppDelegate.TopInset;
            
            nfloat w = Frame.Width - 2 * padding;
            nfloat h = Frame.Height - (AppDelegate.TopInset + barHeight + 2 * padding);

            ImageView.Frame = new CGRect(x, y, w, h);

            x = 0;
            y += h + padding;
            w = Frame.Width;
            h = barHeight;

            ProcessingBar.Frame = new CGRect(x, y, w, h);
        }
    }
}
