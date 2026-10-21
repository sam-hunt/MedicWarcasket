using UnityEngine;
using Verse;

namespace MedicWarcasket;

// Owns the mod settings only. The constructor runs before defs load, so it neither patches
// (ModInit's comment) nor touches the defs the settings override: ModInit, after PatchAll,
// reads their defaults (LoadDefaultsFromDefs) and applies the settings (ApplyToDefs) once defs
// exist, and WriteSettings re-applies them whenever the settings window closes.
public class MedicWarcasketMod : Mod
{
    public MedicWarcasketMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<MedicWarcasketSettings>();
    }

    public static MedicWarcasketSettings Settings { get; private set; }

    public override string SettingsCategory() => "MDWC_SettingsCategory".Translate();

    public override void DoSettingsWindowContents(Rect inRect) => Settings.DoWindowContents(inRect);

    public override void WriteSettings()
    {
        base.WriteSettings();
        Settings.ApplyToDefs();
    }
}
