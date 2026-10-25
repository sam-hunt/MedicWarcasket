using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// The pilot's work: one checkbox per work type the trait mirror leaves to the player
// (WorkProfile.Toggleable, the Caring work types), what a Tunable is for a number. Until the
// player flips one it is WorkProfile.OpenByDefault, the fixed rule the mirror applies on its
// own (Doctor open, Childcare and any modded Caring type blocked while VFEP blocks Caring),
// read live so the default follows whatever the loaded mods make of VFEP's trait. Only an
// override is saved, by the work type's defName, so it outlives the def instances a play-data
// reload replaces and a work type whose DLC or mod is off keeps its override for when it is
// back; flipping a box back to its default clears it. Written onto the trait by the settings'
// ApplyToDefs through WorkProfile.Apply.
public sealed class WorkSetting
{
    private const string Key = "Work";
    private const float DefaultPad = 4f;

    private Dictionary<string, bool> overrides = [];

    public bool IsOpen(WorkTypeDef work) =>
        overrides.TryGetValue(work.defName, out bool open) ? open : WorkProfile.OpenByDefault(work);

    // The toggleable work types the pilot may take on, in the work tab's order.
    public IEnumerable<WorkTypeDef> Open => WorkProfile.Toggleable.Where(IsOpen);

    // The work tab's name for a work type; a modded one without it shows its defName.
    public static string Name(WorkTypeDef work) =>
        work.labelShort.NullOrEmpty() ? work.defName : work.labelShort;

    public void Reset() => overrides.Clear();

    public void ExposeData()
    {
        Scribe_Collections.Look(ref overrides, Key, LookMode.Value, LookMode.Value);
        overrides ??= [];
    }

    // One checkbox row per toggleable work type, laid out as a Tunable row is: the work tab's
    // name for it on the left, the box at the row's end and "(default)" beside the box while
    // unset, on the side a slider row marks its value. A click anywhere on the row flips the
    // box. The work type's own description over "MDWC_WorkDesc" with the default is the whole
    // row's tooltip.
    public void DoRows(Listing_Standard listing)
    {
        foreach (WorkTypeDef work in WorkProfile.Toggleable)
            DoRow(listing, work);
    }

    private void DoRow(Listing_Standard listing, WorkTypeDef work)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Widgets.DrawHighlightIfMouseover(row);
        bool byDefault = WorkProfile.OpenByDefault(work);
        TooltipHandler.TipRegion(row, Tooltip(work, byDefault));

        bool open = IsOpen(work);
        Widgets.CheckboxLabeled(row, Name(work).CapitalizeFirst(), ref open);
        if (open == byDefault)
            overrides.Remove(work.defName);
        else
            overrides[work.defName] = open;
        if (!overrides.ContainsKey(work.defName))
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(new Rect(row.x, row.y, row.width - Widgets.CheckboxSize - DefaultPad, row.height), "MDWC_DefaultSuffix".Translate());
            Text.Anchor = anchor;
        }
        listing.Gap(Tunable.RowGap);
    }

    private static string Tooltip(WorkTypeDef work, bool byDefault)
    {
        string state = (byDefault ? "MDWC_WorkOpen" : "MDWC_WorkBlocked").Translate();
        string ours = "MDWC_WorkDesc".Translate(state);
        return work.description.NullOrEmpty() ? ours : work.description + "\n\n" + ours;
    }
}
