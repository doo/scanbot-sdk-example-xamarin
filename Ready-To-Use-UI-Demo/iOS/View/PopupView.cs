using System;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.model;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class PopupView : UIView
    {
        public UILabel Label { get; private set; }

        public PopupButton CloseButton { get; private set; }

        public PopupButton CopyButton { get; private set; }

        UIView buttonSeparator;

        public PopupView(string text)
        {
            BackgroundColor = UIColor.White;
            Layer.CornerRadius = 5;

            Label = new UILabel();
            Label.Lines = 0;
            Label.LineBreakMode = UILineBreakMode.WordWrap;
            Label.Text = text;
            AddSubview(Label);

            CloseButton = new PopupButton("CLOSE");
            AddSubview(CloseButton);

            CopyButton = new PopupButton("COPY");
            AddSubview(CopyButton);

            buttonSeparator = new UIView();
            buttonSeparator.BackgroundColor = Colors.AppleBlue;
            AddSubview(buttonSeparator);

            ClipsToBounds = true;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat buttonW = Frame.Width / 2;
            nfloat buttonH = buttonW / 3.5f;

            nfloat padding = 5;

            nfloat separatorW = 2;
            nfloat separatorH = buttonH - 2 * padding;

            nfloat x = padding;
            nfloat y = padding;
            nfloat w = Frame.Width - 2 * padding;
            nfloat h = Frame.Height - (2 * padding + buttonH);

            Label.Frame = new CGRect(x, y, w, h);

            x = 0;
            y = Frame.Height - buttonH;
            w = buttonW;
            h = buttonH;

            CloseButton.Frame = new CGRect(x, y, w, h);

            x += buttonW;

            CopyButton.Frame = new CGRect(x, y, w, h);

            buttonSeparator.Frame = new CGRect(x, y + padding, separatorW, separatorH);
        }
    }

    public class PopupButton : UIView
    {
        public EventHandler<EventArgs> Click;

        UILabel label;

        public PopupButton(string text)
        {
            BackgroundColor = Colors.NearWhite;

            label = new UILabel();
            label.Text = text;
            label.TextColor = Colors.AppleBlue;
            label.ClipsToBounds = true;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = UIFont.FromName("HelveticaNeue-Bold", 15);
            AddSubview(label);

            ClipsToBounds = true;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            label.Frame = Bounds;
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
