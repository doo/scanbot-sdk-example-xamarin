
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

        DisabilityCertificateWorkflowStepResult result = null;

        public static DCResultDialogFragment CreateInstance(Workflow flow, DisabilityCertificateWorkflowStepResult result)
        {
            var fragment = new DCResultDialogFragment();
            fragment.workflow = flow;
            fragment.result = result;
            return fragment;
        }

        public override View AddContentView(LayoutInflater inflater, ViewGroup container)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_workflow_result_dialog, container);
            var title = (TextView)view.FindViewById(Resource.Id.title);
            title.Text = "Detected DC Form";

            if (result == null)
            {
                return view;
            }

            
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
            builder.Append(result.DcFormType).Append("\n");

            builder.Append("Checkboxes: \n");

            foreach (DisabilityCertificateInfoBox cb in result.Checkboxes)
            {
                string name = "Unknown";
                if (cb.SubType == DCInfoBoxSubtype.DCBoxInitialCertificate)
                    name = "Initial certificate";
                else if (cb.SubType == DCInfoBoxSubtype.DCBoxRenewedCertificate)
                    name = "Renewed certificate";
                else if (cb.SubType == DCInfoBoxSubtype.DCBoxAssignedToAccidentInsuranceDoctor)
                    name = "Assigned to accident insurance doctor";
                else if (cb.SubType == DCInfoBoxSubtype.DCBoxWorkAccident)
                    name = "Work accident";
                builder.AppendLine($"{name}: {(cb.HasContents ? "yes" : "no")}");
            }
            //builder.Append(result.Checkboxes.).Append("\n");

            foreach (DateRecord date in result.Dates)
            {
                string name = "Unknown";
                if (date.Type == DateRecordType.DateRecordDiagnosedOn)
                    name = "Diagnosed on";
                else if (date.Type == DateRecordType.DateRecordIncapableOfWorkSince)
                    name = "Incapacitated since";
                else if (date.Type == DateRecordType.DateRecordIncapableOfWorkUntil)
                    name = "Incapacitated until";
                builder.AppendLine($"{name}: {date.DateString}");
            }

            //foreach (DCPatientInfoField patientInfoField in result.PatientInfoFields)
            //{
            //    string name = "Unknow";
            //    if (patientInfoField.PatientInfoFieldType == DCPatientInfoFieldType.HealthInsuranceNumber)
            //        name = 

            //}

            ////builder.Append("Class: ");
            ////builder.Append(result.Class).Append("\n");

            ////builder.Append("InsuredPersonType: ");
            ////builder.Append(result.InsuredPersonType).Append("\n");
            ////builder.Append("Intention: ");
            ////builder.Append(result.Intention).Append("\n");
            ////builder.Append("PatientInfoBox: ");
            ////builder.Append(result.PatientInfoBox).Append("\n");
            //builder.Append("PatientInfoFields: ");
            //builder.Append(result.PatientInfoFields).Append("\n");
            //builder.Append("");
            
            var test = result.Checkboxes;

            return builder.ToString();
        }
    }
}
