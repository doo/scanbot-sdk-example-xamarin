using System;
using Android.Content;
using Android.Graphics;

namespace ReadyToUseUIDemo.Droid
{
    public class ImageLoader
    {
        public static ImageLoader Instance;

        BitmapFactory.Options options;
        IO.Scanbot.Sdk.ScanbotSDK SDK;
        public ImageLoader(Context context)
        {
            options = new BitmapFactory.Options();
            SDK = new IO.Scanbot.Sdk.ScanbotSDK(context);
        }

        public Bitmap Load(Android.Net.Uri uri)
        {
            return SDK.FileIOProcessor().ReadImage(uri, options);
        }
    }
}
