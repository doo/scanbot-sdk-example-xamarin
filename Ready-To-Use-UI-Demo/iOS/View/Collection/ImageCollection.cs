using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View.Collection
{
    public class ImageCollection : UICollectionView, IUICollectionViewSource,
        IUICollectionViewDelegate, IUICollectionViewDelegateFlowLayout
    {
        public List<SBSDKUIPage> Pages { get; set; } = new List<SBSDKUIPage>();

        private static readonly UICollectionViewFlowLayout _layout;
        
        static ImageCollection()
        {
            _layout = new UICollectionViewFlowLayout();
            _layout.ScrollDirection = UICollectionViewScrollDirection.Vertical;
        }

        public ImageCollection(CGRect frame) : base(frame, _layout)
        {
            DataSource = this;
            Delegate = this;

            RegisterClassForCell(typeof(ImageCell), ImageCell.Identifier);
        }

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (ImageCell)collectionView.DequeueReusableCell(ImageCell.Identifier, indexPath);

            var page = Pages[indexPath.Row];

            //animalCell.Image = page.Image;
            cell.BackgroundColor = UIColor.Yellow;
            return cell;
        }

        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return Pages.Count;
        }

        public override nint NumberOfItemsInSection(nint section)
        {
            return Pages.Count;
        }
        
        [Export("collectionView:didSelectItemAtIndexPath:")]
        public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            Console.WriteLine("selected!");
        }
    }
}
