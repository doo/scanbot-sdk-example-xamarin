
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Dcscanner.Model;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using Newtonsoft.Json;
using ReadyToUseUIDemo.Droid.Views;
using AndroidX.AppCompat.App;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class DCResultDialogFragment : BaseDialogFragment
    {
        public const string NAME = "DCResultDialogFragment";
        
        public static DCResultDialogFragment CreateInstance(Workflow flow, List<WorkflowStepResult> results)
        {
            var fragment = new DCResultDialogFragment();

            var args = new Bundle();
            args.PutParcelable(WORKFLOW_EXTRA, flow);
            args.PutParcelableArray(WORKFLOW_RESULT_EXTRA, results.ToArray());
            fragment.Arguments = args;

            return fragment;
        }

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            workflow = (Workflow)Arguments.GetParcelable(WORKFLOW_EXTRA);

            string json = Arguments.GetString(WORKFLOW_RESULT_EXTRA);
            stepResults = JsonConvert.DeserializeObject<List<WorkflowStepResult>>(json);

            var view = inflater.Inflate(Resource.Layout.fragment_workflow_result_dialog, container);
            var title = (TextView)view.FindViewById(Resource.Id.title);
            title.Text = "Detected DC Form";

            if (stepResults.Count == 0)
            {
                return view;
            }

            
            var result = stepResults[0] as DisabilityCertificateWorkflowStepResult;
            if (result.Step is ScanDisabilityCertificateWorkflowStep)
            {
                var tv = (TextView)view.FindViewById(Resource.Id.tv_data);
                CopyText = ParseResult(result.DisabilityCertificateResult);
                tv.Text = CopyText;
            }
            return view;
        }

        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var builder = new AlertDialog.Builder(Activity);
            var inflater = LayoutInflater.From(Activity);
            var container = (ViewGroup)inflater.Inflate(Resource.Layout.holo_dialog_frame, null, false);
            AddContentView(inflater, container);

            builder.SetView(container);

            builder.SetPositiveButton("Cancel", delegate
            {
                Dismiss();
            });

            builder.SetNegativeButton("Copy", delegate
            {
                Dismiss();
            });

            var dialog = builder.Create();
            dialog.SetCanceledOnTouchOutside(true);

            return dialog;
        }

        string ParseResult(DisabilityCertificateRecognizerResultInfo result)
        {
            var builder = new StringBuilder();
            builder.Append("Type: ");

            var test = result.Checkboxes;

            return builder.ToString();
        }
    }
}
