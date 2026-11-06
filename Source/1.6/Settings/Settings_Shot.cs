using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

// Shot section, the dressing section's counterpart for the adrenaline shot. On the shoulders'
// CompProperties_ApparelReloadable entry the reload item overrides ammoDef and the shots
// maxCharges; on the MDWC_AdrenalineShot AbilityDef the cooldown overrides cooldownTime,
// stored and shown as the dressing's; on the MDWC_Adrenaline HediffDef the boost and crash
// durations (game hours) together override its severity-per-day comp's severityPerDay and its
// stages' minSeverity (AdrenalineClock): the crash sets the bottom stage's share of the clock,
// and the boost is split across the levels above it in the XML's proportions. The stages'
// restFallFactors and offsets have no setting. On the ability def's ShotExtension the heart
// attack box overrides stopsHeartAttacks; its label names vanilla's HeartAttack by its label,
// translated as the game's, in vanilla's ThreatColor. The reload item's row sits above the
// sliders as the medicine's does; the heart attack box, then the raider medics box (the
// shoulders' CompProperties_MedicAI aiCasts), close the section as the dressing's does. The
// item is any medicine, herbal included (a stimulant dose does not depend on medical potency),
// or vanilla's go-juice or wake-up, listed in that order. Its header carries the ability's
// icon and tooltip.
//
// A reload item change needs nothing of the live pieces: the shots loaded count as the new
// item, as the dressing's kits do. Shots above a lowered maximum are spent off the worn
// shoulders at once (TrimWornInjectors); a shot already given keeps the total length it was
// applied with, since vanilla's comp caches the rate per instance, while the stage thresholds
// are read off the def live, so its levels move to the new split at once.
public partial class MedicWarcasketSettings
{
    // The injector's reload item: any medicine, by potency, then go-juice, then wake-up.
    public readonly ItemSetting shotAmmo = new("ShotAmmo",
        d => d.IsMedicine || d == MDWC_DefOf.GoJuice || d == MDWC_DefOf.WakeUp,
        d => d.IsMedicine ? d.GetStatValueAbstract(StatDefOf.MedicalPotency) : d == MDWC_DefOf.GoJuice ? -1f : -2f);

    // Whether a shot stops a heart attack instead of adding the surge (the ability's
    // ShotExtension).
    public readonly ToggleSetting shotHeartAttack = new("ShotHeartAttack")
    {
        Term = () => MDWC_DefOf.HeartAttack.label.Colorize(ColoredText.ThreatColor),
    };

    // Whether a non-player wearer casts the shot (the shoulders' CompMedicAI).
    public readonly ToggleSetting aiShot = new("AIShot");

    // The shot's: the injector's maxCharges, the hediff's boost and crash in game hours, and the
    // cooldown in game hours. The boost is quarter hours to 3 h, where a fight's length is
    // decided, then the cooldown's coarser bands to a day; the crash is half hours to 6 h, an
    // afternoon, then the same bands to three days.
    public readonly Tunable shots = new("Shots", 1f, 10f, 1f, Whole);
    public readonly Tunable shotBoost = new("ShotBoost", 0.25f, GenDate.HoursPerDay, 0.25f, Duration)
    {
        StepBands = [(3f, 0.25f), (12f, 1f), (GenDate.HoursPerDay, 6f)],
    };
    public readonly Tunable shotCrash = new("ShotCrash", 0.5f, 3f * GenDate.HoursPerDay, 0.5f, Duration)
    {
        StepBands = [(6f, 0.5f), (12f, 1f), (GenDate.HoursPerDay, 6f), (3f * GenDate.HoursPerDay, 12f)],
    };
    public readonly Tunable shotCooldown = new("ShotCooldown", 0f, CooldownMax, 0.25f, Period) { StepBands = CooldownBands };

    // The boost's level count (the stages above the crash), quoted by the shot's summary line.
    private int shotLevels;

    // Each boost level's share of the boost as the XML leaves it, by stage index, and the
    // severity a shot starts at; what the two durations are written through.
    private float[] boostShares;
    private float shotTop;

    // The ability def, for its header icon (see the class header), and the header's tooltip, as
    // the dressing's.
    private AbilityDef shotDef;
    private string shotTip;

    private void ExposeShotSettings()
    {
        shotAmmo.ExposeData();
        shotHeartAttack.ExposeData();
        aiShot.ExposeData();
        shots.ExposeData();
        shotBoost.ExposeData();
        shotCrash.ExposeData();
        shotCooldown.ExposeData();
    }

    private void LoadShotDefaults(Defs defs)
    {
        shotHeartAttack.DefaultValue = defs.shotExtension.stopsHeartAttacks;
        aiShot.DefaultValue = defs.shouldersAI.aiCasts;
        shotAmmo.DefaultValue = defs.injector.ammoDef;
        shots.DefaultValue = defs.injector.maxCharges;
        shotTop = defs.adrenaline.initialSeverity;
        float[] mins = defs.adrenaline.stages.Select(stage => stage.minSeverity).ToArray();
        (float boost, float crash, float[] shares) = AdrenalineClock.Read(mins, defs.fade.severityPerDay, shotTop);
        shotBoost.DefaultValue = boost;
        shotCrash.DefaultValue = crash;
        boostShares = shares;
        shotCooldown.DefaultValue = (float)defs.shot.cooldownTime / GenDate.TicksPerHour;
        shotLevels = defs.adrenaline.stages.Count - 1;
        shotTip = HeaderTip(defs.shot.LabelCap, defs.shot.description);
        shotDef = defs.shot;
    }

    private void ApplyShotToDefs(Defs defs)
    {
        defs.shotExtension.stopsHeartAttacks = shotHeartAttack.Value;
        defs.shouldersAI.aiCasts = aiShot.Value;
        ThingDef shotItem = shotAmmo.Value;
        if (shotItem != null)
            defs.injector.ammoDef = shotItem;
        defs.injector.maxCharges = Mathf.Max(1, shots.IntValue);
        defs.shot.cooldownTime = Mathf.RoundToInt(shotCooldown.Value * GenDate.TicksPerHour);
        defs.fade.severityPerDay = AdrenalineClock.SeverityPerDay(shotBoost.Value, shotCrash.Value, shotTop);
        float[] mins = AdrenalineClock.MinSeverities(shotBoost.Value, shotCrash.Value, boostShares, shotTop);
        List<HediffStage> stages = defs.adrenaline.stages;
        for (int i = 0; i < stages.Count; i++)
            stages[i].minSeverity = mins[i];
    }

    private void ResetShotSettings()
    {
        shotAmmo.Reset();
        shotHeartAttack.Reset();
        aiShot.Reset();
        shots.Reset();
        shotBoost.Reset();
        shotCrash.Reset();
        shotCooldown.Reset();
    }

    private void DrawShotSection(Listing_Standard listing)
    {
        SectionHeader(listing, "MDWC_SectionShot", shotDef.icon, shotTip);
        SummaryRow(listing, ShotSummary);
        shotAmmo.DoRow(listing);
        shots.DoRow(listing);
        shotBoost.DoRow(listing);
        shotCrash.DoRow(listing);
        shotCooldown.DoRow(listing);
        shotHeartAttack.DoRow(listing);
        aiShot.DoRow(listing);
    }

    // What the shot section's rows amount to: the shots held and the item each costs, how long
    // the boost lasts and in how many levels, how long the crash lasts, and the cooldown, with the
    // same no-cooldown clause rule as the dressing's.
    private string ShotSummary()
    {
        string item = shotAmmo.Value?.label ?? string.Empty;
        string boost = Duration(shotBoost.Value);
        string crash = Duration(shotCrash.Value);
        if (shotCooldown.Value <= 0f)
            return "MDWC_ShotSummaryNoCooldown".Translate(shots.IntValue, item, boost, shotLevels, crash);
        return "MDWC_ShotSummary".Translate(shots.IntValue, item, boost, shotLevels, crash, Period(shotCooldown.Value));
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
}
