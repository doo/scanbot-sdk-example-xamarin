using UIKit;
using Foundation;
using ScanbotSDK.iOS;
using System;

namespace ClassicalComponentsDemo.iOS
{

    partial class BusinessCardDemoCell : UICollectionViewCell
    {
        private UIImage _image;
        public UIImage Image
        {
            get
            {
                return _image;
            }

            set
            {
                _image = value;
                imageView.Image = value;
            }
        }

        public BusinessCardDemoCell(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            imageView.Image = Image;
        }
    }
}