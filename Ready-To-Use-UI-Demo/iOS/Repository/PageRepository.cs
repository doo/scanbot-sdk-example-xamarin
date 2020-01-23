using System;
using System.Collections.Generic;
using System.Linq;
using ScanbotSDK.iOS;
using UIKit;

namespace ReadyToUseUIDemo.iOS.Repository
{
    public class PageRepository
    {
        public static SBSDKUIPage Current { get; set; }

        public static List<SBSDKUIPage> Items { get; private set; } = new List<SBSDKUIPage>();
        
        static readonly SBSDKUIPageFileStorage storage = new SBSDKUIPageFileStorage(SBSDKImageFileFormat.Png);

        public static void Remove(SBSDKUIPage page)
        {
            storage.RemovePageFileID(page.PageFileUUID);
            Items.Remove(page);
        }

        public static void Add(List<SBSDKUIPage> pages)
        {
            Items.AddRange(pages);
        }

        public static void Add(SBSDKUIPage page)
        {
            Items.Add(page);
        }

        public static SBSDKUIPage Add(UIImage image, SBSDKPolygon polygon)
        {
            var page = new SBSDKUIPage(image, polygon, SBSDKImageFilterType.None);
            Add(page);
            return page;
        }

        public static void Update(SBSDKUIPage page)
        {
            var existing = Items.Where(p => p.PageFileUUID == page.PageFileUUID).ToList()[0];
            Items.Remove(existing);

            Items.Add(page);
        }

        public static void UpdateCurrent(UIImage image, SBSDKPolygon polygon)
        {
            var page = new SBSDKUIPage(image, polygon, Current.Filter);
            
            Remove(Current);
            Add(page);
            Current = page;
        }

        public static void Clear()
        {
            storage.RemoveAll();
            Items.Clear();
        }

        public static void Apply(SBSDKImageFilterType filter)
        {
            foreach (SBSDKUIPage page in Items)
            {
                page.Filter = filter;
            }
        }

        public static SBSDKUIPage Apply(SBSDKImageFilterType filter, SBSDKUIPage page)
        {
            foreach (SBSDKUIPage item in Items)
            {
                if (page.PageFileUUID == item.PageFileUUID)
                {
                    item.Filter = filter;
                    return item;
                }
            }

            return null;
        }

        public static SBSDKUIPage DuplicateCurrent(SBSDKImageFilterType type)
        {
            return new SBSDKUIPage(Current.OriginalImage, Current.Polygon, type);
        }
    }
}
