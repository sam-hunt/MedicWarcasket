using RimWorld;
using Verse;

namespace MedicWarcasket;

[DefOf]
public static class MDWC_DefOf
{
    // The piece whose wearing lets the field dressing treat diseases and infections
    // (Ability_FieldDressing.TreatsDiseases): a def comparison on the pilot's worn apparel,
    // the Controller helmet's idiom in VFEP.
    public static ThingDef MDWC_WarcasketHelmet_Medic;

    static MDWC_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MDWC_DefOf));
}
