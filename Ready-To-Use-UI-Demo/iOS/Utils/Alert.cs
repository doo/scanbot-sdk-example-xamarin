using System;
using ReadyToUseUIDemo.iOS.Controller;
using System.Collections.Generic;
using UIKit;
namespace ReadyToUseUIDemo.iOS.Utils
{
    public class Alert
    {
        public static void Show(UIViewController parent, string title, string body)
        {
            var alert = UIAlertController.Create(title, body, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Close", UIAlertActionStyle.Cancel, delegate { }));
            parent.PresentViewController(alert, true, null);
        }

        public static bool IsPresented { get; set; }

        public static void ShowPopup(UIViewController controller, string text, Action onClose = null)
        {
            if (IsPresented)
            {
                return;
            }

            IsPresented = true;

            var images = new List<UIImage>();
            var popover = new PopupController(text, images);

            controller.PresentViewController(popover, true, delegate
            {
                popover.Content.CloseButton.Click += delegate
                {
                    IsPresented = false;
                    popover.Dismiss();
                    onClose?.Invoke();
                };
            });
        }
    }
}
