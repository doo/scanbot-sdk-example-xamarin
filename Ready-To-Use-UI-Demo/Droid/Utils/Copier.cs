using System;
using System.IO;
using Android.Content;

namespace ReadyToUseUIDemo.Droid.Utils
{
    public class Copier
    {
        const string SNAPPING_DOCUMENTS_DIR_NAME = "snapping_documents";

        public static void Copy(Context context, Android.Net.Uri uri)
        {
            var path = Path.Combine(context.GetExternalFilesDir(null).AbsolutePath, SNAPPING_DOCUMENTS_DIR_NAME);
            var file = Path.Combine(path, uri.LastPathSegment);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.Copy(uri.Path, file);
        }
    }
}
