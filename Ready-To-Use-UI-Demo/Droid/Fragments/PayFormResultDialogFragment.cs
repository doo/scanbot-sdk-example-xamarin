using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.OS;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Payformscanner.Model;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using ReadyToUseUIDemo.Droid.Views;

namespace ReadyToUseUIDemo.Droid.Fragments
{
    public class PayFormResultDialogFragment : BaseDialogFragment
    {
        public const string NAME = "PayFormResultDialogFragment";

        public static PayFormResultDialogFragment CreateInstance(Workflow flow, List<WorkflowStepResult> results)
        {
            var fragment = new PayFormResultDialogFragment();

            var args = new Bundle();
            args.PutParcelable(WORKFLOW_EXTRA, flow);
            args.PutParcelableArray(WORKFLOW_RESULT_EXTRA, results.ToArray());
            fragment.Arguments = args;

            return fragment;
        }

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            workflow = (Workflow)Arguments.GetParcelable(WORKFLOW_EXTRA);
            stepResults = Arguments.GetParcelableArrayList(WORKFLOW_RESULT_EXTRA).Cast<WorkflowStepResult>().ToList();

            var view = inflater.Inflate(Resource.Layout.fragment_workflow_result_dialog, container);
            var title = (TextView)view.FindViewById(Resource.Id.title);
            title.Text = "Detected SEPA pay form";

            var first = stepResults[0] as PayFormWorkflowStepResult;

            if (!(first is PayFormWorkflowStepResult))
            {
                return view;
            }

            CopyText = ParseData(first.PayformResult);

            var text = view.FindViewById<TextView>(Resource.Id.tv_data);
            text.Text = CopyText;

            return view;
        }

        string ParseData(PayFormRecognitionResult result)
        {
            if (result == null)
            {
                return "Unable to parse pay form, please try again";
            }

            var builder = new StringBuilder();
            foreach (var field in result.PayformFields)
            {
                builder.Append("• ");
                //builder.Append(field.Type.ToString());
                builder.Append(": ");
                //builder.Append(field.Value);
                builder.Append("\n");
            }
            return builder.ToString();
        }
    }
}
