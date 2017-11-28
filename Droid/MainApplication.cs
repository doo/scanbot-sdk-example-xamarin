using System;
using Android.App;
using Android.Runtime;
using Android.Util;

using ScanbotSDK.Xamarin.Android.Wrapper;

namespace scanbotsdkexamplexamarin.Droid
{
    // It is strongly recommended to add the LargeHeap = true flag in your Application class.
    // Working with images, creating PDFs, etc. are memory intensive tasks. So to prevent OutOfMemoryError, consider adding this flag!
    // For more details see: http://developer.android.com/guide/topics/manifest/application-element.html#largeHeap
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        static string LOG_TAG = typeof(MainApplication).Name;

        // Add your Scanbot SDK license key here.
        // You can test all Scanbot SDK features and develop your app without a license. 
        // However, if you do not specify the license key when initializing the SDK, 
        // it will work in trial mode (trial period of 1 minute). 
        // To get another trial period you have to restart your app.
        const string licenseKey = "";

        static MainApplication()
        {
            // Workaround for an Android issue (Android <= 4.2):
            // load native libs manually which are required by Scanbot SDK native libs
            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.JellyBeanMr1)
            {
                Java.Lang.JavaSystem.LoadLibrary("lept");
                Java.Lang.JavaSystem.LoadLibrary("tess");
            }
        }


        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }


        public override void OnCreate()
        {
            base.OnCreate();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");
            SBSDK.Initialize(this, licenseKey, true);
        }
    }
}
