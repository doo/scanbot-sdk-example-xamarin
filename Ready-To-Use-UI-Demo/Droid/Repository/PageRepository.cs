using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using IO.Scanbot.Imagefilters;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.Process;
using ScanbotSDK.Xamarin.Android;

namespace ReadyToUseUIDemo.Droid.Repository
{
    public class PageRepository
    {
        static readonly List<Page> items = new List<Page>();

        public static List<Page> Pages { get => items; }

        public static void Remove(Context context, Page page)
        {
            SBSDK.PageStorage.Remove(page.PageId);
            items.Remove(page);
        }

        public static void Add(List<Page> pages)
        {
            items.AddRange(pages);
        }

        public static void Add(Page page)
        {
            items.Add(page);
        }

        public static Page Update(Page page)
        {
            var existing = items.Where(p => p.PageId == page.PageId).ToList()[0];
            items.Remove(existing);

            items.Add(page);

            return page;
        }

        public static void Clear()
        {
            SBSDK.PageStorage.RemoveAll();
            items.Clear();
        }

        public static void Apply(LegacyFilter filter)
        {
            var temp = new List<Page>();

            foreach(Page page in items)
            {
                SBSDK.PageProcessor.ApplyFilter(page, filter);
                var applied = new Page(page.PageId, page.Polygon, page.DetectionStatus, filter);
                temp.Add(applied);
            }

            items.Clear();
            items.AddRange(temp);
        }

        public static Page Apply(LegacyFilter filter, Page page)
        {
            foreach(Page item in items)
            {
                if (page.PageId == item.PageId)
                {
                    SBSDK.PageProcessor.ApplyFilter(item, filter);
                    SBSDK.PageProcessor.GenerateFilteredPreview(item, filter);
                }
            }

            var result = new Page(page.PageId, page.Polygon, page.DetectionStatus, filter);
            Update(result);
            return result;
        }

        public static Android.Net.Uri FindUri(Page page)
        {
            var path = GetUri(page, PageFileStorage.PageFileType.Document);
            var original = GetUri(page, PageFileStorage.PageFileType.Original);

            if (File.Exists(path.Path))
            {
                return path;
            }

            return original;
        }

        static Android.Net.Uri GetUri(Page page, PageFileStorage.PageFileType type)
        {
            return SBSDK.PageStorage.GetPreviewImageURI(page.PageId, type);
        }
    }
}
