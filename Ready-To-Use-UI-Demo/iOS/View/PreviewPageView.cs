
using System;
using CoreGraphics;
using ReadyToUseUIDemo.iOS.View.Collection;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class PreviewPageView : UIView
    {
        public ImageCollection Collection { get; private set; }

        public PreviewProcessingBar Bar { get; private set; }

        public PreviewPageView()
        {
            Collection = new ImageCollection(CGRect.Empty);
            Collection.BackgroundColor = UIColor.White;
            AddSubview(Collection);

            Bar = new PreviewProcessingBar();
            AddSubview(Bar);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat barHeight = 60;

            nfloat x = 0;
            nfloat y = 0;
            nfloat w = Frame.Width;
            nfloat h = Frame.Height - barHeight;

            Collection.Frame = new CGRect(x, y, w, h);

            y += h;
            h = barHeight;

            Bar.Frame = new CGRect(x, y, w, h);
        }
    }
}
