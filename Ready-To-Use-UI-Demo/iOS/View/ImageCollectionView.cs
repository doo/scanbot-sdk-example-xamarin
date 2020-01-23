
using System;
using CoreGraphics;
using ReadyToUseUIDemo.iOS.View.Collection;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class ImageCollectionView : UIView
    {
        public ImageCollection Collection { get; private set; }

        public ImageCollectionView()
        {
            Collection = new ImageCollection(CGRect.Empty);
            Collection.BackgroundColor = UIColor.White;
            AddSubview(Collection);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat x = 0;
            nfloat y = 0;
            nfloat w = Frame.Width;
            nfloat h = Frame.Height;

            Collection.Frame = new CGRect(x, y, w, h);
        }
    }
}
