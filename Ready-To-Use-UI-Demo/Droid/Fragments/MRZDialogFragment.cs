using System.Linq;
using System.Text;
using Android.OS;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Mrzscanner.Model;
using ReadyToUseUIDemo.Droid.Views;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class MRZDialogFragment : BaseDialogFragment
    {
        public const string MRZ_DATA = "MRZ_DATA";
        public const string NAME = "MRZDialogFragment";

        public static MRZDialogFragment CreateInstance(MRZGenericDocument data)
        {
            var fragment = new MRZDialogFragment();
            var args = new Bundle();
            args.PutParcelable(MRZ_DATA, data);
            fragment.Arguments = args;

            return fragment;
        }

        MRZGenericDocument result;

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            result = (MRZGenericDocument)Arguments.GetParcelable(MRZ_DATA);
            var view = inflater.Inflate(Resource.Layout.fragment_mrz_dialog, container);

            CopyText = ParseData(result);
            view.FindViewById<TextView>(Resource.Id.tv_data).Text = CopyText;
            return view;
        }

        private string ParseData(MRZGenericDocument result)
        {
            var builder = new StringBuilder();

            var description = string.Join(";\n", result?.Document?.Fields?
                .Where(field => field != null)
                .Select((field) =>
                {
                    string outStr = "";
                    if (field.GetType() != null && field.GetType().Name != null)
                    {
                        outStr += field.GetType().Name + " = ";
                    }
                    if (field.Value != null && field.Value.Text != null)
                    {
                        outStr += field.Value.Text;
                    }
                    return outStr;
                })
                .ToList()
            );
            return description;
        }
    }
}
