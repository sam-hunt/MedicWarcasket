using HarmonyLib;
using RimWorld;
using Verse;

namespace MedicWarcasket.Patches;

// The medicine suite (CompDressingSuite) bends the one vanilla reload query that must
// follow its selected type, and guards the one that could load behind its back. Both are
// non-virtual on CompApparelReloadable, so the subclass cannot override them; every other
// reloadable passes through untouched.
//
// AmmoDef is read by the automatic reload (JobGiver_Reload through ReloadableUtility), the
// float-menu reload (FloatMenuOptionProvider_Reload offers a stack only when its def is the
// AmmoDef), DisabledReason, the stat sheet's reload line and the carry-space check, so this
// one prefix makes all of them fetch, offer and name the selected medicine. The remaining
// queries (NeedsReload, MinAmmoNeeded, MaxAmmoNeeded, the ReloadFrom clamp) read only
// remainingCharges and MaxCharges, which the suite keeps equal to the selected type's count,
// so they are right unpatched.
//
// ReloadFrom loads whatever stack it is handed and adds to remainingCharges, that is to the
// selected bucket. Vanilla's own paths only ever hand it the AmmoDef, so the postfix is a
// guard for a foreign reload path: charges added from a stack of another medicine are moved
// to that medicine's bucket.
[HarmonyPatch(typeof(CompApparelReloadable))]
public static class CompApparelReloadable_Suite
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CompApparelReloadable.AmmoDef), MethodType.Getter)]
    public static bool AmmoDef(CompApparelReloadable __instance, ref ThingDef __result)
    {
        if (__instance is not CompDressingSuite suite)
            return true;
        __result = suite.Selected;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CompApparelReloadable.ReloadFrom))]
    public static void ReloadFromPrefix(CompApparelReloadable __instance, out int __state) =>
        __state = __instance is CompDressingSuite ? __instance.RemainingCharges : 0;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CompApparelReloadable.ReloadFrom))]
    public static void ReloadFromPostfix(CompApparelReloadable __instance, Thing ammo, int __state)
    {
        if (__instance is not CompDressingSuite suite || ammo?.def == null)
            return;
        int added = suite.RemainingCharges - __state;
        if (added > 0 && ammo.def != suite.Selected)
            suite.MoveLoaded(ammo.def, added);
    }
}
