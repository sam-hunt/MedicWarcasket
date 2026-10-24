using RimWorld;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

[DefOf]
public static class MDWC_DefOf
{
    // The piece whose wearing lets the mechanite dressing treat diseases and infections
    // (Ability_MechaniteDressing.TreatsDiseases): a def comparison on the pilot's worn apparel,
    // the Controller helmet's idiom in VFEP.
    public static ThingDef MDWC_WarcasketHelmet_Medic;

    // The dressing, for the job driver to find the pilot's own instance among the worn armor's
    // granted abilities when a queued dressing starts (JobDriver_MechaniteDressing).
    public static AbilityDef MDWC_MechaniteDressing;

    // The dressing's motes (1.6/Defs/Effects/): the emitter glow in front of the pilot, the
    // beam between the two, the silhouette overlay on the patient and the flash when a beam
    // completes.
    public static ThingDef MDWC_Mote_DressingGlow;
    public static ThingDef MDWC_Mote_DressingBeam;
    public static ThingDef MDWC_Mote_DressingOverlay;
    public static ThingDef MDWC_Mote_DressingFlash;

    static MDWC_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MDWC_DefOf));
}
