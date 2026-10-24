using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// The suite's medicine: one ThingDef picked from a float menu, what a Tunable is for a number.
// Until the player picks one it is DefaultValue, the suite's ammoDef as the def XML (and any
// patch) leaves it, read by the settings' LoadDefaultsFromDefs. Only an override is saved, by
// defName, so it outlives the def instances a play-data reload replaces; picking the default
// clears it, and a saved name that no longer resolves to an eligible medicine (its mod removed,
// or patched below the tech floor) reads as the default.
public sealed class MedicineSetting
{
    private const string Key = "Medicine";
    private const float LabelShare = 0.4f;
    private const float ButtonPad = 10f;
    private const float IconPad = 4f;

    private string defName;

    public ThingDef DefaultValue { get; set; }

    public ThingDef Value
    {
        get
        {
            if (defName == null)
                return DefaultValue;
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            return def != null && IsEligible(def) ? def : DefaultValue;
        }
    }

    // Herbal is too crude for the carriers: industrial and above qualify, modded medicines by
    // their own tech level.
    private static bool IsEligible(ThingDef def) => def.IsMedicine && def.techLevel >= TechLevel.Industrial;

    // Built on each request rather than cached, so it never holds def instances a play-data
    // reload has replaced; the menu asks for it once per click.
    public static List<ThingDef> Eligible() =>
        DefDatabase<ThingDef>.AllDefsListForReading
            .Where(IsEligible)
            .OrderByDescending(d => d.GetStatValueAbstract(StatDefOf.MedicalPotency))
            .ToList();

    public void Reset() => defName = null;

    public void ExposeData() => Scribe_Values.Look(ref defName, Key);

    // Laid out as a Tunable row: "MDWC_Medicine" on the left, then a button across the slider
    // and value columns showing the medicine's icon and label, with "(default)" while unset,
    // that opens the menu of eligible medicines. "MDWC_MedicineDesc" with the default's label is
    // the whole row's tooltip.
    public void DoRow(Listing_Standard listing)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Rect labelRect = row.LeftPart(LabelShare);
        var buttonRect = new Rect(labelRect.xMax + ButtonPad, row.y, row.xMax - labelRect.xMax - ButtonPad, row.height);

        Widgets.DrawHighlightIfMouseover(row);
        if (DefaultValue != null)
            TooltipHandler.TipRegion(row, "MDWC_MedicineDesc".Translate(DefaultValue.LabelCap));

        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, "MDWC_Medicine".Translate());
        Text.Anchor = anchor;

        if (Widgets.ButtonText(buttonRect, string.Empty))
            OpenMenu();
        ThingDef medicine = Value;
        if (medicine != null)
        {
            Rect content = buttonRect.ContractedBy(IconPad);
            if (medicine.uiIcon != null)
            {
                GUI.DrawTexture(content.LeftPartPixels(content.height), medicine.uiIcon);
                content.xMin += content.height + IconPad;
            }
            string shown = medicine.LabelCap;
            if (defName == null)
                shown += "MDWC_DefaultSuffix".Translate();
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(content, shown.Truncate(content.width));
            Text.Anchor = anchor;
        }
        listing.Gap(Tunable.RowGap);
    }

    private void OpenMenu()
    {
        var options = new List<FloatMenuOption>();
        foreach (ThingDef def in Eligible())
        {
            string label = def.LabelCap;
            if (def == DefaultValue)
                label += "MDWC_DefaultSuffix".Translate();
            options.Add(new FloatMenuOption(label, () => defName = def == DefaultValue ? null : def.defName, def));
        }
        if (options.Count > 0)
            Find.WindowStack.Add(new FloatMenu(options));
    }
}
