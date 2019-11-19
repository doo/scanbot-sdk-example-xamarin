using Foundation;
using System;
using UIKit;

namespace ClassicalComponentsDemo.iOS
{
    public partial class ThumbnailCollectionViewCell : UICollectionViewCell
    {
        public ThumbnailCollectionViewCell(IntPtr handle) : base(handle)
        {
        }

        public void ShowThumbnail(UIImage image)
        {
            thumbnailImage.Image = image;
        }
    }
}