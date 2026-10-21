namespace MedicWarcasket;

// Everything this mod reads off or writes onto the loaded defs at startup: the settings'
// defaults and overrides. Idempotent, and run on every play-data load, not only the first:
// ModInit's static constructor runs it once per process, and the postfix on
// StaticConstructorOnStartupUtility.CallAll (Patches/StaticConstructorOnStartupUtility_CallAll)
// again after every in-process reload, which replaces every def instance and would otherwise
// leave the settings unapplied on the fresh ones.
public static class Startup
{
    public static void Run()
    {
        MedicWarcasketMod.Settings.LoadDefaultsFromDefs();
        MedicWarcasketMod.Settings.ApplyToDefs();
    }
}
