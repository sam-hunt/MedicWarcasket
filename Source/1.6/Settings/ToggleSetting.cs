using UnityEngine;
using Verse;

namespace MedicWarcasket;

// One on/off def field, what a Tunable is for a number: the two CompProperties_MedicAI aiCasts
// flags, the armor's and the shoulders'. Until the player flips it the value is DefaultValue,
// the def field as the XML (and any patch) leaves it, read by the settings'
// LoadDefaultsFromDefs; only an override is saved, under the key, and flipping the box back to
// the default clears it. The row's label and tooltip are "MDWC_" + key and that with "Desc",
// as a Tunable's are; the tooltip's one argument is the default, "on" or "off", in the colours
// of the box's states as the work rows show theirs.
public sealed class ToggleSetting
{
    private static readonly Color OnColor = Color.green;
    private static readonly Color OffColor = ColorLibrary.RedReadable;

    private readonly string key;
    private bool? overrideValue;

    public ToggleSetting(string key)
    {
        this.key = key;
    }

    public bool DefaultValue { get; set; }

    public bool Value => overrideValue ?? DefaultValue;

    public void Reset() => overrideValue = null;

    public void ExposeData() => Scribe_Values.Look(ref overrideValue, key);

    // A checkbox row laid out as a Tunable row is, the box at the row's end; a click anywhere on
    // the row flips it. The tooltip is resolved by hand: a TaggedString handed over as a string
    // loses its tags, the default's colour among them.
    public void DoRow(Listing_Standard listing)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Widgets.DrawHighlightIfMouseover(row);
        string state = DefaultValue
            ? "MDWC_ToggleOn".Translate().Colorize(OnColor)
            : "MDWC_ToggleOff".Translate().Colorize(OffColor);
        TooltipHandler.TipRegion(row, ("MDWC_" + key + "Desc").Translate(state).Resolve());

        bool on = Value;
        Widgets.CheckboxLabeled(row, ("MDWC_" + key).Translate(), ref on);
        overrideValue = on == DefaultValue ? null : on;
        listing.Gap(Tunable.RowGap);
    }
}
