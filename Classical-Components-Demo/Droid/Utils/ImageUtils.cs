using System.IO;
using System.Collections.Generic;

using Android.Graphics;
using Android.App;
using Android.Net;
using IO.Scanbot.Sdk.Util.Bitmap;
using IO.Scanbot.Sdk.Util;

namespace ClassicalComponentsDemo.Droid
{
    public class ImageUtils
    {
        public static Bitmap LoadImage(string imagePath)
        {
            Bitmap bitmap = BitmapUtils.DecodeQuietly(imagePath, null);
            if (bitmap == null)
            {
                throw new IOException("Could not load image. Bitmap is null.");
            }
            return bitmap;
        }

        public static Bitmap LoadImage(Uri imageUri, Activity activity)
        {
            return LoadImage(FileChooserUtils.GetPath(activity, imageUri));
        }

        public static string[] GetSelectedImagesAsFilePaths(List<Uri> selectedImages, Activity activity)
        {
            var result = new List<string>();
            foreach (var androidUri in selectedImages)
            {
                result.Add(FileChooserUtils.GetPath(activity, androidUri));
            }
            return result.ToArray();
        }

        public static Bitmap GetThumbnail(Bitmap originalImage, float width, float height)
        {
            byte[] imageData = ResizeImage(originalImage, width, height, 70);
            return BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
        }

        public static Bitmap ResizeImage(Bitmap originalImage, float width, float height)
        {
            byte[] imageData = ResizeImage(originalImage, width, height, 70);
            return BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
        }

        public static byte[] ResizeImage(Bitmap originalImage, float width, float height, int quality)
        {
            float oldWidth = originalImage.Width;
            float oldHeight = originalImage.Height;
            float scaleFactor = 0f;

            if (oldWidth > oldHeight)
            {
                scaleFactor = width / oldWidth;
            }
            else
            {
                scaleFactor = height / oldHeight;
            }

            float newHeight = oldHeight * scaleFactor;
            float newWidth = oldWidth * scaleFactor;

            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
                return ms.ToArray();
            }
        }
    }
}
