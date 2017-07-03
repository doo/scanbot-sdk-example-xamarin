using System;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Content;

using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Android.Wrapper;

using Java.Util;

using AndroidNetUri = Android.Net.Uri;
using AndroidOS = Android.OS;
using Android.Util;

namespace scanbotsdkexamplexamarin.Droid
{
    [Activity(Label = "Scanbot SDK Example Xamarin", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        public static TempImageStorage TempImageStorage = new TempImageStorage();

        static string LOG_TAG = typeof(MainActivity).Name;

        const int REQUEST_SB_SCANNING_UI = 4711;
        const int REQUEST_SB_CROPPING_UI = 4712;
        const int REQUEST_SYSTEM_GALLERY = 4713;

        const int BIG_THUMB_MAX_W = 800, BIG_THUMB_MAX_H = 800;

        AndroidNetUri documentImageUri, originalImageUri;

        ImageView imageView;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            imageView = FindViewById<ImageView>(Resource.Id.imageView);

            AssignCopyrightText();
            AssignStartCameraButtonHandler();
            AssingCroppingUIButtonHandler();
            AssignApplyImageFilterButtonHandler();
            AssignDocumentDetectionButtonHandler();
            AssignCreatePdfButtonHandler();
        }


        void AssignCopyrightText()
        {
            var copyrightTextView = FindViewById<TextView>(Resource.Id.copyrightTextView);
            copyrightTextView.Text = "Copyright (c) "+DateTime.Now.Year.ToString()+" doo GmbH. All rights reserved.";
        }

        void AssignStartCameraButtonHandler()
        {
            var scanningUIButton = FindViewById<Button>(Resource.Id.scanningUIButton);
            scanningUIButton.Click += delegate
            {
                Intent intent = new Intent(this, typeof(CameraViewDemoActivity));
                StartActivityForResult(intent, REQUEST_SB_SCANNING_UI);
            };
        }

        void AssignApplyImageFilterButtonHandler()
        {
            var applyImageFilterButton = FindViewById<Button>(Resource.Id.applyImageFilterButton);
            applyImageFilterButton.Click += delegate
            {
                if (!CheckDocumentImage()) { return; }

                DebugLog("Applying image filter on image: " + documentImageUri);
                try
                {
                    Task.Run(() =>
                    {
                        // The SDK call is sync!
                        var resultImage = SBSDK.ApplyImageFilter(documentImageUri, ImageFilter.Binarized, this);
                        documentImageUri = TempImageStorage.AddImage(resultImage);
                        ShowImageView(resultImage);
                    });
                }
                catch (Exception e)
                {
                    ErrorLog("Error applying image filter", e);
                }
            };
        }

        void AssignDocumentDetectionButtonHandler()
        {
            var documentDetectionButton = FindViewById<Button>(Resource.Id.documentDetectionButton);
            documentDetectionButton.Click += delegate
            {
                // Select image from gallery and run document detection
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                imageIntent.PutExtra(Intent.ExtraLocalOnly, true);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), REQUEST_SYSTEM_GALLERY);
            };
        }

        void AssingCroppingUIButtonHandler()
        {
            var croppingUIButton = FindViewById<Button>(Resource.Id.croppingUIButton);
            croppingUIButton.Click += delegate
            {
                if (!CheckOriginalImage()) { return; }

                Intent intent = new Intent(this, typeof(CroppingImageDemoActivity));
                intent.PutExtra(CroppingImageDemoActivity.EXTRAS_ARG_IMAGE_FILE_URI, originalImageUri.ToString());
                StartActivityForResult(intent, REQUEST_SB_CROPPING_UI);
            };
        }

        void AssignCreatePdfButtonHandler()
        {
            var createPdfButton = FindViewById<Button>(Resource.Id.createPdfButton);
            createPdfButton.Click += delegate
            {
                if (!CheckDocumentImage()) { return; }

                DebugLog("Starting PDF creation...");

                var externalPath = GetPublicExternalStorageDirectory();
                var targetFile = System.IO.Path.Combine(externalPath, UUID.RandomUUID() + ".pdf");
                var pdfOutputUri = AndroidNetUri.FromFile(new Java.IO.File(targetFile));
                Task.Run(() =>
                {
                    try
                    {
                        var images = new AndroidNetUri[] { documentImageUri }; // add more images for PDF pages here
                        // The SDK call is sync!
                        SBSDK.CreatePDF(images, pdfOutputUri, this);
                        DebugLog("PDF file created: " + pdfOutputUri);
                        OpenPDFFile(pdfOutputUri);
                    }
                    catch (Exception e)
                    {
                        ErrorLog("Error creating PDF", e);
                    }
                });
            };
        }

        bool CheckDocumentImage()
        {
            if (documentImageUri == null)
            {
                Toast.MakeText(this, "Please snap a document image via Scanning UI or run Document Detection on an image file from the gallery", ToastLength.Long).Show();
                return false;
            }
            return true;
        }

        bool CheckOriginalImage()
        {
            if (originalImageUri == null)
            {
                Toast.MakeText(this, "Please snap a document image via Scanning UI or run Document Detection on an image file from the gallery", ToastLength.Long).Show();
                return false;
            }
            return true;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_SB_SCANNING_UI && resultCode == Result.Ok)
            {
                documentImageUri = AndroidNetUri.Parse(data.GetStringExtra(CameraViewDemoActivity.EXTRAS_ARG_DOC_IMAGE_FILE_URI));
                originalImageUri = AndroidNetUri.Parse(data.GetStringExtra(CameraViewDemoActivity.EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI));
                ShowImageView(ImageUtils.LoadImage(documentImageUri, this));
                return;
            }

            if (requestCode == REQUEST_SB_CROPPING_UI && resultCode == Result.Ok)
            {
                documentImageUri = AndroidNetUri.Parse(data.GetStringExtra(CroppingImageDemoActivity.EXTRAS_ARG_IMAGE_FILE_URI));
                ShowImageView(ImageUtils.LoadImage(documentImageUri, this));
                return;
            }

            if (requestCode == REQUEST_SYSTEM_GALLERY && resultCode == Result.Ok)
            {
                originalImageUri = data.Data;
                RunDocumentDetection(originalImageUri);
                return;
            }
        }

        void RunDocumentDetection(AndroidNetUri imageUri)
        {
            DebugLog("Running document detection on image: " + imageUri);

            Task.Run(() =>
            {
                try
                {
                    // The SDK call is sync!
                    var detectionResult = SBSDK.DocumentDetection(imageUri, this);
                    DebugLog("Document detection result: " + detectionResult.Status);
                    if (detectionResult.Status.IsOk())
                    {
                        var documentImage = detectionResult.Image as Bitmap;
                        documentImageUri = TempImageStorage.AddImage(documentImage);
                        ShowImageView(documentImage);

                        DebugLog("Detected polygon: ");
                        foreach (var p in detectionResult.Polygon)
                        {
                            DebugLog(p.ToString());
                        }
                    }
                    else
                    {
                        DebugLog("No document detected!");
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "No document detected! (Detection result: " + detectionResult.Status + ")", ToastLength.Long).Show();
                        });
                    }
                }
                catch (Exception e)
                {
                    ErrorLog("Error while document detection", e);
                }
            });
        }

        void ShowImageView(Bitmap bitmap)
        {
            imageView.Post(() =>
            {
                var thumb = ImageUtils.GetThumbnail(bitmap, BIG_THUMB_MAX_W, BIG_THUMB_MAX_H);
                imageView.SetImageBitmap(thumb);
            });
        }

        void OpenPDFFile(AndroidNetUri pdfFileUri)
        {
            Intent openIntent = new Intent();
            openIntent.SetAction(Intent.ActionView);
            openIntent.SetDataAndType(pdfFileUri, "application/pdf");
            openIntent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            if (openIntent.ResolveActivity(this.PackageManager) != null)
            {
                StartActivity(openIntent);
            }
            else
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Error opening PDF document", ToastLength.Long).Show();
                });
            }
        }

        string GetPublicExternalStorageDirectory()
        {
            var externalPublicPath = System.IO.Path.Combine(
                AndroidOS.Environment.ExternalStorageDirectory.Path, "scanbot-sdk-example-xamarin");
            Directory.CreateDirectory(externalPublicPath);
            return externalPublicPath;
        }

        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }

        void ErrorLog(string msg, Exception ex)
        {
            Log.Error(LOG_TAG, Java.Lang.Throwable.FromException(ex), msg);
        }

    }
}

