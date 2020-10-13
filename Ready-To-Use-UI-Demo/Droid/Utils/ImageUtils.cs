using System;
using Android.Content;
using Android.Graphics;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class ImageUtils
    {
        public static Bitmap ProcessGalleryResult(Context context, Intent data)
        {
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
