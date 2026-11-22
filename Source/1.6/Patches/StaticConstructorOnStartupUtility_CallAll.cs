using System;
using HarmonyLib;
using Verse;

namespace MedicWarcasket.Patches;

// Re-runs Startup.Run on every play-data load. A static constructor runs once per process, but
// a mid-session language switch (LanguageDatabase.SelectLanguage: ClearAllPlayData then
// LoadAllPlayData) rebuilds every def, so def-derived startup state must be re-applied; this
// hook fires at the moment the startup static constructors run, after defs, cross-refs, DefOf
// rebinding and DefInjected, on every load. The UniqueMeleeWeapons repo's copy of this patch
// carries the full decompile-verified rationale. On the first load the patch is applied from
// inside CallAll itself (ModInit is one of the constructors it runs), so that invocation never
// reaches the postfix; ModInit runs Startup.Run directly for it.
//
// The catch is this path's own: vanilla catches a throw from each static constructor, ModInit's
// among them, but one from a postfix would leave CallAll and reach the load pipeline's queue of
// finishing work.
[HarmonyPatch(typeof(StaticConstructorOnStartupUtility), nameof(StaticConstructorOnStartupUtility.CallAll))]
public static class StaticConstructorOnStartupUtility_CallAll
{
    public static void Postfix()
    {
        try
        {
            Startup.Run();
        }
        catch (Exception e)
        {
            Log.Error($"[Medic Warcasket] Startup failed on a play-data reload: {e}");
        }
    }
}
