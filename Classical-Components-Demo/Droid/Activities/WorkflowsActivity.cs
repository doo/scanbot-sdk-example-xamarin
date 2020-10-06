
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IO.Scanbot.Dcscanner.Model;
using IO.Scanbot.Mrzscanner.Model;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Core.Contourdetector;
using IO.Scanbot.Sdk.Core.Payformscanner.Model;
using IO.Scanbot.Sdk.Persistence;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using IO.Scanbot.Sdk.UI.View.Workflow;
using IO.Scanbot.Sdk.UI.View.Workflow.Configuration;
using ScanbotSDK.Xamarin.Android;

namespace ClassicalComponentsDemo.Droid.Activities
{
    public class DcWorkflowValidator : WorkflowValidator<DisabilityCertificateWorkflowStepResult>
    {
        public WorkflowStepError Invoke(Java.Lang.Object t)
        {
            var result = (DisabilityCertificateWorkflowStepResult)t;
            if (result.DisabilityCertificateResult == null || !result.DisabilityCertificateResult.RecognitionSuccessful)
            {
                return ErrorDialog(1, "This does not seem to be a valid certificate.");
            }
            return null;
        }
    }

    public class IdCardWorkflowValidator : WorkflowValidator<MachineReadableZoneWorkflowStepResult>
    {
        public WorkflowStepError Invoke(Java.Lang.Object t)
        {
            var result = (MachineReadableZoneWorkflowStepResult)t;
            if (result.MrzResult == null || !result.MrzResult.RecognitionSuccessful)
            {
                return ErrorDialog(1, "No MRZ zone detected. Point your camera at the backside of the ID card.");
            }
            else if (result.MrzResult.TravelDocType != MRZDocumentType.IDCard)
            {
                return ErrorDialog(2, "This does not seem to be an ID card.");
            }
            return null;
        }
    }

    [Activity(Label = "WorkflowsActivity")]
    public class WorkflowsActivity : Activity
    {
        const int REQUEST_WORKFLOW = 4714;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WorkflowsMenu);

            AssignWorkflowButtonHandlers();
        }

        void AddNewPage(Page page)
        {
            var container = FindViewById<LinearLayout>(Resource.Id.workflowResultsLayout);

            var imgView = new ImageView(this);
            imgView.SetPadding(10, 10, 10, 10);
            container.AddView(imgView);
            var docImgUri = SBSDK.PageStorage.GetPreviewImageURI(page.PageId, PageFileStorage.PageFileType.Document);
            var originalImgUri = SBSDK.PageStorage.GetPreviewImageURI(page.PageId, PageFileStorage.PageFileType.Original);
            var imgUri = File.Exists(docImgUri.Path) ? docImgUri : originalImgUri;
            imgView.Post(() =>
            {
                imgView.SetImageBitmap(ImageUtils.LoadImage(imgUri, this));
            });
        }

        void AddText(string text)
        {
            text = text.Trim();
            if (text == "")
            {
                return;
            }
            var container = FindViewById<LinearLayout>(Resource.Id.workflowResultsLayout);
            var label = new TextView(this);
            label.Text = text;
            container.AddView(label);
        }

        bool CheckScanbotSDKLicense()
        {
            if (SBSDK.IsLicenseValid())
            {
                // Trial period, valid trial license or valid production license.
                return true;
            }

            Toast.MakeText(this, "Scanbot SDK (trial) license has expired!", ToastLength.Long).Show();
            return false;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_WORKFLOW && resultCode == Result.Ok)
            {
                FindViewById<LinearLayout>(Resource.Id.workflowResultsLayout).RemoveAllViews();

                var resultList = data.GetParcelableArrayListExtra(WorkflowScannerActivity.WorkflowResultExtra);

                foreach (WorkflowStepResult result in resultList)
                {
                    if (result.CapturedPage != null)
                    {
                        AddNewPage(result.CapturedPage);
                    }

                    var descr = new System.Text.StringBuilder();
                    if (result is DisabilityCertificateWorkflowStepResult dcResult)
                    {
                        var r = dcResult.DisabilityCertificateResult;
                        descr.AppendLine($"Recognition successful: {r.RecognitionSuccessful}");
                        foreach (DisabilityCertificateInfoBox cb in r.Checkboxes)
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
                            descr.AppendLine($"{name}: {(cb.HasContents ? "yes" : "no")}");
                        }
                        foreach (DateRecord date in r.Dates)
                        {
                            string name = "Unknown";
                            if (date.Type == DateRecordType.DateRecordDiagnosedOn)
                                name = "Diagnosed on";
                            else if (date.Type == DateRecordType.DateRecordIncapableOfWorkSince)
                                name = "Incapacitated since";
                            else if (date.Type == DateRecordType.DateRecordIncapableOfWorkUntil)
                                name = "Incapacitated until";
                            descr.AppendLine($"{name}: {date.DateString}");
                        }
                    }
                    else if (result is BarCodeWorkflowStepResult barcodeResult)
                    {
                        foreach (var barcode in barcodeResult.BarcodeResults.BarcodeItems)
                        {
                            descr.AppendLine($"{barcode.BarcodeFormat.ToString()}:\n{barcode.Text}\n");
                        }
                    }
                    else if (result is MachineReadableZoneWorkflowStepResult mrzResult)
                    {
                        foreach (MRZField field in mrzResult.MrzResult.Fields)
                        {
                            descr.AppendLine($"{field.Name}: {field.Value}");
                        }
                    }
                    else if (result is PayFormWorkflowStepResult payformResult)
                    {
                        if (payformResult.PayformResult.PayformFields != null)
                        {
                            foreach (RecognizedField field in payformResult.PayformResult.PayformFields)
                            {
                                descr.AppendLine($"{field.TokenType}: {field.Value}");
                            }
                        }
                    }
                    AddText(descr.ToString());
                }
            }
        }

        private void StartWorkflow(Workflow workflow)
        {
            if (!CheckScanbotSDKLicense()) return;

            var config = new WorkflowScannerConfiguration();
            config.SetIgnoreBadAspectRatio(true);
            var intent = WorkflowScannerActivity.NewIntent(this, config, workflow, new Dictionary<Java.Lang.Class, Java.Lang.Class>());
            StartActivityForResult(intent, REQUEST_WORKFLOW);
        }

        private void StartWorkflow(string name, params WorkflowStep[] workflowSteps)
        {
            StartWorkflow(new Workflow(workflowSteps, name));
        }

        protected void AssignWorkflowButtonHandlers()
        {
            var dcAspectRatios = new[]
            {
                new PageAspectRatio(1.414, 1.0),
                new PageAspectRatio(0.707, 1.0),
                new PageAspectRatio(1.5715, 1.0)
            };
            FindViewById<Button>(Resource.Id.scanDisabilityCertificateButton).Click += delegate
            {
                var step = new ScanDisabilityCertificateWorkflowStep("DC", "Align the Disability Certificate in the frame.", dcAspectRatios, true, new DcWorkflowValidator());
                StartWorkflow("Disability Certificate", step);
            };

            FindViewById<Button>(Resource.Id.scanQrCodeAndDocumentButton).Click += delegate
            {
                var qrStep = new ScanBarCodeWorkflowStep("QR Code", "Please align the QR code in the frame", new[] { BarcodeFormat.QrCode }, null, new DefaultWorkflowValidator());
                var documentStep = new ScanDocumentPageWorkflowStep("Document", "Please scan a document", new PageAspectRatio[0], new DefaultWorkflowValidator());
                StartWorkflow("QR Code and document", qrStep, documentStep);
            };

            var idCardAspectRatios = new[] { new PageAspectRatio(85.60, 53.98) };
            FindViewById<Button>(Resource.Id.scanMrzButton).Click += delegate
            {
                var frontSideStep = new ScanDocumentPageWorkflowStep("ID Card 1/2", "Please scan the front of your ID card.", idCardAspectRatios, new DefaultWorkflowValidator());
                var backSideStep = new ScanMachineReadableZoneWorkflowStep("Id Card 2/2", "Please scan the back of your id card.", idCardAspectRatios, true, new IdCardWorkflowValidator());
                StartWorkflow("ID card", frontSideStep, backSideStep);
            };

            var sepaAspectRatios = new[] { new PageAspectRatio(15.0, 10.5) };
            FindViewById<Button>(Resource.Id.scanPayformButton).Click += delegate
            {
                var step = new ScanPayFormWorkflowStep("SEPA", "Align the SEPA Payform in the frame.", sepaAspectRatios, true, new DefaultWorkflowValidator());
                StartWorkflow("SEPA", step);
            };
        }
    }
}
