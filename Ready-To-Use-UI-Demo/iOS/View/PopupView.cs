using System;
using System.Collections.Generic;
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

        public PopupImageContainer ImageContainer { get; internal set; }

        UIView buttonSeparator;

        public PopupView(string text)
        {
            BackgroundColor = UIColor.White;
            Layer.CornerRadius = 5;

            ImageContainer = new PopupImageContainer();
            AddSubview(ImageContainer);

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

            nfloat imagesH = 0;

            if (ImageContainer.Items.Count > 0)
            {
                imagesH = Frame.Width / 5;
            }

            nfloat padding = 5;

            nfloat separatorW = 2;
            nfloat separatorH = buttonH - 2 * padding;

            nfloat x = padding;
            nfloat y = padding;
            nfloat w = Frame.Width - 2 * padding;
            nfloat h = imagesH;

            ImageContainer.Frame = new CGRect(x, y, w, h);

            y += h + padding;
            h = Frame.Height - (3 * padding + buttonH + imagesH);

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

    public class PopupImageContainer : UIView
    {
        List<UIImageView> views = new List<UIImageView>();

        List<UIImage> images;
        public List<UIImage> Items
        {
            get => images;
            set
            {
                images = value;
                foreach (var image in images)
                {
                    var view = new UIImageView();
                    view.Image = image;
                    views.Add(view);
                    AddSubview(view);
                }
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var padding = 3;

            nfloat x = 0;
            nfloat y = 0;
            nfloat w = (Frame.Width - 2 * padding) / 3;
            nfloat h = Frame.Height;

            foreach (var view in views)
            {
                view.Frame = new CGRect(x, y, w, h);
                x += w + padding;
            }
        }
    }

}
