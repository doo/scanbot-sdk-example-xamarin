using System;
using System.Collections.Generic;
using Android.Runtime;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using Java.Lang;
using Net.Doo.Snap.Lib.Detector;
using ScanbotSDK.Xamarin.Android;

namespace ReadyToUseUIDemo.Droid
{
    public class WorkflowFactory
    {
        public static Workflow DisabilityCertificate
        {
            get
            {
                var ratios = new List<PageAspectRatio>
                {
                    // DC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                    new PageAspectRatio(148.0, 210.0),
                    // DC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
                    new PageAspectRatio(148.0, 105.0)
                };

                var steps = new List<ScanDisabilityCertificateWorkflowStep>();

                var handler = new DisabilityValidator();
                var step = new ScanDisabilityCertificateWorkflowStep(
                    "Please align the DC form in the frame.",
                    "",
                    ratios.ToArray(),
                    true,
                    new DisabilityValidator()
                );
                steps.Add(step);
                
                return new Workflow(steps.ToArray(), "Disability Certificate");
            }
        }
    }

    class DisabilityValidator : WorkflowValidator<DisabilityCertificateWorkflowStepResult>
    {
        public DisabilityValidator()
        {
        }

        public DisabilityValidator(IntPtr a, JniHandleOwnership b) : base(a, b)
        {
        }

        public override WorkflowStepError Invoke(DisabilityCertificateWorkflowStepResult result)
        {
            if (result.DisabilityCertificateResult == null)
            {
                return new WorkflowStepError(1, "No result available", WorkflowStepError.ShowMode.Toast);
            }

            if (!result.DisabilityCertificateResult.RecognitionSuccessful)
            {
                return new WorkflowStepError(1, "Recognition failed", WorkflowStepError.ShowMode.Toast);
            }

            return null;
        }
    }

}
