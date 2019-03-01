using System;
using System.IO;
using Android.App;
using Android.Runtime;
using Android.Util;

using ScanbotSDK.Xamarin.Android;

namespace scanbotsdkexamplexamarin.Droid
{
    // It is strongly recommended to add the LargeHeap = true flag in your Application class.
    // Working with images, creating PDFs, etc. are memory intensive tasks. So to prevent OutOfMemoryError, consider adding this flag!
    // For more details see: http://developer.android.com/guide/topics/manifest/application-element.html#largeHeap
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        static readonly string LOG_TAG = typeof(MainApplication).Name;

        // Use a custom temp storage directory for demo purposes.
        public static readonly TempImageStorage TempImageStorage = new TempImageStorage(GetExampleTempStorageDir());

        // TODO Add your Scanbot SDK license key here.
        // You can test all Scanbot SDK features and develop your app without a license. 
        // However, if you do not specify the license key when initializing the SDK, 
        // it will work in trial mode (trial period of 1 minute). 
        // To get another trial period you have to restart your app.
        const string licenseKey = null;


        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }


        public override void OnCreate()
        {
            base.OnCreate();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");
            SBSDK.Initialize(this, licenseKey, new SBSDKConfiguration { EnableLogging = true });

            // In this example we always cleanup the demo temp storage directory on app start.
            TempImageStorage.CleanUp();
        }

        private static string GetExampleTempStorageDir()
        {
            // !! Please note !!
            // In this demo app we use the "ExternalStorageDirectory" which is a public(!) storage directory.
            // All image files as well as export files (PDF, TIFF, etc) created by this demo app will be stored
            // in a sub-folder of this storage directory and will be accessible for every(!) app having external storage permissions!
            // We use the "ExternalStorageDirectory" here only for demo purposes, to be able to share generated PDF and TIFF files.
            // (also see the example code for PDF and TIFF creation).
            // If you need a secure storage for all images and export files (which is strongly recommended) use a suitable internal(!) storage directory.
            //
            // For more detais about the Android file system see:
            // - https://developer.android.com/guide/topics/data/data-storage
            // - https://docs.microsoft.com/en-us/xamarin/android/platform/files/

            var externalPublicPath = Path.Combine(
                Android.OS.Environment.ExternalStorageDirectory.Path, "scanbot-sdk-example-xamarin_demo-storage");
            Directory.CreateDirectory(externalPublicPath);
            return externalPublicPath;
        }
    }
}
