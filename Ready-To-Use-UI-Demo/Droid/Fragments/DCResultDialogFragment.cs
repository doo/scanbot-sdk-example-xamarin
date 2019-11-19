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

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class DCResultDialogFragment : DialogFragment
    {
        const string NAME = "DCResultDialogFragment";
        const string WORKFLOW_EXTRA = "WORKFLOW_EXTRA";
        const string WORKFLOW_RESULT_EXTRA = "WORKFLOW_RESULT_EXTRA";

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

        string ParseResult(DisabilityCertificateRecognizerResultInfo result)
        {
            var builder = new StringBuilder();
            builder.Append("Type: ");

            var test = result.Checkboxes;

            return builder.ToString();
        }
    }
}
