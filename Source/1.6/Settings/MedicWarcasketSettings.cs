using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

// The set's mod settings. Each Tunable overrides a def field, written onto the def by
// ApplyToDefs (at startup, then whenever the window closes), so every reader of those fields,
// vanilla's and VEF's included, sees the setting with no code of its own. The defaults, and the
// rationale for them, are the defs'. Nothing here is saved into a game: a save loads the same
// with any settings, and with none.
//
// The suite's doses override maxCharges on the armor's CompDressingSuite entry; the
// dressing's time, cooldown and range override the MDWC_MechaniteDressing AbilityDef's castTime and
// cooldownTime (shown in seconds, stored in ticks on the def) and distanceToTarget. The one
// section ends on a line of what its sliders amount to.
//
// What a write leaves on live ability instances (a running cooldown, a tooltip cache) is
// caught up by each instance on its next gizmo request, keyed on Generation
// (Ability_MechaniteDressing.ReconcileSettings); doses above a lowered maximum are trimmed off the
// worn suites at once (TrimWornSuites).
public class MedicWarcasketSettings : ModSettings
{
    private const string DressingDefName = "MDWC_MechaniteDressing";
    private const string ArmorDefName = "MDWC_Warcasket_Medic";

    // The whitespace above every section header but the first, and below the last row.
    private const float SectionGap = 28f;

    // Bumped by every ApplyToDefs, including the one at startup.
    public static int Generation { get; private set; }

    // The suite's maxCharges, per medicine type. The cast time is banded: quarter seconds at the
    // short end, where they matter, coarser steps as it reaches a slow dressing.
    // VEF paths to the target with PathEndMode.Touch at a distanceToTarget of 1.5 or less, so
    // the range's minimum is the adjacent dressing.
    public readonly Tunable doses = new("Doses", 1f, 20f, 1f, Whole);
    public readonly Tunable castTime = new("CastTime", 0f, 10f, 0.25f, v => v.ToString("0.##"))
    {
        StepBands = [(3f, 0.25f), (6f, 0.5f), (10f, 1f)],
    };
    public readonly Tunable cooldown = new("Cooldown", 0f, 300f, 5f, Whole);
    public readonly Tunable range = new("Range", 1.5f, 12.5f, 1f, v => v.ToString("0.#"));

    private bool defaultsLoaded;
    private Vector2 scrollPosition;
    private float contentHeight;

    // The icon beside the section header: the industrial medicine's, which is what the
    // dressing's gizmo shows by default (the ability def has no icon of its own).
    private Texture2D dressingIcon;

    private IEnumerable<Tunable> All => [doses, castTime, cooldown, range];

    private static string Whole(float v) => Mathf.RoundToInt(v).ToString();

    public override void ExposeData()
    {
        base.ExposeData();
        foreach (Tunable tunable in All)
            tunable.ExposeData();
    }

    // Reads the def-backed defaults before the first ApplyToDefs overwrites them; defs must be
    // loaded, so ModInit's static constructor calls this, never the Mod constructor.
    public void LoadDefaultsFromDefs()
    {
        if (!TryGetDefs(out Defs defs))
            return;
        dressingIcon = ThingDefOf.MedicineIndustrial.uiIcon;

        doses.DefaultValue = defs.suite.maxCharges;
        castTime.DefaultValue = (float)defs.dressing.castTime / GenTicks.TicksPerRealSecond;
        cooldown.DefaultValue = (float)defs.dressing.cooldownTime / GenTicks.TicksPerRealSecond;
        range.DefaultValue = defs.dressing.distanceToTarget;
        defaultsLoaded = true;
    }

    public void ApplyToDefs()
    {
        if (!defaultsLoaded || !TryGetDefs(out Defs defs))
            return;
        defs.suite.maxCharges = Mathf.Max(1, doses.IntValue);
        defs.dressing.castTime = Mathf.RoundToInt(castTime.Value * GenTicks.TicksPerRealSecond);
        defs.dressing.cooldownTime = Mathf.RoundToInt(cooldown.Value * GenTicks.TicksPerRealSecond);
        defs.dressing.distanceToTarget = range.Value;
        Generation++;
        TrimWornSuites();
    }

    // A lowered dose count leaves worn suites above it, which vanilla's reload accounting
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

    // One row per tunable under the section's header, then a reset button below. The section
    // is headed by whitespace and a medium-font header rather than a rule, which reads as
    // another slider, and the rows sit indented under it; it closes on the same gap so the list
    // does not end flush on its last row. The list scrolls once it outgrows the window's height
    // (a small window, a large font), sized from the previous frame's listing height; the
    // scrollbar only takes its width once it is needed. maxOneColumn keeps the listing growing
    // downward: past its rect's height it would start a new column off to the right, outside
    // the window, and never report the height the scroll view needs.
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
        SectionHeader(listing, "MDWC_SectionDressing", dressingIcon, first: true);
        doses.DoRow(listing);
        castTime.DoRow(listing);
        cooldown.DoRow(listing);
        range.DoRow(listing);
        SummaryRow(listing, DressingSummary);
        listing.Gap(SectionGap);
        contentHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - buttonHeight, buttonWidth, buttonHeight),
                "RestoreToDefaultSettings".Translate()))
        {
            foreach (Tunable tunable in All)
                tunable.Reset();
        }
    }

    // What the section's sliders amount to: the suite's capacity per type and the dressing's
    // time and cooldown, in the formats their rows show.
    private string DressingSummary() =>
        "MDWC_DressingSummary".Translate(doses.IntValue, castTime.Value.ToString("0.##"), Whole(cooldown.Value));

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
        public CompProperties_ApparelReloadable suite;
    }

    // All or nothing: with either target missing (another mod patched it away) the settings
    // leave every def at its XML values and say so once, as a warning, since the set itself
    // still works.
    private static bool warned;

    private static bool TryGetDefs(out Defs defs)
    {
        ThingDef armor = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName);
        defs = new Defs
        {
            dressing = DefDatabase<AbilityDef>.GetNamedSilentFail(DressingDefName),
            suite = armor?.comps?.OfType<CompProperties_ApparelReloadable>()
                .FirstOrDefault(p => p.compClass == typeof(CompDressingSuite)),
        };
        if (defs.dressing != null && defs.suite != null)
            return true;
        if (!warned)
            Log.Warning("[Medic Warcasket] Settings could not find the field dressing ability or the armor's medicine suite; the set keeps its XML values.");
        warned = true;
        return false;
    }
}
