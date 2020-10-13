using System;
using Android.Content;
using Android.Graphics;
using Android.Provider;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class ImageUtils
    {
        public static Bitmap ProcessGalleryResult(Context context, Intent data)
        {
            // TODO: What in the world is the correct way to import images these days?
            return MediaStore.Images.Media.GetBitmap(context.ContentResolver, data.Data);

            var imageUri = data.Data;
            Bitmap bitmap = null;
            if (imageUri != null)
            {
                try
                {
                    var source = ImageDecoder.CreateSource(context.ContentResolver, imageUri);
                    bitmap = ImageDecoder.DecodeBitmap(source);
                }
                catch (Exception)
                {
                }
            }

            return bitmap;
        }
    }
}
