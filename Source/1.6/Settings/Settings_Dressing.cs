using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

// Dressing section. On the armor's CompProperties_DressingSuite entry, the medicine overrides
// ammoDef and the kits maxCharges. On the MDWC_MechaniteDressing AbilityDef, the casting time,
// cooldown and range override castTime (in seconds, ticks on the def), cooldownTime (stored in
// game hours, shown in seconds, hours or days by its size; ticks on the def) and
// distanceToTarget, and the time per wound overrides its DressingExtension's ticksPerTend (in
// seconds, like the casting time). The tint overrides the rgb of the three dressing motes'
// graphicData.color, each keeping its own alpha, and the gauge reads its colours off the
// beam's (Gizmo_DressingSuite). The raider medics box overrides the aiCasts of the armor's
// CompProperties_MedicAI entry, read live by the comp. A line of what its rows amount to opens
// the section, under its header, the two picked rows sit above the sliders and the box closes
// the section. Its header carries the ability's icon and tooltip.
//
// Kits above a lowered maximum are trimmed off the worn suites at once (TrimWornSuites). A
// medicine change needs nothing of that: kits are a vanilla charge count with no type of their
// own, so those loaded count as the new medicine and the next reload fetches it. A tint change
// reaches every mote spawned after it; one in flight keeps the graphic it was spawned with.
public partial class MedicWarcasketSettings
{
    // The suite's medicine: any medicine, the most potent first. A weak one needs no rule of
    // its own: the dressing's quality is vanilla's, so herbal's potency and 70% quality cap
    // tend as a doctor's herbal kit would.
    public readonly ItemSetting medicine = new("Medicine",
        d => d.IsMedicine,
        d => d.GetStatValueAbstract(StatDefOf.MedicalPotency));
    public readonly TintSetting tint = new();

    // Whether a non-player wearer casts the dressing (the armor's CompMedicAI).
    public readonly ToggleSetting aiDressing = new("AIDressing");

    // The suite's maxCharges. The casting time is banded: quarter seconds at the short end, where they
    // matter, coarser steps as it reaches a slow dressing.
    // The range is the touch stop, 1.9, then whole cells 2..12, one band so the minimum sits off
    // the grid. A whole-number radius reaches the cells the game's own ring of that radius would
    // (DistanceTo is the straight-line distance compared with <=, and the square root of a
    // square is exact in float), and the 2 default must be a stop since the Tunable clears a
    // value within half a step of it. 1.9 is touching, diagonals included; a minimum below 1.42
    // would refuse a diagonal neighbour, where the walk's path to touch may already stand.
    public readonly Tunable kits = new("Kits", 1f, 20f, 1f, Whole);
    public readonly Tunable castTime = new("CastTime", 0f, 10f, 0.25f, RealSeconds)
    {
        StepBands = [(3f, 0.25f), (6f, 0.5f), (10f, 1f)],
    };
    public readonly Tunable tendInterval = new("TendInterval", 0.25f, 5f, 0.25f, RealSeconds);
    public readonly Tunable cooldown = new("Cooldown", 0f, CooldownMax, 0.25f, Period) { StepBands = CooldownBands };
    public readonly Tunable range = new("Range", 1.9f, 12f, 1f, v => v.ToString("0.#"))
    {
        StepBands = [(12f, 1f)],
    };

    // The severity one kit treats, quoted by the summary line; a def field with no setting.
    private int severityPerKit;

    // Each mote's graphicData.color as the XML leaves it, by MoteDefNames order.
    private Color[] moteColors;

    // The ability def, for its header icon (see the class header), and the header's tooltip:
    // the ability's name as VEF titles an ability's tooltip over the description its command
    // shows in game.
    private AbilityDef dressingDef;
    private string dressingTip;

    private void ExposeDressingSettings()
    {
        medicine.ExposeData();
        tint.ExposeData();
        aiDressing.ExposeData();
        kits.ExposeData();
        castTime.ExposeData();
        tendInterval.ExposeData();
        cooldown.ExposeData();
        range.ExposeData();
    }

    private void LoadDressingDefaults(Defs defs)
    {
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
        dressingDef = defs.dressing;
        aiDressing.DefaultValue = defs.armorAI.aiCasts;
    }

    private void ApplyDressingToDefs(Defs defs)
    {
        ThingDef chosen = medicine.Value;
        if (chosen != null)
            defs.suite.ammoDef = chosen;
        defs.suite.maxCharges = Mathf.Max(1, kits.IntValue);
        defs.dressing.castTime = Mathf.RoundToInt(castTime.Value * GenTicks.TicksPerRealSecond);
        defs.dressing.cooldownTime = Mathf.RoundToInt(cooldown.Value * GenDate.TicksPerHour);
        defs.dressing.distanceToTarget = range.Value;
        defs.extension.ticksPerTend = Mathf.Max(1, Mathf.RoundToInt(tendInterval.Value * GenTicks.TicksPerRealSecond));
        defs.armorAI.aiCasts = aiDressing.Value;
        for (int i = 0; i < defs.motes.Length; i++)
        {
            Color rgb = tint.Override ?? moteColors[i];
            defs.motes[i].graphicData.color = new Color(rgb.r, rgb.g, rgb.b, moteColors[i].a);
            RebuildGraphic(defs.motes[i]);
        }
    }

    private void ResetDressingSettings()
    {
        medicine.Reset();
        tint.Reset();
        aiDressing.Reset();
        kits.Reset();
        castTime.Reset();
        tendInterval.Reset();
        cooldown.Reset();
        range.Reset();
    }

    private void DrawDressingSection(Listing_Standard listing)
    {
        SectionHeader(listing, "MDWC_SectionDressing", dressingDef.icon, dressingTip, first: true);
        SummaryRow(listing, DressingSummary);
        medicine.DoRow(listing);
        tint.DoRow(listing);
        kits.DoRow(listing);
        castTime.DoRow(listing);
        tendInterval.DoRow(listing);
        cooldown.DoRow(listing);
        range.DoRow(listing);
        aiDressing.DoRow(listing);
    }

    // What the section's rows amount to: the suite's capacity and medicine, what a kit treats,
    // and the dressing's timings, in the formats their rows show; a cooldown of 0 ends the line
    // on its own clause rather than "0s of cooldown".
    private string DressingSummary()
    {
        string medicineLabel = medicine.Value?.label ?? string.Empty;
        string casting = RealSeconds(castTime.Value);
        string perWound = RealSeconds(tendInterval.Value);
        if (cooldown.Value <= 0f)
            return "MDWC_DressingSummaryNoCooldown".Translate(kits.IntValue, medicineLabel, severityPerKit, casting, perWound);
        return "MDWC_DressingSummary".Translate(kits.IntValue, medicineLabel, severityPerKit, casting, perWound, Period(cooldown.Value));
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
}
