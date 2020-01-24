using System;
using System.Collections.Generic;
using CoreGraphics;
using ReadyToUseUIDemo.iOS.View;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class PopupController : UIViewController
    {
        public PopupView Content { get; set; }
        string text;
        List<UIImage> images;

        public PopupController(string text, List<UIImage> images)
        {
            this.text = text;
            this.images = images;

            ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;

        }

        public override void ViewDidLoad()
        {
            nfloat hPadding = 20;
            nfloat vPadding = View.Frame.Height / 6;

            nfloat x = hPadding;
            nfloat y = vPadding;
            nfloat w = View.Frame.Width - 2 * hPadding;
            nfloat h = View.Frame.Height - 2 * vPadding;

            Content = new PopupView(text);
            Content.ImageContainer.Items = images;
            Content.Frame = new CGRect(x, y, w, h);
            Content.Label.Text = text;
            
            View.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.5f);
            View.AddSubview(Content);
        }

        public void Dismiss()
        {
            DismissModalViewController(true);
            Content.CloseButton.Click = null;
        }
    }
}
