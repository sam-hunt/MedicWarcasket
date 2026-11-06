using System;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

// The set's mod settings: the window frame, the ExposeData / LoadDefaultsFromDefs / ApplyToDefs /
// ResetToDefaults fan-out, the time formats and the shared row helpers. The class is split
// across Settings_<Section>.cs partials, one per window section (the dressing, the shot, the
// work, the man in black), each owning its fields, scribe entries, def-backed defaults, apply,
// reset and draw, so a setting is a one-file edit; each partial's header lists the def fields
// it writes. Each setting overrides a def field, written onto the def by ApplyToDefs (at
// startup, then whenever the window closes), so every reader of those fields, vanilla's and
// VEF's included, sees the setting with no code of its own. The defaults, and the rationale
// for them, are the defs'.
// Nothing here is saved into a game: a save loads the same with any settings, and with none.
//
// Each ability section's header carries the ability's own icon, read off the def at draw
// time rather than cached: VEF resolves def.icon from iconPath in a deferred step at the end of
// loading, and a play-data reload replaces the def and its texture together, so the def
// handles LoadDefaultsFromDefs keeps (re-run on that reload) are the only state held.
//
// What a write leaves on live ability instances (a running cooldown, a tooltip cache) is
// caught up by each instance on its next gizmo request, keyed on Generation
// (Ability_MechaniteDressing.ReconcileSettings); what it leaves on worn pieces is settled by
// the write itself, through each section's trim.
public partial class MedicWarcasketSettings : ModSettings
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

    // Both cooldowns are game hours up to ten days, banded so one slider spans a gap between
    // casts in a fight and one between raids: real seconds at normal speed up to ten (quarter,
    // then half, then whole seconds), then half hours to 3 h, hours to 12 h, quarter days to a
    // day, half days to three, and whole days. The slider gives every stop the same width.
    private const float CooldownMax = 10f * GenDate.HoursPerDay;
    private static readonly (float upTo, float step)[] CooldownBands =
    [
        (Seconds(3f), Seconds(0.25f)), (Seconds(5f), Seconds(0.5f)), (Seconds(10f), Seconds(1f)),
        (3f, 0.5f), (12f, 1f), (24f, 6f), (72f, 12f), (CooldownMax, GenDate.HoursPerDay),
    ];

    private bool defaultsLoaded;
    private Vector2 scrollPosition;
    private float contentHeight;

    private static string Whole(float v) => Mathf.RoundToInt(v).ToString();

    // Game hours from real seconds at normal speed.
    private static float Seconds(float s) => s * GenTicks.TicksPerRealSecond / GenDate.TicksPerHour;

    // Every time a row or summary shows carries its unit as vanilla's short period form does
    // (GenDate.ToStringTicksToPeriod with shortForm): the number, then the unit letter, no space.
    private static string RealSeconds(float s) => s.ToString("0.##") + "LetterSecond".Translate();

    // A period in game hours, in the unit of the cooldown band it falls in: seconds below the
    // half hour, hours below a day, days from there.
    private static string Period(float hours)
    {
        if (hours < 0.5f)
            return RealSeconds(hours * GenDate.TicksPerHour / GenTicks.TicksPerRealSecond);
        if (hours < GenDate.HoursPerDay)
            return hours.ToString("0.##") + "LetterHour".Translate();
        return (hours / GenDate.HoursPerDay).ToString("0.##") + "LetterDay".Translate();
    }

    // A length in game hours, as Period but in hours below a day: a quarter-hour boost reads
    // "0.25h", not as real seconds.
    private static string Duration(float hours) => hours < GenDate.HoursPerDay
        ? hours.ToString("0.##") + "LetterHour".Translate()
        : (hours / GenDate.HoursPerDay).ToString("0.##") + "LetterDay".Translate();

    public override void ExposeData()
    {
        base.ExposeData();
        ExposeDressingSettings();
        ExposeShotSettings();
        ExposeWorkSettings();
        ExposeStrangerSettings();
    }

    // Reads the def-backed defaults before the first ApplyToDefs overwrites them; defs must be
    // loaded, so ModInit's static constructor calls this, never the Mod constructor. The man in
    // black's defaults need none of the set's defs, so they load ahead of the guard, as they
    // apply ahead of it.
    public void LoadDefaultsFromDefs()
    {
        LoadStrangerDefaults();
        if (!TryGetDefs(out Defs defs))
            return;
        LoadDressingDefaults(defs);
        LoadShotDefaults(defs);
        LoadWorkDefaults();
        defaultsLoaded = true;
    }

    // The work profile and the man in black go first: they need none of the dressing's defs, so
    // a saved choice still holds while they are missing, though the page is unavailable to
    // change it. The trims run after the generation moves, once every def holds its new value.
    public void ApplyToDefs()
    {
        ApplyWorkToDefs();
        ApplyStrangerToDefs();
        if (!defaultsLoaded || !TryGetDefs(out Defs defs))
            return;
        ApplyDressingToDefs(defs);
        ApplyShotToDefs(defs);
        Generation++;
        TrimWornSuites();
        TrimWornInjectors();
    }

    public void ResetToDefaults()
    {
        ResetDressingSettings();
        ResetShotSettings();
        ResetWorkSettings();
        ResetStrangerSettings();
    }

    // Each section opens on its summary under its header, then one row per setting, and a reset
    // button sits below the list. The section is headed by whitespace and a medium-font header
    // rather than a rule, which reads as another slider, and the rows sit indented under it; it
    // closes on the same gap so the list does not end flush on its last row.
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
        DrawDressingSection(listing);
        DrawShotSection(listing);
        DrawWorkSection(listing);
        DrawStrangerSection(listing);
        listing.Gap(SectionGap);
        contentHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - buttonHeight, buttonWidth, buttonHeight),
                "RestoreToDefaultSettings".Translate()))
        {
            ResetToDefaults();
        }
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
    // The height is the icon's: the ability icons are full-bleed 128px art, which a square much
    // under 48px blurs past recognition.
    private static void SectionHeader(Listing_Standard listing, string key, Texture2D icon, string tooltip, bool first = false)
    {
        const float headerGap = 4f;
        const float headerHeight = 48f;
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
        public CompProperties_MedicAI armorAI;
        public CompProperties_MedicAI shouldersAI;
    }

    // All or nothing: with any target missing (another mod patched it away) the settings leave
    // every def at its XML values and say so once, as a warning, since the set itself still
    // works.
    private static bool warned;

    private static bool TryGetDefs(out Defs defs)
    {
        AbilityDef dressing = DefDatabase<AbilityDef>.GetNamedSilentFail(DressingDefName);
        HediffDef adrenaline = DefDatabase<HediffDef>.GetNamedSilentFail(AdrenalineDefName);
        ThingDef armor = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName);
        ThingDef shoulders = DefDatabase<ThingDef>.GetNamedSilentFail(ShouldersDefName);
        defs = new Defs
        {
            dressing = dressing,
            suite = armor?.GetCompProperties<CompProperties_DressingSuite>(),
            extension = dressing?.GetModExtension<DressingExtension>(),
            motes = new ThingDef[MoteDefNames.Length],
            shot = DefDatabase<AbilityDef>.GetNamedSilentFail(ShotDefName),
            injector = shoulders?.GetCompProperties<CompProperties_ApparelReloadable>(),
            adrenaline = adrenaline,
            fade = adrenaline?.CompProps<HediffCompProperties_SeverityPerDay>(),
            armorAI = armor?.GetCompProperties<CompProperties_MedicAI>(),
            shouldersAI = shoulders?.GetCompProperties<CompProperties_MedicAI>(),
        };
        bool motesFound = true;
        for (int i = 0; i < MoteDefNames.Length; i++)
        {
            defs.motes[i] = DefDatabase<ThingDef>.GetNamedSilentFail(MoteDefNames[i]);
            motesFound &= defs.motes[i]?.graphicData != null;
        }
        bool shotFound = defs.shot != null && defs.injector != null && defs.fade != null && defs.adrenaline.stages?.Count >= 2;
        bool aiFound = defs.armorAI != null && defs.shouldersAI != null;
        if (defs.dressing != null && defs.suite != null && defs.extension != null && motesFound && shotFound && aiFound)
            return true;
        if (!warned)
            Log.Warning("[Medic Warcasket] Settings could not find the mechanite dressing ability, its dressing extension, its motes, the armor's dressing suite, the adrenaline shot ability, the shoulders' injector, the adrenaline hediff's fade and stages or the two pieces' AI entries; the set keeps its XML values.");
        warned = true;
        return false;
    }
}
