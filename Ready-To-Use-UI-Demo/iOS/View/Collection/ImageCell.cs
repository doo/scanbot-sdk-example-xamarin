using System;
using CoreGraphics;
using Foundation;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View.Collection
{
    public class ImageCell : UICollectionViewCell
    {
        public const string Identifier = "ImageCell";

        public UIImageView ImageView { get; private set; }

        public SBSDKUIPage Page { get; private set; }
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
            
            var padding = 5;

            nfloat x = padding;
            nfloat y = padding;
            nfloat w = Frame.Width - 2 * padding;
            nfloat h = Frame.Height - 2 * padding;

            ImageView.Frame = new CGRect(x, y, w, h);
        }

        public void Update(SBSDKUIPage page)
        {
            ImageView.Image = page.DocumentImage;
            Page = page;
        }
    }
}
