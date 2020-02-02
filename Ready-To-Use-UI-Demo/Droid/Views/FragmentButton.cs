using System;
using Android.Content;
using Android.Widget;
using ReadyToUseUIDemo.model;

namespace ReadyToUseUIDemo.Droid.Views
{
    public class FragmentButton : Button
    {
        public ListItem Data { get; set; }

        public FragmentButton(Context context) : base(context)
        {
        }
    }
}
