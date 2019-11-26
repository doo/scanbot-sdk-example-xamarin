using System;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.model;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class ScannerButton : UIView
    {
        public EventHandler<EventArgs> Click;

        public ListItem Data { get; private set; }

        UILabel title;

        public ScannerButton(ListItem data)
        {
            title = new UILabel();
            title.Text = data.Title;
            title.Font = UIFont.FromName("HelveticaNeue", 13f);
            title.TextColor = Colors.DarkGray;
            AddSubview(title);

            Layer.CornerRadius = 3;
            Layer.BorderColor = Colors.LightGray.CGColor;
            Layer.BorderWidth = 1;
        }


        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat padding = 5;
            title.Frame = new CGRect(padding, 0, Frame.Width - 2 * padding, Frame.Height);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            Layer.Opacity = 0.5f;
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            Layer.Opacity = 1.0f;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            Layer.Opacity = 1.0f;
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}
