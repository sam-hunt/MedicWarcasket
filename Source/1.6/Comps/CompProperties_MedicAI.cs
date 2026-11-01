using System.Collections.Generic;
using Verse;

namespace MedicWarcasket;

// A non-player wearer's use of the piece's active (CompMedicAI): one entry on the armor, for
// the dressing, and one on the shoulders, for the shot. Values live in each piece's def,
// rationale in its header; aiCasts is a mod setting defaulting to the def's value, written
// back here.
public class CompProperties_MedicAI : CompProperties
{
    // Whether a non-player wearer casts the piece's active at all.
    public bool aiCasts = true;

    // Ticks between checks, staggered by the piece's id.
    public int checkInterval = 60;

    // How far from the wearer, in cells, a patient is looked for.
    public float searchRadius = 40f;

    // Ticks since the wearer was last hurt, and since it last engaged a target, before it
    // leaves the fight to tend.
    public int quietTicks = 300;

    public CompProperties_MedicAI()
    {
        compClass = typeof(CompMedicAI);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (string error in base.ConfigErrors(parentDef))
            yield return error;
        if (checkInterval <= 0)
            yield return $"checkInterval must be positive, is {checkInterval}";
        if (!(searchRadius > 0f))
            yield return $"searchRadius must be positive, is {searchRadius}";
        if (quietTicks < 0)
            yield return $"quietTicks must not be negative, is {quietTicks}";
    }
}
