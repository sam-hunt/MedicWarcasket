using System;
using HarmonyLib;
using Verse;

namespace MedicWarcasket;

// Mod entry point. Applies every [HarmonyPatch] class in this assembly, then runs the
// def-derived startup work (Startup.Run: the settings' defaults and overrides) for this first
// load; later play-data reloads run it again through the CallAll postfix.
//
// Deliberately a [StaticConstructorOnStartup] rather than a Mod-subclass constructor:
// Mod constructors run BEFORE any defs are loaded, and applying a Harmony patch
// JIT-compiles the target and runs its declaring type's static constructor. This mod
// exists to extend another mod (VFE Pirates), so its patch targets will often live in
// VFEPirates.dll, and a VFEP type whose static ctor resolves defs would be permanently
// broken by an early patch. Static constructors on startup run after all defs have
// loaded, which makes foreign-target patches safe here without any deferral machinery,
// and is the first point the settings can reach the defs they override. Keep the patching here
// rather than in MedicWarcasketMod's constructor.
//
// The classes are patched one by one, each in its own try/catch, where Harmony.PatchAll would
// stop at the first that throws and leave every later class unpatched, and Startup.Run unrun
// with them. PatchAll is this same loop over the assembly's types without the catch. Only
// attributed types are handed to the class processor: it takes a static method named Prepare,
// Prefix, Postfix and the like for a patch method on any type it is given, and the game's own
// Harmony builds filter the same way (2.4 on RimWorld 1.6, 3.0 on 1.7; the 2.3 this compiles
// against does not). A failure costs the one class its patch and an error in the log.
[StaticConstructorOnStartup]
public static class ModInit
{
    static ModInit()
    {
        // The Harmony id only needs to be unique; convention is the mod's packageId.
        var harmony = new Harmony("shunter.medicwarcasket");
        // The assembly is named through a type of ours: Assembly.GetExecutingAssembly() has
        // returned Harmony's own assembly from a constructor another mod patched
        // (HarmonyRimWorld #52).
        foreach (Type type in AccessTools.GetTypesFromAssembly(typeof(ModInit).Assembly))
        {
            if (!type.IsDefined(typeof(HarmonyAttribute), true))
                continue;
            try
            {
                harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception e)
            {
                Log.Error($"[Medic Warcasket] Patch class {type.FullName} failed and was skipped: {e}");
            }
        }

        Startup.Run();
    }
}
