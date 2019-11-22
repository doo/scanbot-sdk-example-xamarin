using System;
using Android.Content;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class Alert
    {

        public static void ShowLicenseDialog(Context context)
        {
            var text =
                "The demo app will terminate because of the missing license key. " +
                "Get your free 30-day license today!";
            Toast(context, text);
        }

        public static void Toast(Context context, string text)
        {
            Android.Widget.Toast.MakeText(context, text, Android.Widget.ToastLength.Long).Show();
        }
    }
}
