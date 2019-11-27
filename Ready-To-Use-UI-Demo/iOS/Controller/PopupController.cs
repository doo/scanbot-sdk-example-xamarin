using System;
using CoreGraphics;
using ReadyToUseUIDemo.iOS.View;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Controller
{
    public class PopupController : UIViewController
    {
        public PopupView Content { get; set; }
        string text;

        public PopupController(string text)
        {
            this.text = text;
            ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem
            {
                Title = "Close"
            };

            NavigationItem.RightBarButtonItem = new UIBarButtonItem
            {
                Title = "Copy"
            };

            nfloat hPadding = 20;
            nfloat vPadding = View.Frame.Height / 6;

            nfloat x = hPadding;
            nfloat y = vPadding;
            nfloat w = View.Frame.Width - 2 * hPadding;
            nfloat h = View.Frame.Height - 2 * vPadding;

            Content = new PopupView(text);
            Content.Frame = new CGRect(x, y, w, h);
            Content.Label.Text = text;

            View.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.5f);
            View.AddSubview(Content);
        }

        public void Dismiss()
        {
            DismissModalViewController(true);
            Content.CopyButton.Click = null;
            Content.CloseButton.Click = null;
        }
    }
}
