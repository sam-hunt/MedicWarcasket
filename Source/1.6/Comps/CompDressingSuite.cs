using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// The armor's medicine suite: vanilla's CompApparelReloadable whose charge is a kit, one item of
// the props' ammoDef (a mod setting), and whose kits are spent as a severity budget. The
// mechanite dressing (Ability_MechaniteDressing) pays for each wound with Draw: an injury costs
// its severity up to SeverityPerKit, anything else a whole kit.
//
// remainingCharges counts the kits loaded INCLUDING the open one, and openUsed is the severity
// already drawn from that kit (0 <= openUsed < SeverityPerKit, 0 while no kit is loaded). Since a
// partly used kit is still a charge, vanilla's capacity and reload arithmetic (ReloadFrom's
// clamp, MinAmmoNeeded, CanBeUsed, the dev "reload to full") can neither overfill the suite nor
// throw the rest of an open kit away. Draw closes a kit through UsedOnce, so vanilla's
// bookkeeping runs as for any spent charge, and carries the excess into the next; a shortfall on
// the last kit is forgiven, as vanilla never refuses the top wound its last medicine.
//
// Colonists reload to the gauge's target, TargetKits (-1 saved = full), rather than to full:
// NeedsReload and MaxAmmoNeeded are not virtual, so Patches/CompApparelReloadable_Reload bends
// them for this comp only. The open kit counts toward the target, so the suite reloads only once
// a whole kit is used up.
//
// Readouts: the gauge's bar label (Gizmo_DressingSuite), the inspect line, the tooltip line and
// the stat sheet's remaining-charges entry show the fractional count, Available. The gauge's fill
// alone shows whole kits, the open one included, since that is what TargetKits compares against.
// Gizmo_Slider reads its target once, on first draw, so the one cached gauge is rebuilt whenever
// the capacity or the settings generation moves under it.
//
// Trim restores the invariants after anything that can break them: a lowered maxCharges setting
// (vanilla's reload accounting breaks on an overfill), a severityPerKit lowered under a saved
// openUsed, and an empty suite left holding a partial kit. It runs on load, on equip, and from
// MedicWarcasketSettings.TrimWornSuites when the settings are written.
public class CompDressingSuite : CompApparelReloadable
{
    // Float drift allowance in severity: a kit within this of its budget closes, and an open
    // kit's draw under it reads as none.
    private const float Epsilon = 0.001f;

    // CompApparelVerbOwner_Charged's display order for its remaining-charges stat entry, which
    // the fractional one takes over.
    private const int VanillaChargesOrder = 2749;

    private float openUsed;

    // The gauge's reload target in kits; -1 until the player sets one, read as a full suite.
    private int targetKits = -1;

    [Unsaved]
    private Gizmo_DressingSuite gauge;

    [Unsaved]
    private int gaugeMaxCharges = -1;

    [Unsaved]
    private int gaugeGeneration = -1;

    public new CompProperties_DressingSuite Props => (CompProperties_DressingSuite)props;

    // Floored so no reader divides by zero on a def ConfigErrors has already reported.
    public float SeverityPerKit => Mathf.Max(Props.severityPerKit, 0.01f);

    // Severity already drawn from the kit in use; 0 while no kit is loaded.
    public float OpenUsed => openUsed;

    // What is loaded, in kits, the open kit counted by what is left of it.
    public float Available => remainingCharges - openUsed / SeverityPerKit;

    public bool HasKit => remainingCharges > 0;

    public bool HasOpenKit => remainingCharges > 0 && openUsed > 0f;

    // "2.6 / 5", the gauge's bar label and every other fractional readout.
    public string LabelAvailable => $"{Available:0.0} / {MaxCharges}";

    public int TargetKits
    {
        get => targetKits < 0 ? MaxCharges : Mathf.Min(targetKits, MaxCharges);
        set => targetKits = Mathf.Clamp(value, 0, MaxCharges);
    }

    // Never refuses: with no kit loaded it does nothing, and a cost beyond the last kit empties
    // the suite.
    public void Draw(float cost)
    {
        if (!(cost > 0f) || remainingCharges <= 0)
            return;
        openUsed += cost;
        while (remainingCharges > 0 && openUsed >= SeverityPerKit - Epsilon)
        {
            openUsed -= SeverityPerKit;
            UsedOnce();
        }
        if (remainingCharges <= 0 || openUsed < Epsilon)
            openUsed = 0f;
    }

    public void Trim()
    {
        if (remainingCharges > MaxCharges)
            remainingCharges = MaxCharges;
        if (float.IsNaN(openUsed) || openUsed < Epsilon)
            openUsed = 0f;
        // Closed directly rather than through UsedOnce: this is a correction, not a use.
        while (remainingCharges > 0 && openUsed >= SeverityPerKit - Epsilon)
        {
            openUsed -= SeverityPerKit;
            remainingCharges--;
        }
        if (remainingCharges <= 0 || openUsed < Epsilon)
            openUsed = 0f;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref openUsed, "openUsed");
        Scribe_Values.Look(ref targetKits, "targetKits", -1);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            Trim();
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        Trim();
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            yield return gizmo;
        if (gauge == null || gaugeMaxCharges != MaxCharges || gaugeGeneration != MedicWarcasketSettings.Generation)
        {
            gauge = new Gizmo_DressingSuite(this);
            gaugeMaxCharges = MaxCharges;
            gaugeGeneration = MedicWarcasketSettings.Generation;
        }
        yield return gauge;
    }

    public override string CompInspectStringExtra() =>
        "MDWC_KitsLoaded".Translate(Available.ToString("0.0"), MaxCharges);

    public override string CompTipStringExtra()
    {
        TaggedString label = "Stat_Thing_ReloadChargesRemaining_Name".Translate(base.Props.ChargeNounArgument).CapitalizeFirst();
        return $"\n\n{label}: {LabelAvailable}";
    }

    // Vanilla's integer remaining-charges entry (CompApparelVerbOwner_Charged) is swapped for
    // the same entry over the fractional count, recognised by its label; everything else the
    // base yields passes through.
    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        NamedArgument noun = base.Props.ChargeNounArgument;
        TaggedString label = "Stat_Thing_ReloadChargesRemaining_Name".Translate(noun);
        string labelCap = label.CapitalizeFirst();
        IEnumerable<StatDrawEntry> entries = base.SpecialDisplayStats();
        if (entries != null)
            foreach (StatDrawEntry entry in entries)
                if (entry.stat != null || entry.LabelCap != labelCap)
                    yield return entry;
        yield return new StatDrawEntry(StatCategoryDefOf.Apparel, label, LabelAvailable,
            "Stat_Thing_ReloadChargesRemaining_Desc".Translate(noun), VanillaChargesOrder);
        yield return new StatDrawEntry(StatCategoryDefOf.Apparel,
            "MDWC_StatSeverityPerKit".Translate(), SeverityPerKit.ToString("0.##"),
            "MDWC_StatSeverityPerKitDesc".Translate(), VanillaChargesOrder - 1);
    }
}
