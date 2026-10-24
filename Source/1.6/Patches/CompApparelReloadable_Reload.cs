using HarmonyLib;
using RimWorld;

namespace MedicWarcasket.Patches;

// The medicine suite reloads to its gauge's target (CompDressingSuite.TargetKits) instead of to
// full, as Toxin's tox tank and a CompRefuelable's TargetFuelLevel do. Vanilla's reload queries
// are not virtual, so the comp cannot override them; every other reloadable passes through.
//
// NeedsReload gates JobGiver_Reload, the float menu, MinAmmoNeeded and ReloadFrom; false here
// zeroes the two amounts and makes the reload a no-op. MaxAmmoNeeded caps the medicine the job
// carries (JobGiver_Reload.MakeReloadJob), and ReloadFrom loads what was carried, so the suite
// lands on the target. ReloadFrom is left alone: its only cap is the suite's size, reached only if
// the pawn already held extra medicine when the job began. AmmoDef needs nothing, being the
// props' own field, which the medicine setting writes.
//
// remainingCharges counts the open kit, so a partly used kit counts toward the target and the
// suite reloads only once a whole kit is used up.
[HarmonyPatch(typeof(CompApparelReloadable))]
public static class CompApparelReloadable_Reload
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CompApparelReloadable.NeedsReload))]
    public static bool NeedsReload(CompApparelReloadable __instance, ref bool __result)
    {
        if (__instance is not CompDressingSuite suite)
            return true;
        __result = suite.AmmoDef != null && suite.RemainingCharges < suite.TargetKits;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CompApparelReloadable.MaxAmmoNeeded))]
    public static bool MaxAmmoNeeded(CompApparelReloadable __instance, ref int __result)
    {
        if (__instance is not CompDressingSuite suite)
            return true;
        __result = suite.AmmoDef == null
            ? 0
            : __instance.Props.ammoCountPerCharge * System.Math.Max(0, suite.TargetKits - suite.RemainingCharges);
        return false;
    }
}
