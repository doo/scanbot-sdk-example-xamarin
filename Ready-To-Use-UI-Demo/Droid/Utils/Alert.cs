using System;
using Android.Content;
using Android.Widget;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class Alert
    {

        public static void ShowLicenseDialog(Context context)
        {
            var text =
                "The demo app will terminate because of the missing license key. " +
                "Get your free 30-day license today!";
            Toast.MakeText(context, text, ToastLength.Long).Show();
        }

    }
}
