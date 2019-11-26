
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.OS;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Mrzscanner.Model;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using ReadyToUseUIDemo.Droid.Repository;
using ReadyToUseUIDemo.Droid.Views;
using ReadyToUseUIDemo.model;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class MRZFrontBackImageResultDialogFragment : BaseDialogFragment
    {
        public const string NAME = "MRZFrontBackImageResultDialogFragment";

        public static MRZFrontBackImageResultDialogFragment CreateInstance(Workflow flow, List<WorkflowStepResult> results)
        {
            var f = new MRZFrontBackImageResultDialogFragment();

            var args = new Bundle();
            args.PutParcelable(WORKFLOW_EXTRA, flow);
            args.PutParcelableArrayList(WORKFLOW_RESULT_EXTRA, results.ToArray());
            f.Arguments = args;

            return f;
        }

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            workflow = (Workflow)Arguments.GetParcelable(WORKFLOW_EXTRA);
            stepResults = Arguments.GetParcelableArrayList(WORKFLOW_RESULT_EXTRA).Cast<WorkflowStepResult>().ToList();

            var view = inflater.Inflate(Resource.Layout.fragment_workflow_result_dialog, container);
            var title = (TextView)view.FindViewById(Resource.Id.title);
            title.Text = "Result";

            var frontResult = stepResults[0] as ContourDetectorWorkflowStepResult;
            var backResult = stepResults[1] as MachineReadableZoneWorkflowStepResult;

            if (frontResult.Step is ScanDocumentPageWorkflowStep)
            {
                view.FindViewById(Resource.Id.images_container).Visibility = ViewStates.Visible;
                view.FindViewById(Resource.Id.front_snap_result).Visibility = ViewStates.Visible;

                var uri = PageRepository.FindUri(frontResult.CapturedPage);
                view.FindViewById<ImageView>(Resource.Id.front_snap_result).SetImageURI(uri);
            }

            if (backResult.Step is ScanMachineReadableZoneWorkflowStep)
            {
                var text = view.FindViewById<TextView>(Resource.Id.tv_data);
                text.Text = ParseData(backResult.MrzResult);

                view.FindViewById(Resource.Id.images_container).Visibility = ViewStates.Visible;
                view.FindViewById(Resource.Id.front_snap_result).Visibility = ViewStates.Visible;

                var uri = PageRepository.FindUri(backResult.CapturedPage);
                view.FindViewById<ImageView>(Resource.Id.back_snap_result).SetImageURI(uri);
            }

            return view;
        }

        private string ParseData(MRZRecognitionResult result)
        {
            var builder = new StringBuilder();

            builder.Append(Texts.mrz_document_type).Append(" ").Append(result.TravelDocType.Name()).Append("\n");
            builder.Append(Texts.mrz_document_country).Append(" ").Append(result.NationalityField().Value).Append("\n");
            builder.Append(Texts.mrz_last_name).Append(" ").Append(result.LastNameField().Value).Append("\n");
            builder.Append(Texts.mrz_first_name).Append(" ").Append(result.FirstNameField().Value).Append("\n");
            builder.Append(Texts.mrz_document_code).Append(" ").Append(result.DocumentCodeField().Value).Append("\n");
            builder.Append(Texts.mrz_dob).Append(" ").Append(result.DateOfBirthField().Value).Append("\n");
            builder.Append(Texts.mrz_gender).Append(" ").Append(result.GenderField().Value).Append("\n");

            var validity = "Invalid";

            if (result.CheckDigitsCount == result.ValidCheckDigitsCount)
            {
                validity = "Valid";
            }

            builder.Append(Texts.mrz_checksums).Append(" ").Append(validity).Append("\n");

            return builder.ToString();
        }
    }
}
