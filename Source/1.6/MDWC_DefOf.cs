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

    // The two actives, for finding a pilot's own instance among the worn apparel's granted
    // abilities (JobDriver_MedicCast.FindAbility): the armor's dressing, whose job fires the
    // shoulders' shot as it ends, and the shot itself.
    public static AbilityDef MDWC_MechaniteDressing;
    public static AbilityDef MDWC_AdrenalineShot;

    // The shot's effect (HediffDefs/Adrenaline.xml).
    public static HediffDef MDWC_Adrenaline;

    // Vanilla defs without a DefOf handle of their own: the heart attack the shot stops, and
    // the injection sound it plays (go-juice's ingestSound).
    public static HediffDef HeartAttack;
    public static SoundDef Ingest_Inject;

    // The two drugs the shot's reload item may be besides a medicine (MedicWarcasketSettings'
    // shotAmmo), listed after the medicines in that order.
    public static ThingDef GoJuice;
    public static ThingDef WakeUp;

    // The dressing's motes (1.6/Defs/Effects/): the emitter glow in front of the pilot, the
    // beam between the two and the silhouette overlay on the patient. The beam's colour is also
    // the gauge's (Gizmo_DressingSuite).
    public static ThingDef MDWC_Mote_DressingGlow;
    public static ThingDef MDWC_Mote_DressingBeam;
    public static ThingDef MDWC_Mote_DressingOverlay;

    static MDWC_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MDWC_DefOf));
}
