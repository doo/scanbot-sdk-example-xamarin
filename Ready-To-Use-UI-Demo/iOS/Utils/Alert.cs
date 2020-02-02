
using System;
using UIKit;
namespace ReadyToUseUIDemo.iOS.Utils
{
    public class Alert
    {
        public static void Show (UIViewController parent, string title, string body)
        {
            var alert = UIAlertController.Create(title, body, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Close", UIAlertActionStyle.Cancel, delegate { }));
            parent.PresentViewController(alert, true, null);
        }
    }
}
