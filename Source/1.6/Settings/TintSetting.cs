using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MedicWarcasket;

// The dressing's colour: one of a few named tints picked from a row of radio buttons, what a
// Tunable is for a number. Until the player picks one it is Default, the colour each of the
// three mote defs carries in graphicData.color as the def XML (and any patch) leaves it, read
// per mote by the settings' LoadDefaultsFromDefs, which also hands the beam's here as
// DefaultValue for the swatch. The alternatives are constants, value-matched to the default
// (the motes draw additively, so a brighter tint would glow harder). Only an override is saved,
// by name, so a saved name this version does not know reads as the default.
public sealed class TintSetting
{
    public enum Tint
    {
        Default,
        Green,
        Pink,
    }

    private const string Key = "Tint";
    private const float LabelShare = 0.4f;
    private const float OptionPad = 10f;
    private const float SwatchSize = 16f;
    private const float RadioPad = 8f;
    private const float SwatchPad = 4f;

    private static readonly Tint[] Options = [Tint.Default, Tint.Green, Tint.Pink];

    // The old dressing green, and vanilla's ColorLibrary.Pink (ff81c0) at the medical blue's
    // value of 0.886.
    private static readonly Color Green = new(0.45f, 0.85f, 0.52f);
    private static readonly Color Pink = new(0.886f, 0.448f, 0.667f);

    private string tintName;

    // The beam mote's XML colour, shown for Default.
    public Color DefaultValue { get; set; } = Color.white;

    public Tint Value => tintName != null && Enum.TryParse(tintName, out Tint tint) ? tint : Tint.Default;

    // The colour to write over a mote's own, null while the default stands.
    public Color? Override => Value switch
    {
        Tint.Green => Green,
        Tint.Pink => Pink,
        _ => null,
    };

    public void Reset() => tintName = null;

    public void ExposeData() => Scribe_Values.Look(ref tintName, Key);

    // Laid out as a Tunable row: "MDWC_Tint" on the left, then the options side by side across
    // the slider and value columns, each a radio button, a swatch of its colour and its label.
    // The default is first and unmarked, so a short label fits a narrow window; "MDWC_TintDesc"
    // with its label is the whole row's tooltip.
    public void DoRow(Listing_Standard listing)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Rect labelRect = row.LeftPart(LabelShare);

        Widgets.DrawHighlightIfMouseover(row);
        TooltipHandler.TipRegion(row, "MDWC_TintDesc".Translate(Label(Tint.Default)));

        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, "MDWC_Tint".Translate());
        float x = labelRect.xMax + OptionPad;
        float width = (row.xMax - x) / Options.Length;
        foreach (Tint option in Options)
        {
            DoOption(new Rect(x, row.y, width, row.height), option);
            x += width;
        }
        Text.Anchor = anchor;
        listing.Gap(Tunable.RowGap);
    }

    // The radio button handles its own click; the swatch and label beside it select too.
    private void DoOption(Rect rect, Tint option)
    {
        bool chosen = Value == option;
        float radioY = rect.y + (rect.height - Widgets.RadioButtonSize) / 2f;
        bool clicked = Widgets.RadioButton(rect.x, radioY, chosen);

        var swatch = new Rect(rect.x + Widgets.RadioButtonSize + RadioPad, rect.y + (rect.height - SwatchSize) / 2f, SwatchSize, SwatchSize);
        Widgets.DrawBoxSolid(swatch, ColorOf(option));
        var labelRect = new Rect(swatch.xMax + SwatchPad, rect.y, rect.xMax - swatch.xMax - SwatchPad, rect.height);
        Widgets.Label(labelRect, Label(option).Truncate(labelRect.width));
        var rest = new Rect(swatch.x, rect.y, rect.xMax - swatch.x, rect.height);
        if (Widgets.ButtonInvisible(rest) && !chosen)
        {
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            clicked = true;
        }

        if (clicked)
            tintName = option == Tint.Default ? null : option.ToString();
    }

    private Color ColorOf(Tint option)
    {
        Color color = option switch
        {
            Tint.Green => Green,
            Tint.Pink => Pink,
            _ => DefaultValue,
        };
        return new Color(color.r, color.g, color.b);
    }

    private static string Label(Tint option) => ("MDWC_Tint" + option).Translate();
}
