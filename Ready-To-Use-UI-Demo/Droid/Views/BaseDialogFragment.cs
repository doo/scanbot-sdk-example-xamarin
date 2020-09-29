using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using IO.Scanbot.Sdk.UI.Entity.Workflow;

namespace ReadyToUseUIDemo.Droid.Views
{
    public class BaseDialogFragment : DialogFragment
    {
        public const string WORKFLOW_EXTRA = "WORKFLOW_EXTRA";
        public const string WORKFLOW_RESULT_EXTRA = "WORKFLOW_RESULT_EXTRA";
        public const string SCANNER_RESULT_EXTRA = "SCANNER_RESULT_EXTRA";

        public string CopyText { get; set; }

        protected Workflow workflow;
        protected List<WorkflowStepResult> stepResults;

        public virtual View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            throw new Exception("AddContentView should be overridden");
        }

        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var builder = new AlertDialog.Builder(Activity);
            var inflater = LayoutInflater.From(Activity);
            var container = (ViewGroup)inflater.Inflate(Resource.Layout.holo_dialog_frame, null, false);
            AddContentView(inflater, container);

            builder.SetView(container);

            builder.SetPositiveButton("Close", delegate
            {
                Dismiss();
            });

            var dialog = builder.Create();
            dialog.SetCanceledOnTouchOutside(true);

            return dialog;
        }

    }
}
