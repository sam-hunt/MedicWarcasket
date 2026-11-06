using System;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// One on/off def field, what a Tunable is for a number: the two CompProperties_MedicAI aiCasts
// flags, the armor's and the shoulders', the adrenaline shot's ShotExtension stopsHeartAttacks,
// the man in black incident's pawnKind (ours or not)
// and pawnFixedGender (fixed or not), and our kind's requiresResearch. Until the player flips it the value is DefaultValue,
// the def field as the XML (and any patch) leaves it, read by the settings'
// LoadDefaultsFromDefs; only an override is saved, under the key, and flipping the box back to
// the default clears it. The row's label and tooltip are "MDWC_" + key and that with "Desc",
// as a Tunable's are; the tooltip's one argument is the default's word, StateWord, which the
// work rows' tooltips share so every box in the window names its default alike. A row that
// names a def sets Term, resolved on every draw so a language reload's label reads through: the
// label's {0} and the tooltip's {1}.
public sealed class ToggleSetting
{
    // A box's state as its tooltip's default names it, "enabled" or "disabled", in the colours
    // of the box's states: pure green for the tick, vanilla's readable red for the cross.
    private static readonly Color EnabledColor = Color.green;
    private static readonly Color DisabledColor = ColorLibrary.RedReadable;

    internal static string StateWord(bool enabled) => enabled
        ? "MDWC_Enabled".Translate().Colorize(EnabledColor)
        : "MDWC_Disabled".Translate().Colorize(DisabledColor);

    private readonly string key;
    private bool? overrideValue;

    public ToggleSetting(string key)
    {
        this.key = key;
    }

    public bool DefaultValue { get; set; }

    public Func<string> Term { get; set; }

    public bool Value => overrideValue ?? DefaultValue;

    public void Reset() => overrideValue = null;

    public void ExposeData() => Scribe_Values.Look(ref overrideValue, key);

    // A checkbox row laid out as a Tunable row is, the box at the row's end; a click anywhere on
    // the row flips it. The label and tooltip are resolved by hand: a TaggedString handed over as a
    // string loses its tags (the implicit conversion strips them), the default's colour and a
    // Term's among them. A disabled row (a box another mod has made moot) is drawn in vanilla's
    // inactive grey, label and box alike, takes no click and keeps its value, so the choice holds
    // for when the row is active again; its tooltip still reads. A forced-off row (a child box
    // whose parent is off) is disabled and also drawn unticked, what the parent makes of it; its
    // saved value shows again once the parent is on.
    public void DoRow(Listing_Standard listing, bool disabled = false, bool forcedOff = false)
    {
        disabled |= forcedOff;
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        if (!disabled)
            Widgets.DrawHighlightIfMouseover(row);
        string term = Term?.Invoke();
        TaggedString tip = term == null
            ? ("MDWC_" + key + "Desc").Translate(StateWord(DefaultValue))
            : ("MDWC_" + key + "Desc").Translate(StateWord(DefaultValue), term);
        TooltipHandler.TipRegion(row, tip.Resolve());

        bool on = !forcedOff && Value;
        if (disabled)
            GUI.color = Widgets.InactiveColor;
        TaggedString label = term == null ? ("MDWC_" + key).Translate() : ("MDWC_" + key).Translate(term);
        Widgets.CheckboxLabeled(row, label.Resolve(), ref on, disabled);
        GUI.color = Color.white;
        if (!disabled)
            overrideValue = on == DefaultValue ? null : on;
        listing.Gap(Tunable.RowGap);
    }
}
