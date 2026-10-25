using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// The medicine suite's gauge, Toxin's Gizmo_ToxTank pattern over CompDressingSuite: the bar is
// the kits loaded (the open kit by what is left of it), ticked at each kit boundary, and for the
// player's wearers the drag handle sets TargetKits, the level colonists reload to
// (Patches/CompApparelReloadable_Reload). Increments of MaxCharges snap the handle to whole kits.
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

    protected override float ValuePercent => Mathf.Clamp01(suite.Available / suite.MaxCharges);

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

    // The open kit's remainder as a percent of a kit; "0%" while every loaded kit is sealed.
    protected override string GetTooltip()
    {
        float openLeft = suite.HasOpenKit ? 1f - suite.OpenUsed / suite.SeverityPerKit : 0f;
        return "MDWC_GaugeTip".Translate(suite.SealedKits, openLeft.ToStringPercent(),
            suite.SeverityPerKit.ToString("0.##"), suite.TargetKits);
    }
}
