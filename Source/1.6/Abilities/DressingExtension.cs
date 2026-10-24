using Verse;

namespace MedicWarcasket;

// The mechanite dressing's timings beyond what VEF's AbilityDef carries, as a plain
// DefModExtension on MDWC_MechaniteDressing: VEF filters modExtensions to its own
// AbilityExtension_AbilityMod type, so this one is invisible to it and read only by the
// dressing (Ability_MechaniteDressing, JobDriver_MechaniteDressing). Values live in the def
// XML, rationale in its header; ticksPerTend is a mod setting defaulting to it.
public class DressingExtension : DefModExtension
{
    // Ticks the beam spends on each wound; the first lands this long after the beam connects.
    public int ticksPerTend = 60;

    // Ticks the pilot holds still after the beam while the glow fades, before the job ends.
    public int warmdownTicks = 60;
}
