using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// The armor's medicine suite: vanilla's CompApparelReloadable holding up to maxCharges doses of
// EACH medicine type, one type selected at a time. The field dressing (Ability_MechaniteDressing)
// spends a dose of the type that was selected when its target was clicked.
//
// The selected type's count IS the vanilla charge count. remainingCharges is kept equal to
// loaded[selected] at all times, so every non-virtual vanilla reload path (NeedsReload,
// ReloadFrom's clamp, MinAmmoNeeded, MaxAmmoNeeded, CanBeUsed, LabelRemaining, the stat
// sheet) is right for the selected type unpatched: the suite wants a reload exactly when the
// selected type is below full, and a reload fills the selected type whatever the others hold.
// The one vanilla reader that must follow the selection is the AmmoDef getter, which
// Patches/CompApparelReloadable_Suite prefixes; the automatic reload, the float-menu reload,
// DisabledReason and the stat sheet's reload line all go through it.
//
// Reconcile() runs before every read and folds remainingCharges back into the selected
// bucket, so anything that writes remainingCharges behind the comp's back (the dev "reload to
// full" gizmo, UsedOnce from another mod, a lowered maxCharges setting) is attributed to the
// selected type rather than lost. Buckets above a lowered maximum are trimmed on load, on
// equip and on a settings write (MedicWarcasketSettings.TrimWornSuites), as Shipcracker's tank
// is, since vanilla's reload accounting breaks on an overfill.
//
// There is no unload: a dose leaves the suite only by being spent. Types are every def with a
// MedicalPotency stat base (ThingDef.IsMedicine), potency descending, so a modded medicine
// lists itself; counts default to zero. The free first fill (PostPostMake, vanilla's full
// charge on creation) lands on the def's ammoDef, the type the selection starts from.
//
// Buckets are scribed by defName rather than by def reference so a medicine whose mod has
// since been removed drops out silently, taking its doses with it, instead of logging a
// null-key error on load. Declared by compClass on the armor's CompProperties_ApparelReloadable
// entry; the DefInjected handle of that entry's text fields is this class's name.
public class CompDressingSuite : CompApparelReloadable
{
    private ThingDef selected;
    private Dictionary<ThingDef, int> loaded = new();

    // Scribe scratch: the buckets and the selection go out by defName (see the header).
    private string selectedType;
    private List<string> loadedTypes;
    private List<int> loadedCounts;

    // Every medicine the game knows, most potent first. Not cached: a mid-session play-data
    // reload replaces every def, and the callers (the gizmo's float menu, the inspect string)
    // are rare.
    public static IEnumerable<ThingDef> Types =>
        DefDatabase<ThingDef>.AllDefsListForReading
            .Where(d => d.IsMedicine)
            .OrderByDescending(d => d.GetStatValueAbstract(StatDefOf.MedicalPotency));

    private ThingDef SelectedRaw => selected ?? Props.ammoDef;

    public ThingDef Selected
    {
        get
        {
            Reconcile();
            return SelectedRaw;
        }
    }

    public int Loaded(ThingDef type)
    {
        Reconcile();
        return type != null && loaded.TryGetValue(type, out int count) ? count : 0;
    }

    public bool HasDose(ThingDef type) => Loaded(type) > 0;

    // Switching shows the new type's count on the gizmo and makes the next reload fetch that
    // type; the pending cast of a dressing already ordered keeps the type it was clicked with.
    public void Select(ThingDef type)
    {
        Reconcile();
        if (type == null || type == SelectedRaw)
            return;
        selected = type;
        remainingCharges = loaded.TryGetValue(type, out int count) ? count : 0;
    }

    public bool TryConsume(ThingDef type)
    {
        Reconcile();
        if (type == null || !loaded.TryGetValue(type, out int count) || count <= 0)
            return false;
        loaded[type] = count - 1;
        if (type == SelectedRaw)
            remainingCharges = count - 1;
        return true;
    }

    // Re-attributes charges a foreign reload path added under the selected type to the type
    // it actually loaded (Patches/CompApparelReloadable_Suite's ReloadFrom postfix). Doses
    // above the destination's maximum are lost, as the ammo already is.
    public void MoveLoaded(ThingDef to, int count)
    {
        Reconcile();
        if (to == null || count <= 0 || to == SelectedRaw)
            return;
        ThingDef from = SelectedRaw;
        int moved = Mathf.Min(count, loaded[from]);
        loaded[from] -= moved;
        remainingCharges = loaded[from];
        loaded[to] = Mathf.Min(MaxCharges, Loaded(to) + moved);
    }

    // Drops whatever no longer fits a lowered maximum, every bucket included.
    public void Trim()
    {
        Reconcile();
        foreach (ThingDef type in loaded.Keys.ToList())
            if (loaded[type] > MaxCharges)
                loaded[type] = MaxCharges;
        remainingCharges = loaded[SelectedRaw];
    }

    private void Reconcile()
    {
        if (remainingCharges > MaxCharges)
            remainingCharges = MaxCharges;
        loaded[SelectedRaw] = remainingCharges;
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        selected = Props.ammoDef;
        Reconcile();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            Reconcile();
            selectedType = selected?.defName;
            loadedTypes = loaded.Keys.Select(d => d.defName).ToList();
            loadedCounts = loaded.Values.ToList();
        }
        Scribe_Values.Look(ref selectedType, "selected");
        Scribe_Collections.Look(ref loadedTypes, "loadedTypes", LookMode.Value);
        Scribe_Collections.Look(ref loadedCounts, "loadedCounts", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            loaded = new Dictionary<ThingDef, int>();
            if (loadedTypes != null && loadedCounts != null)
                for (int i = 0; i < loadedTypes.Count && i < loadedCounts.Count; i++)
                {
                    ThingDef type = Medicine(loadedTypes[i]);
                    if (type != null)
                        loaded[type] = loadedCounts[i];
                }
            selected = Medicine(selectedType);
            // A save from this comp whose selected type is gone falls back to the def's type
            // with that type's own count; vanilla's charge count was the vanished type's. A
            // save from before the comp existed (no lists) keeps vanilla's full charge instead.
            if (selected != null || loadedTypes != null)
                remainingCharges = loaded.TryGetValue(SelectedRaw, out int count) ? count : 0;
            Trim();
        }
        // The scratch lives from Saving (written then) and from LoadingVars through the
        // cross-reference pass to PostLoadInit (read there), never longer.
        if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            selectedType = null;
            loadedTypes = null;
            loadedCounts = null;
        }
    }

    // A saved defName back to its def, or null when the def is gone or no longer a medicine.
    private static ThingDef Medicine(string defName)
    {
        ThingDef def = defName == null ? null : DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        return def != null && def.IsMedicine ? def : null;
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        Trim();
    }

    // "Loaded doses: medicine 5 / 5, glitterworld medicine 2 / 5" over the types holding any,
    // in place of vanilla's single remaining-charges line, which the gizmo already shows for
    // the selected type.
    public override string CompInspectStringExtra()
    {
        Reconcile();
        var parts = new StringBuilder();
        foreach (ThingDef type in Types)
        {
            int count = Loaded(type);
            if (count <= 0)
                continue;
            if (parts.Length > 0)
                parts.Append(", ");
            parts.Append("MDWC_LoadedDosesEntry".Translate(type.label, count, MaxCharges));
        }
        return "MDWC_LoadedDoses".Translate(parts.Length > 0 ? parts.ToString() : "MDWC_LoadedDosesNone".Translate().ToString());
    }
}
