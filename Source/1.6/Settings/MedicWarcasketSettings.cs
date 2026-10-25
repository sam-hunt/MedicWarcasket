using System;
using System.Collections.Generic;
using System.Linq;
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
// castTime (shown in seconds, stored in ticks on the def), cooldownTime (shown in game hours,
// likewise in ticks) and distanceToTarget, and the time per wound overrides its
// DressingExtension's ticksPerTend (in seconds, like the warmup). The medicine
// also writes its uiIcon onto the ability's icon while the def names no iconPath of its own, so
// VEF's gizmo and targeting cursor show it unaided. The tint overrides the rgb of the three
// dressing motes' graphicData.color, each keeping its own alpha, and the gauge reads its colours
// off the beam's (Gizmo_DressingSuite). A line of what its rows amount to opens the section,
// under its header, and the two picked rows sit above the sliders. Its header carries the
// ability's tooltip.
//
// The shot section does the same for the adrenaline shot: on the shoulders'
// CompProperties_ApparelReloadable entry the shots override maxCharges; on the
// MDWC_AdrenalineShot AbilityDef the cooldown overrides cooldownTime, in game hours as the
// dressing's; on the MDWC_Adrenaline HediffDef the duration overrides its severity-per-day
// comp's severityPerDay (shown in hours, stored as -24 / hours) and the rest fall writes the
// top stage's restFallFactor with each lower stage one higher, the XML ladder's step. The injector's ammoDef's uiIcon (go-juice's)
// goes onto the shot's icon as the medicine's goes onto the dressing's.
//
// The work section is one checkbox per Caring work type the trait mirror leaves to the player
// (WorkSetting over WorkProfile.Toggleable), written onto the medic trait's disabledWorkTypes
// through WorkProfile.Apply, which also refreshes every entombed pilot; the mirror's own rule
// is each box's default. Its header's icon is vanilla's mining work site, a Core texture, so
// it survives a play-data reload as a plain static, and its tooltip says what the section
// covers and what it leaves to VFEP's trait. The section is skipped while the mirror found
// nothing to toggle (a broken install).
//
// What a write leaves on live ability instances (a running cooldown, a tooltip cache) is
// caught up by each instance on its next gizmo request, keyed on Generation
// (Ability_MechaniteDressing.ReconcileSettings); kits above a lowered maximum are trimmed off
// the worn suites at once (TrimWornSuites). A medicine change needs nothing of either: kits are
// a vanilla charge count with no type of their own, so those loaded count as the new medicine
// and the next reload fetches it. A tint change reaches every mote spawned after it; one in
// flight keeps the graphic it was spawned with. Shots above a lowered maximum are spent off
// the worn shoulders at once (TrimWornInjectors); a shot already given keeps the duration it
// was applied with, since vanilla's comp caches the rate per instance, while the rest fall is
// read off the stages live and reaches it at once.
public class MedicWarcasketSettings : ModSettings
{
    private const string DressingDefName = "MDWC_MechaniteDressing";
    private const string ArmorDefName = "MDWC_Warcasket_Medic";
    private const string ShotDefName = "MDWC_AdrenalineShot";
    private const string ShouldersDefName = "MDWC_WarcasketShoulders_Medic";
    private const string AdrenalineDefName = "MDWC_Adrenaline";

    // The beam first: its colour stands for the set on the tint row and the gauge.
    private static readonly string[] MoteDefNames =
        ["MDWC_Mote_DressingBeam", "MDWC_Mote_DressingGlow", "MDWC_Mote_DressingOverlay"];

    // The whitespace above every section header but the first, and below the last row.
    private const float SectionGap = 28f;

    // Bumped by every ApplyToDefs, including the one at startup.
    public static int Generation { get; private set; }

    public readonly MedicineSetting medicine = new();
    public readonly TintSetting tint = new();
    public readonly WorkSetting work = new();

    // Both cooldowns are game hours up to three days, banded: quarter hours (about ten seconds
    // at normal speed) where a cooldown between fights sits, whole hours up to a day, then
    // eighths of a day.
    private static readonly (float upTo, float step)[] CooldownBands = [(6f, 0.25f), (24f, 1f), (72f, 3f)];

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
    public readonly Tunable cooldown = new("Cooldown", 0f, 72f, 0.25f, Hours) { StepBands = CooldownBands };
    public readonly Tunable range = new("Range", 1.9f, 12.9f, 1f, v => v.ToString("0.#"));

    // The shot's: the injector's maxCharges, the hediff's duration in hours, the top stage's
    // rest fall factor (the lower stages step up from it), and the cooldown in game hours.
    public readonly Tunable shots = new("Shots", 1f, 10f, 1f, Whole);
    public readonly Tunable shotDuration = new("ShotDuration", 0.5f, 12f, 0.5f, v => v.ToString("0.#"));
    public readonly Tunable restFall = new("RestFall", 1f, 5f, 0.5f, v => v.ToString("0.#"));
    public readonly Tunable shotCooldown = new("ShotCooldown", 0f, 72f, 0.25f, Hours) { StepBands = CooldownBands };

    private bool defaultsLoaded;
    private Vector2 scrollPosition;
    private float contentHeight;

    // The severity one kit treats, quoted by the summary line; a def field with no setting.
    private int severityPerKit;

    // Each mote's graphicData.color as the XML leaves it, by MoteDefNames order.
    private Color[] moteColors;

    // The tooltip over each ability section's header: the ability, its name as VEF titles an
    // ability's tooltip over the description its command shows in game.
    private string dressingTip;
    private string shotTip;

    // The shot section's header icon, the injector's ammoDef's (a Core texture, safe to hold).
    private Texture2D shotIcon;

    // The hediff's stage count, quoted by the shot's summary line.
    private int shotLevels;

    // The work section's header icon, vanilla's mining work site. Its art is in the base game's
    // Resources, not Ideology's, so it loads whatever DLCs are active and survives a reload.
    private Texture2D workIcon;

    private IEnumerable<Tunable> All => [kits, castTime, tendInterval, cooldown, range, shots, shotDuration, restFall, shotCooldown];

    private static string Whole(float v) => Mathf.RoundToInt(v).ToString();

    private static string Hours(float v) => v.ToString("0.##");

    public override void ExposeData()
    {
        base.ExposeData();
        medicine.ExposeData();
        tint.ExposeData();
        work.ExposeData();
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
        cooldown.DefaultValue = (float)defs.dressing.cooldownTime / GenDate.TicksPerHour;
        range.DefaultValue = defs.dressing.distanceToTarget;
        severityPerKit = Mathf.RoundToInt(defs.suite.severityPerKit);
        moteColors = new Color[defs.motes.Length];
        for (int i = 0; i < defs.motes.Length; i++)
            moteColors[i] = defs.motes[i].graphicData.color;
        tint.DefaultValue = moteColors[0];
        dressingTip = HeaderTip(defs.dressing.LabelCap, defs.dressing.description);
        shots.DefaultValue = defs.injector.maxCharges;
        shotDuration.DefaultValue = defs.fade.severityPerDay < 0f ? -24f / defs.fade.severityPerDay : 0f;
        restFall.DefaultValue = defs.adrenaline.stages[defs.adrenaline.stages.Count - 1].restFallFactor;
        shotCooldown.DefaultValue = (float)defs.shot.cooldownTime / GenDate.TicksPerHour;
        shotLevels = defs.adrenaline.stages.Count;
        shotIcon = defs.injector.ammoDef?.uiIcon;
        shotTip = HeaderTip(defs.shot.LabelCap, defs.shot.description);
        workIcon = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Sites/MiningWorkSite");
        defaultsLoaded = true;
    }

    // The work profile goes first: it needs none of the dressing's defs, so a saved choice
    // still holds while they are missing, though the page is unavailable to change it.
    public void ApplyToDefs()
    {
        WorkProfile.Apply(work.IsOpen);
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
        defs.dressing.cooldownTime = Mathf.RoundToInt(cooldown.Value * GenDate.TicksPerHour);
        defs.dressing.distanceToTarget = range.Value;
        defs.extension.ticksPerTend = Mathf.Max(1, Mathf.RoundToInt(tendInterval.Value * GenTicks.TicksPerRealSecond));
        for (int i = 0; i < defs.motes.Length; i++)
        {
            Color rgb = tint.Override ?? moteColors[i];
            defs.motes[i].graphicData.color = new Color(rgb.r, rgb.g, rgb.b, moteColors[i].a);
            RebuildGraphic(defs.motes[i]);
        }
        defs.injector.maxCharges = Mathf.Max(1, shots.IntValue);
        if (defs.shot.iconPath.NullOrEmpty())
            defs.shot.icon = shotIcon != null ? shotIcon : BaseContent.BadTex;
        defs.shot.cooldownTime = Mathf.RoundToInt(shotCooldown.Value * GenDate.TicksPerHour);
        defs.fade.severityPerDay = -24f / Mathf.Max(shotDuration.Value, 0.01f);
        List<HediffStage> stages = defs.adrenaline.stages;
        for (int i = 0; i < stages.Count; i++)
            stages[i].restFallFactor = restFall.Value + (stages.Count - 1 - i);
        Generation++;
        TrimWornSuites();
        TrimWornInjectors();
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

    // Shots above a lowered maximum are spent off the worn shoulders, through the comp's own
    // UsedOnce (remainingCharges is protected, and a subclass for a trim is not worth it): a
    // stored piece keeps its count, which vanilla's reload accounting reads as full.
    private static void TrimWornInjectors()
    {
        if (Current.ProgramState != ProgramState.Playing)
            return;
        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
        {
            if (pawn.apparel == null)
                continue;
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.def.defName != ShouldersDefName || apparel.TryGetComp<CompApparelReloadable>() is not { } injector)
                    continue;
                while (injector.RemainingCharges > injector.MaxCharges)
                    injector.UsedOnce();
            }
        }
    }

    // Each section opens on its summary under its header, then one row per setting, and a reset
    // button sits below the list. The section is headed by whitespace and a medium-font header
    // rather than a rule, which reads as another slider, and the rows sit indented under it; it
    // closes on the same gap so the list does not end flush on its last row. The header's icon
    // is the configured medicine's, read on each draw so it follows the medicine row at once.
    // The list scrolls once it outgrows the window's height (a small window, a large font),
    // sized from the previous frame's listing height; the scrollbar only takes its width once
    // it is needed. maxOneColumn keeps the listing growing downward: past its rect's height it
    // would start a new column off to the right, outside the window, and never report the
    // height the scroll view needs.
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
        SectionHeader(listing, "MDWC_SectionDressing", medicine.Value?.uiIcon, dressingTip, first: true);
        SummaryRow(listing, DressingSummary);
        medicine.DoRow(listing);
        tint.DoRow(listing);
        kits.DoRow(listing);
        castTime.DoRow(listing);
        tendInterval.DoRow(listing);
        cooldown.DoRow(listing);
        range.DoRow(listing);
        SectionHeader(listing, "MDWC_SectionShot", shotIcon, shotTip);
        SummaryRow(listing, ShotSummary);
        shots.DoRow(listing);
        shotDuration.DoRow(listing);
        restFall.DoRow(listing);
        shotCooldown.DoRow(listing);
        if (WorkProfile.Toggleable.Count > 0)
        {
            SectionHeader(listing, "MDWC_SectionWork", workIcon, HeaderTip("MDWC_SectionWork".Translate(), "MDWC_SectionWorkDesc".Translate()));
            SummaryRow(listing, WorkSummary);
            work.DoRows(listing);
        }
        listing.Gap(SectionGap);
        contentHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - buttonHeight, buttonWidth, buttonHeight),
                "RestoreToDefaultSettings".Translate()))
        {
            medicine.Reset();
            tint.Reset();
            work.Reset();
            foreach (Tunable tunable in All)
                tunable.Reset();
        }
    }

    // What the section's rows amount to: the suite's capacity and medicine, what a kit treats,
    // and the dressing's timings, in the formats their rows show; a cooldown of 0 ends the line
    // on its own clause rather than "0 h of cooldown".
    private string DressingSummary()
    {
        string medicineLabel = medicine.Value?.label ?? string.Empty;
        string warmup = castTime.Value.ToString("0.##");
        string perWound = tendInterval.Value.ToString("0.##");
        if (cooldown.Value <= 0f)
            return "MDWC_DressingSummaryNoCooldown".Translate(kits.IntValue, medicineLabel, severityPerKit, warmup, perWound);
        return "MDWC_DressingSummary".Translate(kits.IntValue, medicineLabel, severityPerKit, warmup, perWound, Hours(cooldown.Value));
    }

    // What the shot section's rows amount to: the shots a vial gives, how long one lasts and
    // in how many levels, the rest fall at its start, and the cooldown, with the same
    // no-cooldown clause rule as the dressing's.
    private string ShotSummary()
    {
        string hours = shotDuration.Value.ToString("0.#");
        string fall = restFall.Value.ToString("0.#");
        if (shotCooldown.Value <= 0f)
            return "MDWC_ShotSummaryNoCooldown".Translate(shots.IntValue, hours, shotLevels, fall);
        return "MDWC_ShotSummary".Translate(shots.IntValue, hours, shotLevels, fall, Hours(shotCooldown.Value));
    }

    // What the work section's boxes amount to: the caring work the pilot may take on, named as
    // the work tab names it, or that there is none; the rest is VFEP's rule either way.
    private string WorkSummary()
    {
        List<string> open = work.Open.Select(WorkSetting.Name).ToList();
        if (open.Count == 0)
            return "MDWC_WorkSummaryNone".Translate();
        return "MDWC_WorkSummary".Translate(open.ToCommaList(useAnd: true));
    }

    // A muted line in a section, indented and padded as its rows are, as tall as its text, with
    // a row's gap below it.
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
        listing.Gap(Tunable.RowGap);
    }

    // The header's text in the medium font after its icon, a square as tall as the header; a
    // header whose texture is missing starts its text at the left edge rather than after an
    // empty slot. The tooltip covers the icon and the text only, not the empty width after them.
    private static void SectionHeader(Listing_Standard listing, string key, Texture2D icon, string tooltip, bool first = false)
    {
        const float headerGap = 4f;
        const float headerHeight = 32f;
        const float iconPad = 8f;
        if (!first)
            listing.Gap(SectionGap);
        Rect rect = listing.GetRect(headerHeight);
        float left = rect.xMin;
        if (icon != null && icon != BaseContent.BadTex)
        {
            GUI.DrawTexture(rect.LeftPartPixels(headerHeight), icon);
            rect.xMin += headerHeight + iconPad;
        }
        GameFont font = Text.Font;
        TextAnchor anchor = Text.Anchor;
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleLeft;
        string label = key.Translate();
        Widgets.Label(rect, label);
        if (!tooltip.NullOrEmpty())
            TooltipHandler.TipRegion(new Rect(left, rect.y, rect.xMin - left + Text.CalcSize(label).x, rect.height), tooltip);
        Text.Anchor = anchor;
        Text.Font = font;
        listing.Gap(headerGap);
    }

    // An ability's tooltip as VEF opens one: its name in vanilla's tip title colour over its
    // description. VEF's stat lines are left out; the rows above the summary quote those numbers.
    private static string HeaderTip(string label, string description) =>
        description.NullOrEmpty() ? null : label.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + description;

    private struct Defs
    {
        public AbilityDef dressing;
        public CompProperties_DressingSuite suite;
        public DressingExtension extension;
        public ThingDef[] motes;
        public AbilityDef shot;
        public CompProperties_ApparelReloadable injector;
        public HediffDef adrenaline;
        public HediffCompProperties_SeverityPerDay fade;
    }

    // All or nothing: with any target missing (another mod patched it away) the settings leave
    // every def at its XML values and say so once, as a warning, since the set itself still
    // works.
    private static bool warned;

    private static bool TryGetDefs(out Defs defs)
    {
        AbilityDef dressing = DefDatabase<AbilityDef>.GetNamedSilentFail(DressingDefName);
        HediffDef adrenaline = DefDatabase<HediffDef>.GetNamedSilentFail(AdrenalineDefName);
        defs = new Defs
        {
            dressing = dressing,
            suite = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName)?.GetCompProperties<CompProperties_DressingSuite>(),
            extension = dressing?.GetModExtension<DressingExtension>(),
            motes = new ThingDef[MoteDefNames.Length],
            shot = DefDatabase<AbilityDef>.GetNamedSilentFail(ShotDefName),
            injector = DefDatabase<ThingDef>.GetNamedSilentFail(ShouldersDefName)?.GetCompProperties<CompProperties_ApparelReloadable>(),
            adrenaline = adrenaline,
            fade = adrenaline?.CompProps<HediffCompProperties_SeverityPerDay>(),
        };
        bool motesFound = true;
        for (int i = 0; i < MoteDefNames.Length; i++)
        {
            defs.motes[i] = DefDatabase<ThingDef>.GetNamedSilentFail(MoteDefNames[i]);
            motesFound &= defs.motes[i]?.graphicData != null;
        }
        bool shotFound = defs.shot != null && defs.injector != null && defs.fade != null && !defs.adrenaline.stages.NullOrEmpty();
        if (defs.dressing != null && defs.suite != null && defs.extension != null && motesFound && shotFound)
            return true;
        if (!warned)
            Log.Warning("[Medic Warcasket] Settings could not find the mechanite dressing ability, its dressing extension, its motes, the armor's dressing suite, the adrenaline shot ability, the shoulders' injector or the adrenaline hediff's fade and stages; the set keeps its XML values.");
        warned = true;
        return false;
    }
}
