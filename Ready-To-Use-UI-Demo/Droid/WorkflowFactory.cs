using System;
using System.Collections.Generic;
using Android.Runtime;
using IO.Scanbot.Mrzscanner.Model;
using IO.Scanbot.Sdk.Core.Contourdetector;
using IO.Scanbot.Sdk.UI.Entity.Workflow;
using Java.Lang;
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

        public static Workflow PayFormWithClassicalDocPolygonDetection
        {
            get
            {
                var steps = new List<ScanPayFormWorkflowStep>();

                var step = new ScanPayFormWorkflowStep(
                    "Please scan a SEPA PayForm",
                    "",
                    new List<PageAspectRatio>().ToArray(),
                    true,
                    new PayFormValidator()
                );
                steps.Add(step);

                return new Workflow(steps.ToArray(), "PayForm - Polygon Doc");
            }
        }

        public static Workflow ScanMRZAndSnap
        {
            get
            {
                var ratios = new List<PageAspectRatio>
                {
                    new PageAspectRatio(85.0, 54.0),
                    new PageAspectRatio(125.0, 88.0)
                };

                var steps = new List<ScanMachineReadableZoneWorkflowStep>();

                var step = new ScanMachineReadableZoneWorkflowStep(
                    "Scan ID card or passport",
                    "Please align the back of your ID card or passport in the frame",
                    ratios,
                    true,
                    new MRZValidator()
                );

                steps.Add(step);
                return new Workflow(steps.ToArray(), "Scanning MRZ Code");
            }
        }

        public static Workflow ScanMRZAndFrontBackSnap
        {
            get
            {
                var ratios = new List<PageAspectRatio>
                {
                    new PageAspectRatio(85.0, 54.0)
                };
                var steps = new List<WorkflowStep>();

                var step1 = new ScanDocumentPageWorkflowStep(
                    "Scan 1/2",
                    "Please scan the front side of your ID card",
                    ratios,
                    new DefaultWorkflowValidator()
                );

                var step2 = new ScanMachineReadableZoneWorkflowStep(
                    "Scan 2/2",
                    "Please scan the back side of your ID card",
                    ratios,
                    true,
                    new MRZValidator()
                );

                steps.Add(step1);
                steps.Add(step2);

                return new Workflow(steps.ToArray(), "Scanning MRZ Code & Both sides");
            }
        }

    }

    class MRZValidator : WorkflowValidator<MachineReadableZoneWorkflowStepResult>
    {
        public WorkflowStepError Invoke(Java.Lang.Object t)
        {
            var result = (MachineReadableZoneWorkflowStepResult)t;
            if (result.MrzResult == null)
            {
                return new WorkflowStepError(1, "No result available", WorkflowStepError.ShowMode.Toast);
            }

            if (!result.MrzResult.RecognitionSuccessful)
            {
                return new WorkflowStepError(2, "Recognition not successful", WorkflowStepError.ShowMode.Toast);
            }

            return null;
        }
    }

    class DisabilityValidator : WorkflowValidator<DisabilityCertificateWorkflowStepResult>
    {
        public WorkflowStepError Invoke(Java.Lang.Object t)
        {
            var result = (DisabilityCertificateWorkflowStepResult)t;
            if (result.DisabilityCertificateResult == null)
            {
                return new WorkflowStepError(1, "No result available", WorkflowStepError.ShowMode.Toast);
            }

            if (!result.DisabilityCertificateResult.RecognitionSuccessful)
            {
                return new WorkflowStepError(2, "Recognition failed", WorkflowStepError.ShowMode.Toast);
            }
            return null;
        }
    }

    class PayFormValidator : WorkflowValidator<PayFormWorkflowStepResult>
    {
        public WorkflowStepError Invoke(Java.Lang.Object t)
        {
            var result = (PayFormWorkflowStepResult)t;
            if (result.PayformResult == null)
            {
                return new WorkflowStepError(1, "No result available", WorkflowStepError.ShowMode.Toast);
            }

            if (result.PayformResult.PayformFields.Count == 0)
            {
                return new WorkflowStepError(2, "No payform fields found", WorkflowStepError.ShowMode.Toast);
            }

            return null;
        }
    }
}
