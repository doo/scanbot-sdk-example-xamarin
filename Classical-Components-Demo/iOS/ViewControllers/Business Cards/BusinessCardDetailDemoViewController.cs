using UIKit;
using System;

namespace ClassicalComponentsDemo.iOS
{
    public partial class BusinessCardDetailDemoViewController : UIViewController
    {
        private BusinessCard _businessCard;
        public BusinessCard BusinessCard
        {
            get
            {
                return _businessCard;
            }

            set
            {
                _businessCard = value;
                UpdateBusinessCard();
            }
        }

        public BusinessCardDetailDemoViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UpdateBusinessCard();
        }

        private void UpdateBusinessCard()
        {
            if (IsViewLoaded)
            {
                imageView.Image = BusinessCard.Image;
                textView.Text = BusinessCard.RecognizedText;
            }
        }
    }
}