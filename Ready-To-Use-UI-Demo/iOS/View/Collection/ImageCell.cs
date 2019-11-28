using System;
using Foundation;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View.Collection
{
    public class ImageCell : UICollectionViewCell
    {
        public const string Identifier = "ImageCell";

        public UIImageView ImageView { get; private set; }

        public ImageCell()
        {
            Initialize();
        }

        public ImageCell(IntPtr handle) : base (handle)
        {
            Initialize();
        }

        void Initialize()
        {
            ImageView = new UIImageView();
            AddSubview(ImageView);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ImageView.Frame = Bounds;
        }
    }
}
