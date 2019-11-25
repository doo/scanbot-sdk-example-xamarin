using System;
using System.Text;
using Android.OS;
using Android.Support.V4.App;
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

        public static MRZDialogFragment CreateInstance(MRZRecognitionResult data)
        {
            var fragment = new MRZDialogFragment();
            var args = new Bundle();
            args.PutParcelable(MRZ_DATA, data);
            fragment.Arguments = args;

            return fragment;
        }

        MRZRecognitionResult result;

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            result = (MRZRecognitionResult)Arguments.GetParcelable(MRZ_DATA);
            var view = inflater.Inflate(Resource.Layout.fragment_mrz_dialog, container);
            view.FindViewById<TextView>(Resource.Id.tv_data).Text = ParseData(result);
            return view;
        }

        private string ParseData(MRZRecognitionResult result)
        {
            var builder = new StringBuilder();

            Append(builder, GetString(Resource.String.mrz_document_code), result.DocumentCodeField().Value);
            Append(builder, GetString(Resource.String.mrz_first_name), result.FirstNameField().Value);
            Append(builder, GetString(Resource.String.mrz_last_name), result.LastNameField().Value);
            Append(builder, GetString(Resource.String.mrz_issuing_organization), result.IssuingStateOrOrganizationField().Value);
            Append(builder, GetString(Resource.String.mrz_document_of_issue), result.DepartmentOfIssuanceField().Value);
            Append(builder, GetString(Resource.String.mrz_nationality), result.NationalityField().Value);
            Append(builder, GetString(Resource.String.mrz_dob), result.DateOfBirthField().Value);
            Append(builder, GetString(Resource.String.mrz_gender), result.GenderField().Value);
            Append(builder, GetString(Resource.String.mrz_date_expiry), result.DateOfExpiryField().Value);
            Append(builder, GetString(Resource.String.mrz_personal_number), result.PersonalNumberField().Value);
            Append(builder, GetString(Resource.String.mrz_optional1), result.Optional1Field().Value);
            Append(builder, GetString(Resource.String.mrz_optional2), result.Optional2Field().Value);
            Append(builder, GetString(Resource.String.mrz_discreet_issuing_organization), result.DiscreetIssuingStateOrOrganizationField().Value);
            Append(builder, GetString(Resource.String.mrz_valid_check_digits_count), result.ValidCheckDigitsCount.ToString());
            Append(builder, GetString(Resource.String.mrz_check_digits_count), result.CheckDigitsCount.ToString());
            Append(builder, GetString(Resource.String.travel_doc_type), result.TravelDocTypeField().Value);

            return builder.ToString();
        }

        void Append(StringBuilder builder, string key, string value)
        {
            builder.Append(key).Append(value).Append("\n");
        }
    }
}
