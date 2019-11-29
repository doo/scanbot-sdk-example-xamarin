
using System;
using System.Collections.Generic;
using CoreGraphics;
using ReadyToUseUIDemo.model;
using UIKit;

namespace ReadyToUseUIDemo.iOS.View
{
    public class ButtonContainer : UIView
    {
        public UILabel Title { get; private set; }

        public List<ScannerButton> Buttons { get; private set; }

        nfloat padding = 15;
        nfloat titleHeight, buttonHeight;
        public nfloat Height
        {
            get
            {
                return titleHeight +
                    (Buttons.Count * buttonHeight) + 
                    (Buttons.Count * padding);
            }
        }

        public ButtonContainer(string title, List<ListItem> data)
        {
            Title = new UILabel();
            Title.Text = title;
            Title.Font = UIFont.FromName("HelveticaNeue-Bold", 13f);
            Title.TextColor = Colors.DarkGray;
            AddSubview(Title);

            Buttons = new List<ScannerButton>();

            foreach(ListItem item in data)
            {
                var button = new ScannerButton(item);
                AddSubview(button);
                Buttons.Add(button);
            }

            titleHeight = 20;
            buttonHeight = 40;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat x = padding;
            nfloat y = 0;
            nfloat w = Frame.Width - 2 * padding;
            nfloat h = titleHeight;

            Title.Frame = new CGRect(x, y, w, h);

            y += h + padding;
            h = buttonHeight;

            foreach(ScannerButton button in Buttons)
            {
                button.Frame = new CGRect(x, y, w, h);
                y += h + padding;
            }
        }
    }
}
