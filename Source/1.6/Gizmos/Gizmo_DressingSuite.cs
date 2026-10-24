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
// The colours are the dressing green the motes use: fill, a lighter highlight on hover, and a
// dark target marker, since vanilla's pale one is lost against a light fill.
public class Gizmo_DressingSuite : Gizmo_Slider
{
    private static readonly Color Fill = new(0.45f, 0.85f, 0.52f);
    private static readonly Color Highlight = new(0.70f, 0.95f, 0.74f);
    private static readonly Color Marker = new(0.28f, 0.58f, 0.34f);

    private static bool draggingBar;

    private readonly CompDressingSuite suite;

    public Gizmo_DressingSuite(CompDressingSuite suite)
    {
        this.suite = suite;
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

    protected override Color BarColor => Fill;

    protected override Color BarHighlightColor => Highlight;

    protected override Color BarDragColor => Marker;

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
