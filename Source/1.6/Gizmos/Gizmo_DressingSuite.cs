using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// The medicine suite's gauge, Toxin's Gizmo_ToxTank pattern over CompDressingSuite: the bar is
// the kits loaded, ticked at each kit boundary, and for the player's wearers the drag handle sets
// TargetKits, the level colonists reload to (Patches/CompApparelReloadable_Reload). Increments of
// MaxCharges snap the handle to whole kits.
//
// The fill is whole kits, the open one included (RemainingCharges, the ceiling of Available),
// because that is what the target compares against: the suite reloads only once a whole kit is
// used up, so a fractional fill would sit just under its own target marker and read as "below
// target, nobody reloading". The bar label and the tooltip's open-kit line carry the fraction.
//
// Gizmo_Slider reads Target once, on first draw, and writes it back every frame while draggable,
// so the setter writes only a changed kit count: an untouched gauge leaves the saved "full" (-1)
// in place, which follows a raised capacity. The comp rebuilds this instance when the capacity or
// the settings generation changes, so the first-draw read never goes stale.
//
// The colours follow the dressing's tint, read off the beam mote's def as the settings leave it
// when the gauge is built (the comp rebuilds it on every settings change): the fill is the tint,
// the hover highlight the tint part way to white, and the target marker the tint darkened, since
// vanilla's pale marker is lost against a light fill.
public class Gizmo_DressingSuite : Gizmo_Slider
{
    private const float HighlightBlend = 0.45f;
    private const float MarkerShade = 0.65f;

    private static bool draggingBar;

    private readonly CompDressingSuite suite;
    private readonly Color fill;
    private readonly Color highlight;
    private readonly Color marker;

    public Gizmo_DressingSuite(CompDressingSuite suite)
    {
        this.suite = suite;
        Color tint = MDWC_DefOf.MDWC_Mote_DressingBeam.graphicData.color;
        fill = new Color(tint.r, tint.g, tint.b);
        highlight = Color.Lerp(fill, Color.white, HighlightBlend);
        marker = new Color(fill.r * MarkerShade, fill.g * MarkerShade, fill.b * MarkerShade);
    }

    protected override float Target
    {
        get => (float)suite.TargetKits / suite.MaxCharges;
        set
        {
            int kits = Mathf.RoundToInt(value * suite.MaxCharges);
            if (kits != suite.TargetKits)
                suite.TargetKits = kits;
        }
    }

    protected override float ValuePercent => Mathf.Clamp01((float)suite.RemainingCharges / suite.MaxCharges);

    protected override string Title =>
        suite.AmmoDef != null ? suite.AmmoDef.LabelCap.ToString() : suite.parent.LabelCap;

    protected override bool IsDraggable => suite.Wearer?.Faction == Faction.OfPlayer;

    protected override string BarLabel => suite.LabelAvailable;

    protected override int Increments => suite.MaxCharges;

    protected override Color BarColor => fill;

    protected override Color BarHighlightColor => highlight;

    protected override Color BarDragColor => marker;

    protected override bool DraggingBar
    {
        get => draggingBar;
        set => draggingBar = value;
    }

    protected override IEnumerable<float> GetBarThresholds()
    {
        int max = suite.MaxCharges;
        for (int i = 1; i < max; i++)
            yield return (float)i / max;
    }

    // Laid out as vanilla's hemogen gauge (GeneGizmo_ResourceHemogen): the count under a tinted
    // header, the open kit's remainder while one is open, the reload target for the wearers whose
    // handle sets it (0 never reloads), then what a kit treats. Every count follows a label, so
    // none needs a plural. The header is the generic "Medicine", not the loaded medicine's
    // label, which the gizmo's title already shows and which a modded medicine can make long.
    protected override string GetTooltip()
    {
        var tip = new StringBuilder();
        tip.Append("MDWC_Medicine".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor));
        tip.Append(": ").Append(suite.LabelAvailable);
        if (suite.HasOpenKit)
        {
            float openLeft = 1f - suite.OpenUsed / suite.SeverityPerKit;
            tip.Append('\n').Append("MDWC_GaugeOpenKit".Translate(openLeft.ToStringPercent()).Resolve());
        }
        if (IsDraggable)
        {
            tip.Append('\n').Append(suite.TargetKits > 0
                ? "MDWC_GaugeReloadTarget".Translate(suite.TargetKits).Resolve()
                : "MDWC_GaugeNeverReload".Translate().Resolve());
        }
        tip.Append("\n\n").Append(
            "MDWC_GaugeKitDesc".Translate(suite.SeverityPerKit.ToString("0.##")).Resolve());
        return tip.ToString();
    }
}
