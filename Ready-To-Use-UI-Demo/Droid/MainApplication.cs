﻿using System;
using System.IO;
using Android.App;
using Android.Runtime;
using Android.Util;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Android;

namespace ReadyToUseUIDemo.Droid
{
    // It is strongly recommended to add the LargeHeap = true flag in your Application class.
    // Working with images, creating PDFs, etc. are memory intensive tasks. So to prevent OutOfMemoryError, consider adding this flag!
    // For more details see: http://developer.android.com/guide/topics/manifest/application-element.html#largeHeap
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        static readonly string LOG_TAG = typeof(MainApplication).Name;

        // TODO Add the Scanbot SDK license key here.
        // Please note: The Scanbot SDK will run without a license key for one minute per session!
        // After the trial period is over all Scanbot SDK functions as well as the UI components will stop working.
        // You can get an unrestricted "no-strings-attached" 30 day trial license key for free.
        // Please submit the trial license form (https://scanbot.io/sdk/trial.html) on our website by using
        // the app identifier "io.scanbot.example.sdk.xamarin.rtu" of this example app.
        const string LICENSE_KEY = null;

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }

        public override void OnCreate()
        {
            base.OnCreate();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");

            // Initialization with a custom, public(!) "StorageBaseDirectory" for demo purposes - see comments below!
            SBSDK.Initialize(this, LICENSE_KEY, new SBSDKConfiguration
            {
                EnableLogging = true,
                StorageBaseDirectory = GetDemoStorageBaseDirectory(),
                //Encryption = new SBSDKEncryption
                //{
                //    Mode = EncryptionMode.AES256,
                //    Password = "S0m3W3irDL0ngPa$$w0rdino!!!!"
                //}
            });

            ImageLoader.Instance = new ImageLoader(this);

            // Alternative initialization with the default "StorageBaseDirectory" which will be internal and secure (recommended).
            //SBSDK.Initialize(this, LICENSE_KEY, new SBSDKConfiguration { EnableLogging = true });
        }

        private string GetDemoStorageBaseDirectory()
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
            var external = GetExternalFilesDir(null).AbsolutePath;
            var path = Path.Combine(external, "scanbot-sdk-example-xamarin-rtu_demo-storage");
            Directory.CreateDirectory(path);

            return path;
        }
    }
}
