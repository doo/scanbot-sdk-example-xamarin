#if __IOS__
using UIKit;
#else
using Android.Graphics;
#endif

namespace ReadyToUseUIDemo.model
{
    public class Colors
    {
        public static readonly
#if __IOS__
            UIColor
#else
            Color
#endif
            ScanbotRed = FromRgb(200, 25, 60);

        public static readonly
#if __IOS__
            UIColor
#else
            Color
#endif
            AppleBlue = FromRgb(10, 132, 255);

        public static readonly
#if __IOS__
            UIColor
#else
            Color
#endif
            LightGray = FromRgb(220, 220, 220);

        public static readonly
#if __IOS__
            UIColor
#else
            Color
#endif
            DarkGray = FromRgb(70, 70, 70);

        public static readonly
#if __IOS__
            UIColor
#else
            Color
#endif
            NearWhite = FromRgb(245, 245, 245);

        // Common function
        static
#if __IOS__
            UIColor
            #else
            Color
            #endif
            FromRgb(int red, int green, int blue)
        {
            #if __IOS__
            return UIColor.FromRGB(red, green, blue);
            #else
            return Color.Rgb(red, green, blue);
            #endif
        }
    }
}
