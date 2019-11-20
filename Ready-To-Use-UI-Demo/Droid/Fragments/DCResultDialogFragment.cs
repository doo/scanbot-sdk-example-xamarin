using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Dcscanner.Model;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using Newtonsoft.Json;
using System.Linq;
using Android.Support.V7.App;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class DCResultDialogFragment : DialogFragment
    {
        public const string NAME = "DCResultDialogFragment";
        public const string WORKFLOW_EXTRA = "WORKFLOW_EXTRA";
        public const string WORKFLOW_RESULT_EXTRA = "WORKFLOW_RESULT_EXTRA";

        private Workflow workflow;
        private List<WorkflowStepResult> results;

        public static DCResultDialogFragment CreateInstance(Workflow flow, List<WorkflowStepResult> results)
        {
            var fragment = new DCResultDialogFragment();

            var args = new Bundle();
            args.PutParcelable(WORKFLOW_EXTRA, flow);
            args.PutString(WORKFLOW_RESULT_EXTRA, JsonConvert.SerializeObject(results));
            fragment.Arguments = args;

            return fragment;
        }

        private View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            workflow = (Workflow)Arguments.GetParcelable(WORKFLOW_EXTRA);

            string json = Arguments.GetString(WORKFLOW_RESULT_EXTRA);
            results = JsonConvert.DeserializeObject<List<WorkflowStepResult>>(json);

            var view = inflater.Inflate(Resource.Layout.fragment_workflow_result_dialog, container);
            var title = (TextView)view.FindViewById(Resource.Id.title);
            title.Text = "asdf";

            if (results.Count == 0)
            {
                return view;
            }

            var result = results[0] as DisabilityCertificateWorkflowStepResult;
            if (result.Step is ScanDisabilityCertificateWorkflowStep)
            {
                var tv = (TextView)view.FindViewById(Resource.Id.tv_data);
                tv.Text = ParseResult(result.DisabilityCertificateResult);
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
