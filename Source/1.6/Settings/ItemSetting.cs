using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// One reloadable's ammo item: a ThingDef picked from a float menu, what a Tunable is for a
// number. Two instances, built by the settings: the suite's medicine and the injector's reload
// item, each with its own key, eligibility rule and menu order. Until the player picks one it
// is DefaultValue, the comp's ammoDef as the def XML (and any patch) leaves it, read by the
// settings' LoadDefaultsFromDefs. Only an override is saved, under the key, by defName, so it
// outlives the def instances a play-data reload replaces; picking the default clears it, and a
// saved name that no longer resolves to an eligible item (its mod removed, or patched out of
// the rule) reads as the default. The row's label and tooltip are "MDWC_" + key and that with
// "Desc", as a Tunable's are.
public sealed class ItemSetting
{
    private const float LabelShare = 0.4f;
    private const float ButtonPad = 10f;
    private const float IconPad = 4f;

    private readonly string key;
    private readonly Func<ThingDef, bool> isEligible;
    private readonly Func<ThingDef, float> rank;
    private string defName;

    // The menu lists every def isEligible accepts, highest rank first, ties in database order.
    public ItemSetting(string key, Func<ThingDef, bool> isEligible, Func<ThingDef, float> rank)
    {
        this.key = key;
        this.isEligible = isEligible;
        this.rank = rank;
    }

    public ThingDef DefaultValue { get; set; }

    public ThingDef Value
    {
        get
        {
            if (defName == null)
                return DefaultValue;
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            return def != null && isEligible(def) ? def : DefaultValue;
        }
    }

    // Built on each request rather than cached, so it never holds def instances a play-data
    // reload has replaced; the menu asks for it once per click.
    public List<ThingDef> Eligible() =>
        DefDatabase<ThingDef>.AllDefsListForReading
            .Where(isEligible)
            .OrderByDescending(rank)
            .ToList();

    public void Reset() => defName = null;

    public void ExposeData() => Scribe_Values.Look(ref defName, key);

    // Laid out as a Tunable row: the label on the left, then a button across the slider and
    // value columns showing the item's icon and label, with "(default)" while unset, that opens
    // the menu of eligible items. The description with the default's label is the tooltip of
    // the row up to the button, not over it: a float menu opens with its corner at the cursor,
    // which leaves the cursor on the row and a row-wide tip drawn over the menu.
    public void DoRow(Listing_Standard listing)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Rect labelRect = row.LeftPart(LabelShare);
        var buttonRect = new Rect(labelRect.xMax + ButtonPad, row.y, row.xMax - labelRect.xMax - ButtonPad, row.height);

        Widgets.DrawHighlightIfMouseover(row);
        if (DefaultValue != null)
            TooltipHandler.TipRegion(new Rect(row.x, row.y, buttonRect.xMin - row.x, row.height), ("MDWC_" + key + "Desc").Translate(DefaultValue.LabelCap));

        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, ("MDWC_" + key).Translate());
        Text.Anchor = anchor;

        if (Widgets.ButtonText(buttonRect, string.Empty))
            OpenMenu();
        ThingDef item = Value;
        if (item != null)
        {
            Rect content = buttonRect.ContractedBy(IconPad);
            if (item.uiIcon != null)
            {
                GUI.DrawTexture(content.LeftPartPixels(content.height), item.uiIcon);
                content.xMin += content.height + IconPad;
            }
            string shown = item.LabelCap;
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
