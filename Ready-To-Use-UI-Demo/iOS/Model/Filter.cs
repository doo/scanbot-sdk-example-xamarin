using System;
using ScanbotSDK.iOS;

namespace ReadyToUseUIDemo.iOS.Model
{
    public class Filter
    {
        public Filter()
        {

        }

        public Filter(string title, SBSDKImageFilterType type)
        {
            Title = title;
            Type = type;
        }

        string Title { get; set; }

        SBSDKImageFilterType Type { get; set; }
    }
}
