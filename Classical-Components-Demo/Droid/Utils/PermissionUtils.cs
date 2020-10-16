using System;
using Android;
using Android.App;
using Android.Views;
using AndroidX.Core.App;
using Google.Android.Material.Snackbar;

namespace ClassicalComponentsDemo.Droid.Utils
{
    public class PermissionUtils
    {
        public const int REQUEST_CAMERA = 1337;

        public static void Request(Activity context, View layout)
        {
            var permissions = new string[] { Manifest.Permission.Camera, Manifest.Permission.ReadExternalStorage };
            var text = "This app requires camera permission to scan documents";

            if (ShowRationale(context, Manifest.Permission.Camera))
            {
                // Provide an additional rationale to the user if the permission was not granted
                // and the user would benefit from additional context for the use of the permission.
                // For example if the user has previously denied the permission.

                var bar = Snackbar.Make(layout, text, Snackbar.LengthIndefinite);
                var action = new Action<View>(delegate (View obj)
                {
                    Request(context, permissions, REQUEST_CAMERA);
                });

                bar.SetAction("OK", action);
                bar.Show();
            }
            else
            {
                Request(context, permissions, REQUEST_CAMERA);
            }
        }

        static bool ShowRationale(Activity context, string permission)
        {
            return ActivityCompat.ShouldShowRequestPermissionRationale(context, permission);
        }

        static void Request(Activity context, string[] permissions, int code)
        {
            ActivityCompat.RequestPermissions(context, permissions, code);
        }
    }
}
