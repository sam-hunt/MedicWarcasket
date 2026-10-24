using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MedicWarcasket;

// The armor's mechanite dressing suite (see CompDressingSuite). The reload fields (maxCharges,
// ammoDef, ammoCountPerCharge, chargeNoun, ...) are vanilla CompApparelReloadable's; a charge
// is one kit of the ammoDef medicine. Values live in the armor def, rationale in its header;
// maxCharges and ammoDef are mod settings defaulting to them, written back here.
public class CompProperties_DressingSuite : CompProperties_ApparelReloadable
{
    // Injury severity one kit treats, vanilla's per-treatment cap
    // (TendUtility.GetOptimalHediffsToTendWithSingleTreatment's 20). A non-injury hediff costs
    // a whole kit whatever its severity.
    public float severityPerKit = 20f;

    public CompProperties_DressingSuite()
    {
        compClass = typeof(CompDressingSuite);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
            yield return error;
        if (!(severityPerKit > 0f))
            yield return $"severityPerKit must be positive, is {severityPerKit}";
    }
}
