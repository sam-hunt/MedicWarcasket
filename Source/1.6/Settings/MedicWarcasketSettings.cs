using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

// The set's mod settings. Each setting overrides a def field, written onto the def by
// ApplyToDefs (at startup, then whenever the window closes), so every reader of those fields,
// vanilla's and VEF's included, sees the setting with no code of its own. The defaults, and the
// rationale for them, are the defs'. Nothing here is saved into a game: a save loads the same
// with any settings, and with none.
//
// On the armor's CompProperties_DressingSuite entry, the medicine overrides ammoDef and the kits
// maxCharges. On the MDWC_MechaniteDressing AbilityDef, the warmup, cooldown and range override
// castTime, cooldownTime (shown in seconds, stored in ticks on the def) and distanceToTarget,
// and the time per wound overrides its DressingExtension's ticksPerTend (likewise). The medicine
// also writes its uiIcon onto the ability's icon while the def names no iconPath of its own, so
// VEF's gizmo and targeting cursor show it unaided. The tint overrides the rgb of the three
// dressing motes' graphicData.color, each keeping its own alpha, and the gauge reads its colours
// off the beam's (Gizmo_DressingSuite). The one section ends on a line of what its rows amount
// to.
//
// What a write leaves on live ability instances (a running cooldown, a tooltip cache) is
// caught up by each instance on its next gizmo request, keyed on Generation
// (Ability_MechaniteDressing.ReconcileSettings); kits above a lowered maximum are trimmed off
// the worn suites at once (TrimWornSuites). A medicine change needs nothing of either: kits are
// a vanilla charge count with no type of their own, so those loaded count as the new medicine
// and the next reload fetches it. A tint change reaches every mote spawned after it; one in
// flight keeps the graphic it was spawned with.
public class MedicWarcasketSettings : ModSettings
{
    private const string DressingDefName = "MDWC_MechaniteDressing";
    private const string ArmorDefName = "MDWC_Warcasket_Medic";

    // The beam first: its colour stands for the set on the tint row and the gauge.
    private static readonly string[] MoteDefNames =
        ["MDWC_Mote_DressingBeam", "MDWC_Mote_DressingGlow", "MDWC_Mote_DressingOverlay"];

    // The whitespace above every section header but the first, and below the last row.
    private const float SectionGap = 28f;

    // Bumped by every ApplyToDefs, including the one at startup.
    public static int Generation { get; private set; }

    public readonly MedicineSetting medicine = new();
    public readonly TintSetting tint = new();

    // The suite's maxCharges. The warmup is banded: quarter seconds at the short end, where they
    // matter, coarser steps as it reaches a slow dressing.
    // The range sits on RimWorld's x.9 radii, which put the 2.9 default on the grid (the Tunable
    // clears a value within half a step of the default, so an off-grid default could never be
    // left by one step). Its 1.9 minimum reaches the same cells as 1.5, the adjacent ones
    // diagonals included; the dressing's own walk always paths to touch, so no VEF pathing
    // threshold constrains it.
    public readonly Tunable kits = new("Kits", 1f, 20f, 1f, Whole);
    public readonly Tunable castTime = new("CastTime", 0f, 10f, 0.25f, v => v.ToString("0.##"))
    {
        StepBands = [(3f, 0.25f), (6f, 0.5f), (10f, 1f)],
    };
    public readonly Tunable tendInterval = new("TendInterval", 0.25f, 5f, 0.25f, v => v.ToString("0.##"));
    public readonly Tunable cooldown = new("Cooldown", 0f, 300f, 5f, Whole);
    public readonly Tunable range = new("Range", 1.9f, 12.9f, 1f, v => v.ToString("0.#"));

    private bool defaultsLoaded;
    private Vector2 scrollPosition;
    private float contentHeight;

    // The severity one kit treats, quoted by the summary line; a def field with no setting.
    private int severityPerKit;

    // Each mote's graphicData.color as the XML leaves it, by MoteDefNames order.
    private Color[] moteColors;

    private IEnumerable<Tunable> All => [kits, castTime, tendInterval, cooldown, range];

    private static string Whole(float v) => Mathf.RoundToInt(v).ToString();

    public override void ExposeData()
    {
        base.ExposeData();
        medicine.ExposeData();
        tint.ExposeData();
        foreach (Tunable tunable in All)
            tunable.ExposeData();
    }

    // Reads the def-backed defaults before the first ApplyToDefs overwrites them; defs must be
    // loaded, so ModInit's static constructor calls this, never the Mod constructor.
    public void LoadDefaultsFromDefs()
    {
        if (!TryGetDefs(out Defs defs))
            return;
        medicine.DefaultValue = defs.suite.ammoDef;
        kits.DefaultValue = defs.suite.maxCharges;
        castTime.DefaultValue = (float)defs.dressing.castTime / GenTicks.TicksPerRealSecond;
        tendInterval.DefaultValue = (float)defs.extension.ticksPerTend / GenTicks.TicksPerRealSecond;
        cooldown.DefaultValue = (float)defs.dressing.cooldownTime / GenTicks.TicksPerRealSecond;
        range.DefaultValue = defs.dressing.distanceToTarget;
        severityPerKit = Mathf.RoundToInt(defs.suite.severityPerKit);
        moteColors = new Color[defs.motes.Length];
        for (int i = 0; i < defs.motes.Length; i++)
            moteColors[i] = defs.motes[i].graphicData.color;
        tint.DefaultValue = moteColors[0];
        defaultsLoaded = true;
    }

    public void ApplyToDefs()
    {
        if (!defaultsLoaded || !TryGetDefs(out Defs defs))
            return;
        ThingDef chosen = medicine.Value;
        if (chosen != null)
        {
            defs.suite.ammoDef = chosen;
            if (defs.dressing.iconPath.NullOrEmpty())
                defs.dressing.icon = chosen.uiIcon != null ? chosen.uiIcon : BaseContent.BadTex;
        }
        defs.suite.maxCharges = Mathf.Max(1, kits.IntValue);
        defs.dressing.castTime = Mathf.RoundToInt(castTime.Value * GenTicks.TicksPerRealSecond);
        defs.dressing.cooldownTime = Mathf.RoundToInt(cooldown.Value * GenTicks.TicksPerRealSecond);
        defs.dressing.distanceToTarget = range.Value;
        defs.extension.ticksPerTend = Mathf.Max(1, Mathf.RoundToInt(tendInterval.Value * GenTicks.TicksPerRealSecond));
        for (int i = 0; i < defs.motes.Length; i++)
        {
            Color rgb = tint.Override ?? moteColors[i];
            defs.motes[i].graphicData.color = new Color(rgb.r, rgb.g, rgb.b, moteColors[i].a);
            RebuildGraphic(defs.motes[i]);
        }
        Generation++;
        TrimWornSuites();
    }

    // A def's graphic is built once from its graphicData and cached with the colour it was built
    // in, and a mote reaches it through GraphicColoredFor, which answers a colour the cache does
    // not match with Graphic.GetColoredVersion: for a Graphic_Single that is a plain
    // Graphic_Single without the def's graphicClass or shaderParameters, which would strip the
    // beam's age-driven noise and the overlay's silhouette. So after a colour write the cache is
    // cleared, through CopyFrom onto itself (every field assigned to itself, cachedGraphic nulled;
    // the one public way), and the def's handle rebuilt from it, so the next mote finds a cache
    // in its own colour and takes it as is.
    private static void RebuildGraphic(ThingDef mote)
    {
        mote.graphicData.CopyFrom(mote.graphicData);
        mote.graphic = mote.graphicData.Graphic;
    }

    // A lowered kit count leaves worn suites above it, which vanilla's reload accounting
    // cannot take (CompDressingSuite); a stored armor is trimmed when equipped, and a suite
    // saved above the count on load. Every live pawn, since a caravan or transporter pawn
    // spawns with no hook of its own.
    private static void TrimWornSuites()
    {
        if (Current.ProgramState != ProgramState.Playing)
            return;
        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
        {
            if (pawn.apparel == null)
                continue;
            foreach (Apparel apparel in pawn.apparel.WornApparel)
                apparel.TryGetComp<CompDressingSuite>()?.Trim();
        }
    }

    // One row per setting under the section's header, then a reset button below. The section
    // is headed by whitespace and a medium-font header rather than a rule, which reads as
    // another slider, and the rows sit indented under it; it closes on the same gap so the list
    // does not end flush on its last row. The header's icon is the configured medicine's, read
    // on each draw so it follows the medicine row at once. The list scrolls once it outgrows the
    // window's height (a small window, a large font), sized from the previous frame's listing
    // height; the scrollbar only takes its width once it is needed. maxOneColumn keeps the
    // listing growing downward: past its rect's height it would start a new column off to the
    // right, outside the window, and never report the height the scroll view needs.
    // While the defs were not found there is nothing to tune, so the page says so instead of
    // drawing rows around the -1 the defaults never got.
    public void DoWindowContents(Rect inRect)
    {
        if (!defaultsLoaded)
        {
            Widgets.Label(inRect, "MDWC_SettingsUnavailable".Translate());
            return;
        }

        const float buttonHeight = 30f;
        const float buttonWidth = 200f;
        const float scrollbarWidth = 16f;
        var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - buttonHeight - Listing.DefaultGap);
        float viewWidth = contentHeight > outRect.height ? outRect.width - scrollbarWidth : outRect.width;
        var viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(contentHeight, outRect.height));
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var listing = new Listing_Standard { maxOneColumn = true };
        listing.Begin(viewRect);
        SectionHeader(listing, "MDWC_SectionDressing", medicine.Value?.uiIcon, first: true);
        medicine.DoRow(listing);
        kits.DoRow(listing);
        castTime.DoRow(listing);
        tendInterval.DoRow(listing);
        cooldown.DoRow(listing);
        range.DoRow(listing);
        tint.DoRow(listing);
        SummaryRow(listing, DressingSummary);
        listing.Gap(SectionGap);
        contentHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - buttonHeight, buttonWidth, buttonHeight),
                "RestoreToDefaultSettings".Translate()))
        {
            medicine.Reset();
            tint.Reset();
            foreach (Tunable tunable in All)
                tunable.Reset();
        }
    }

    // What the section's rows amount to: the suite's capacity and medicine, what a kit treats,
    // and the dressing's timings, in the formats their rows show; a cooldown of 0 ends the line
    // on its own clause rather than "0 s of cooldown".
    private string DressingSummary()
    {
        string medicineLabel = medicine.Value?.label ?? string.Empty;
        string warmup = castTime.Value.ToString("0.##");
        string perWound = tendInterval.Value.ToString("0.##");
        if (Mathf.RoundToInt(cooldown.Value) == 0)
            return "MDWC_DressingSummaryNoCooldown".Translate(kits.IntValue, medicineLabel, severityPerKit, warmup, perWound);
        return "MDWC_DressingSummary".Translate(kits.IntValue, medicineLabel, severityPerKit, warmup, perWound, Whole(cooldown.Value));
    }

    // A muted line under a section's rows, indented and padded as they are, as tall as its text.
    private static void SummaryRow(Listing_Standard listing, Func<string> summary)
    {
        string text = summary();
        float width = listing.ColumnWidth - Tunable.RowIndent - Tunable.RowEndPad;
        Rect rect = listing.GetRect(Text.CalcHeight(text, width));
        rect.xMin += Tunable.RowIndent;
        rect.xMax -= Tunable.RowEndPad;
        GUI.color = ColoredText.SubtleGrayColor;
        Widgets.Label(rect, text);
        GUI.color = Color.white;
    }

    // The header's text in the medium font after its icon, a square as tall as the header; a
    // header whose texture is missing starts its text at the left edge rather than after an
    // empty slot.
    private static void SectionHeader(Listing_Standard listing, string key, Texture2D icon, bool first = false)
    {
        const float headerGap = 4f;
        const float headerHeight = 32f;
        const float iconPad = 8f;
        if (!first)
            listing.Gap(SectionGap);
        Rect rect = listing.GetRect(headerHeight);
        if (icon != null && icon != BaseContent.BadTex)
        {
            GUI.DrawTexture(rect.LeftPartPixels(headerHeight), icon);
            rect.xMin += headerHeight + iconPad;
        }
        GameFont font = Text.Font;
        TextAnchor anchor = Text.Anchor;
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(rect, key.Translate());
        Text.Anchor = anchor;
        Text.Font = font;
        listing.Gap(headerGap);
    }

    private struct Defs
    {
        public AbilityDef dressing;
        public CompProperties_DressingSuite suite;
        public DressingExtension extension;
        public ThingDef[] motes;
    }

    // All or nothing: with any target missing (another mod patched it away) the settings leave
    // every def at its XML values and say so once, as a warning, since the set itself still
    // works.
    private static bool warned;

    private static bool TryGetDefs(out Defs defs)
    {
        AbilityDef dressing = DefDatabase<AbilityDef>.GetNamedSilentFail(DressingDefName);
        defs = new Defs
        {
            dressing = dressing,
            suite = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName)?.GetCompProperties<CompProperties_DressingSuite>(),
            extension = dressing?.GetModExtension<DressingExtension>(),
            motes = new ThingDef[MoteDefNames.Length],
        };
        bool motesFound = true;
        for (int i = 0; i < MoteDefNames.Length; i++)
        {
            defs.motes[i] = DefDatabase<ThingDef>.GetNamedSilentFail(MoteDefNames[i]);
            motesFound &= defs.motes[i]?.graphicData != null;
        }
        if (defs.dressing != null && defs.suite != null && defs.extension != null && motesFound)
            return true;
        if (!warned)
            Log.Warning("[Medic Warcasket] Settings could not find the mechanite dressing ability, its dressing extension, its motes or the armor's dressing suite; the set keeps its XML values.");
        warned = true;
        return false;
    }
}
