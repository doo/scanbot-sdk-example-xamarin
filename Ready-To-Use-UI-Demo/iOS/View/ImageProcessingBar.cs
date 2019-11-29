
using System;
using CoreGraphics;
using Foundation;
using ReadyToUseUIDemo.model;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class ImageProcessingButton : UIView
    {
        public EventHandler<EventArgs> Click;
        UILabel textView;

        public ImageProcessingButton(string text)
        {
            textView = new UILabel();
            textView.TextAlignment = UITextAlignment.Center;
            textView.TextColor = UIColor.White;
            textView.Font = UIFont.FromName("HelveticaNeue-Bold", 13);
            textView.Text = text;
            textView.BackgroundColor = UIColor.Clear;
            AddSubview(textView);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            Layer.Opacity = 0.5f;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            Layer.Opacity = 1.0f;
            Click?.Invoke(this, EventArgs.Empty);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            Layer.Opacity = 1.0f;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            textView.Frame = Bounds;
        }
    }

    public class ImageProcessingBar : UIView
    {
        public ImageProcessingButton CropAndRotateButton { get; private set; }

        public ImageProcessingButton FilterButton { get; private set; }

        public ImageProcessingButton DeleteButton { get; private set; }

        public ImageProcessingBar()
        {
            BackgroundColor = Colors.ScanbotRed;

            CropAndRotateButton = new ImageProcessingButton(Texts.crop_amp_rotate);
            AddSubview(CropAndRotateButton);

            FilterButton = new ImageProcessingButton(Texts.filter);
            AddSubview(FilterButton);

            DeleteButton = new ImageProcessingButton(Texts.delete);
            AddSubview(DeleteButton);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat buttonW = Frame.Width / 3;

            nfloat x = 0;
            nfloat y = 0;
            nfloat w = buttonW;
            nfloat h = Frame.Height;

            CropAndRotateButton.Frame = new CGRect(x, y, w, h);

            x += w;

            FilterButton.Frame = new CGRect(x, y, w, h);

            x += w;

            DeleteButton.Frame = new CGRect(x, y, w, h);
        }
    }
}
