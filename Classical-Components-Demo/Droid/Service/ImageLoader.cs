using System;
using Android.Content;
using Android.Graphics;
using Android.Provider;

namespace ClassicalComponentsDemo.Droid
{
    public class ImageLoader
    {
        public static ImageLoader Instance;

        Context context;
        BitmapFactory.Options options;
        IO.Scanbot.Sdk.ScanbotSDK SDK;
        public ImageLoader(Context context)
        {
            this.context = context;
            options = new BitmapFactory.Options();
            SDK = new IO.Scanbot.Sdk.ScanbotSDK(context);
        }

        public Bitmap Load(Android.Net.Uri uri)
        {
            return SDK.FileIOProcessor().ReadImage(uri, options);
        }

        public Bitmap LoadFromMedia(Android.Net.Uri uri)
        {
            return MediaStore.Images.Media.GetBitmap(context.ContentResolver, uri);
        }
    }
}
