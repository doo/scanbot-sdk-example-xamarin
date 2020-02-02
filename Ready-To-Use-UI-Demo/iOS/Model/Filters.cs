using System;
using System.Collections.Generic;

namespace ReadyToUseUIDemo.iOS.Model
{
    public class Filters
    {
        public static readonly List<Filter> List = new List<Filter>
        {
            new Filter("None", ScanbotSDK.iOS.SBSDKImageFilterType.None),
            new Filter("Low Light Binarization", ScanbotSDK.iOS.SBSDKImageFilterType.LowLightBinarization),
            new Filter("Low Light Binarization 2", ScanbotSDK.iOS.SBSDKImageFilterType.LowLightBinarization2),
            new Filter("Edge Highlight", ScanbotSDK.iOS.SBSDKImageFilterType.EdgeHighlight),
            new Filter("Deep Binarization", ScanbotSDK.iOS.SBSDKImageFilterType.DeepBinarization),
            new Filter("Otsu Binarization", ScanbotSDK.iOS.SBSDKImageFilterType.OtsuBinarization),
            new Filter("Clean Background", ScanbotSDK.iOS.SBSDKImageFilterType.BackgroundClean),
            new Filter("Color Document", ScanbotSDK.iOS.SBSDKImageFilterType.ColorDocument),
            new Filter("Color", ScanbotSDK.iOS.SBSDKImageFilterType.Color),
            new Filter("Grayscale", ScanbotSDK.iOS.SBSDKImageFilterType.Gray),
            new Filter("Binarized", ScanbotSDK.iOS.SBSDKImageFilterType.Binarized),
            new Filter("Pure Binarized", ScanbotSDK.iOS.SBSDKImageFilterType.PureBinarized),
            new Filter("Black & White", ScanbotSDK.iOS.SBSDKImageFilterType.BlackAndWhite)
        };
    }
}
