using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;

namespace MedicWarcasket;

// The Medic armor's active: the pilot primes the suite (the warmup), then streams carrier
// mechanites onto a patient along a beam that tends one wound per interval, most urgent first,
// paying for each wound out of the suite's kits (CompDressingSuite, read off holder, which is
// why the two comps share the armor def) as it is tended, then holds still for a warmdown while
// the glow fades. With the medic helmet worn the beam also tends diseases and infections. The
// phases are the job's toils (JobDriver_MechaniteDressing); this class holds the dressing's
// state and does the work each toil calls into: PreWarmupAction and WarmupToil (the warmup),
// Cast (the connect), BeamTick, WarmdownTick and EndCastJob (the settling on every job end).
// Design in Docs/Specs/Mechanite-Dressing.md.
//
// Ordering: CreateCastJob hands the job to Pawn_JobTracker.TryTakeOrderedJob rather than VEF's
// StartAbilityJob, so a plain click interrupts the current job and clears the queue as any
// vanilla order does, a Shift-click appends (QueueOrder) and a click on the patient already
// being dressed is a no-op (JobIsSameAs). It also leaves CompAbilities.currentlyCasting alone:
// VEF writes it at order time, which a queued job would read stale; here the driver binds it
// when the job starts. ITargetingSource is re-listed so MultiSelect can be re-implemented as
// true, which keeps the vanilla Targeter open after each Shift-click; the Targeter re-enters
// through OrderForceTarget without VEF's DoTargeting, so that override orders directly instead
// of indexing currentTargets.
//
// Targeting: any pawn on the map can be clicked; the range is not a limit (GetRangeForPawn
// returns a number VEF treats as unlimited) and the def's requireLineOfSight is off, because
// the job walks the pilot to the patient and enforces sight where it matters, at the def's
// distanceToTarget. The pilot is excluded by the targeting parameters' canTargetSelf and once
// more in ValidateTarget. A hostile that is up and about is excluded too: a downed raider can be
// dressed before capture, but a conscious enemy would only be walked up to and held still.
// MouseAttachment applies vanilla's cursor rule, the no-shoot cross over an invalid target and
// def.icon (the configured medicine's, written by the settings) over a valid one; ValidateTarget
// is asked quietly there, since VEF's default would repeat the rejection message every frame.
//
// Charging (3.2 of the spec): an injury costs min(severity, severityPerKit), any other tendable
// hediff a whole kit, and a tendAllAtOnce hediff a kit for the first of its def in a dressing;
// the suite's Draw spends it, forgiving a shortfall on the last kit. Medicine XP is vanilla's
// per-treatment lump (Toils_Tend.FinalizeTend: 500 on a humanlike, 175 on an animal, times the
// medicine's MedicineTendXpGainFactor) pro rata to the kit share each wound drew, so a kit's
// worth of dressing earns one treatment's XP and chip damage cannot farm it.
//
// The stun (StunHandler.StunFor without battle log or mote) is refreshed every tick of the
// warmup and the beam with a window of a few ticks, so it lasts the warmup plus one interval
// per wound and lapses on its own on any exit; StopStun is never called, since it would also
// cut a stun from elsewhere. A downed patient is skipped: StunHandlerTick zeroes a downed pawn's
// stun every tick, and they need no holding. A sleeper is not woken, so a bed patient keeps
// the bed's tend-quality offset.
//
// Motes are not scribed; every tick recreates any that are missing, as VEF's warmupMote code
// does, so a save mid-dressing resumes with its visuals. The scribed state is the dressing in
// flight; Init runs again on the first gizmo draw after every load and must not reset it, so it
// clears only the cached suite reference.
//
// The mod settings write onto the def (castTime, cooldownTime, distanceToTarget, the extension's
// ticksPerTend) and the suite's props (ammoDef, maxCharges); the dressing reads them live, bar
// the warmup length, which VEF's wait toil fixes at job start. A running cooldown stored as an
// end tick is capped to the new length on the next gizmo request (ReconcileSettings).
public class Ability_MechaniteDressing : Ability, ITargetingSource
{
    // "Unlimited" as a finite number so VEF's range arithmetic and ring drawing stay sane.
    private const float UnlimitedRange = 10000f;

    // The stun refreshed each tick lasts this long past the last refresh.
    private const int StunRefreshTicks = 5;

    // The emitter glow, and the beam's origin, sit this far in front of the pilot.
    private const float GlowDistance = 0.5f;

    private const float TextMoteFadeDelay = 3.65f;

    private enum BeamState
    {
        Running,
        Complete,
        Dry,
        Broken,
    }

    // The dressing in flight, scribed. The medicine goes out by defName so one whose mod is gone
    // loads as nothing rather than as a missing-def error.
    private Pawn patient;
    private ThingDef medicine;
    private string medicineName;
    private float quality;
    private float maxQuality;
    private int tended;
    private int remaining;
    private int ticksIntoWound;
    private bool connected;
    private bool beaming;
    private List<HediffDef> chargedAllAtOnce = new();

    private CompDressingSuite suite;
    private int settingsGeneration = -1;
    private Vector3 glowOffset;
    private Mote glow;
    private MoteDualAttached beam;
    private Mote overlay;
    private Sustainer tendSound;
    private readonly List<Hediff> tendableScratch = new();
    private readonly HashSet<HediffDef> costScratch = new();

    public CompDressingSuite Suite => suite ??= holder?.TryGetComp<CompDressingSuite>();

    private DressingExtension Extension => def.GetModExtension<DressingExtension>();

    public bool Connected => connected;

    // Tended / (tended + still tendable), for the beam toil's progress bar.
    public float BeamProgress => tended + remaining > 0 ? (float)tended / (tended + remaining) : 0f;

    public override float GetRangeForPawn() => UnlimitedRange;

    // VEF's only auto-cast consumer reads CompAbilities.LearnedAbilities, never an apparel
    // comp's GivenAbilities, so an apparel ability's toggle would show and never fire.
    public override bool CanAutoCast => false;

    public override bool AutoCast => false;

    bool ITargetingSource.MultiSelect => true;

    public override void Init()
    {
        base.Init();
        suite = null;
    }

    public static bool WearsMedicHelmet(Pawn pilot) =>
        pilot?.apparel != null && pilot.apparel.WornApparel.Any(a => a.def == MDWC_DefOf.MDWC_WarcasketHelmet_Medic);

    public bool TreatsDiseases => WearsMedicHelmet(pawn);

    // Every hediff the beam tends, in tend-priority order: injuries and fresh missing parts,
    // plus every other tendable hediff (diseases, infections) with the helmet worn.
    private void FillTendable(Pawn patient, List<Hediff> outHediffs)
    {
        outHediffs.Clear();
        bool diseases = TreatsDiseases;
        List<Hediff> hediffs = patient.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++)
        {
            Hediff hediff = hediffs[i];
            if (hediff.TendableNow() && (diseases || hediff is Hediff_Injury || hediff is Hediff_MissingPart))
                outHediffs.Add(hediff);
        }
        TendUtility.SortByTendPriority(outHediffs);
    }

    // What one hediff draws from the suite. charged collects the tendAllAtOnce defs already
    // paid for in the same dressing (or the same estimate), which cost nothing again.
    private static float CostOf(Hediff hediff, float severityPerKit, ICollection<HediffDef> charged)
    {
        if (hediff is Hediff_Injury)
            return Mathf.Min(hediff.Severity, severityPerKit);
        HediffCompProperties_TendDuration tendProps = hediff.def.CompProps<HediffCompProperties_TendDuration>();
        if (tendProps != null && tendProps.tendAllAtOnce)
        {
            if (charged.Contains(hediff.def))
                return 0f;
            charged.Add(hediff.def);
        }
        return severityPerKit;
    }

    private float TotalCost(List<Hediff> hediffs, float severityPerKit)
    {
        costScratch.Clear();
        float total = 0f;
        for (int i = 0; i < hediffs.Count; i++)
            total += CostOf(hediffs[i], severityPerKit, costScratch);
        return total;
    }

    public override bool IsEnabledForPawn(out string reason)
    {
        if (!base.IsEnabledForPawn(out reason))
            return false;
        CompDressingSuite suite = Suite;
        if (suite == null)
            return false;
        if (suite.HasKit)
            return true;
        reason = "MDWC_NoMedicineLoaded".Translate(suite.AmmoDef.label);
        return false;
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages))
            return false;
        if (target.Thing is not Pawn patient || patient == pawn || !patient.Spawned || patient.Dead)
            return false;
        if (!patient.Downed && patient.HostileTo(pawn))
        {
            if (showMessages)
                Messages.Message("MDWC_HostileTarget".Translate(patient.LabelShortCap), patient, MessageTypeDefOf.RejectInput, historical: false);
            return false;
        }
        FillTendable(patient, tendableScratch);
        if (tendableScratch.Count > 0)
            return true;
        if (showMessages)
            Messages.Message(NothingToTendMessage(patient), patient, MessageTypeDefOf.RejectInput, historical: false);
        return false;
    }

    private string NothingToTendMessage(Pawn patient) =>
        (TreatsDiseases ? "MDWC_NothingToTreat" : "MDWC_NoWoundsToDress").Translate(patient.LabelShortCap);

    // The Targeter's re-entry after a Shift-click, which skips VEF's DoTargeting and would
    // otherwise write currentTargets[-1] through the base.
    public override void OrderForceTarget(LocalTargetInfo target) => CreateCastJob(target);

    // Vanilla's order semantics in place of VEF's StartAbilityJob (see the header). The def has
    // no AbilityExtension_AbilityMods and the pilot is never a caravan member while this shows,
    // so skipping the base's Valid, PreCast and caravan branches loses nothing.
    public override void CreateCastJob(params GlobalTargetInfo[] targets)
    {
        currentTargetingIndex = -1;
        currentTargets = new GlobalTargetInfo[def.targetCount];
        if (targets.Length == 0 || targets[0].Thing is not Pawn target || pawn.jobs == null)
            return;
        Job job = JobMaker.MakeJob(def.jobDef, target);
        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }

    // The job's check toil, with the pilot in reach: the patient still has something to treat
    // and the suite holds a kit. Nothing has been spent yet. The cast time was fixed when the
    // job started, so a zero warmup is simply a wait toil of no ticks.
    public bool CheckBeforeWarmup(Pawn patient)
    {
        if (!ValidateTarget(patient))
            return false;
        if (Suite is { HasKit: true })
            return true;
        Messages.Message("MDWC_NoMedicineLoaded".Translate(Suite?.AmmoDef.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
        return false;
    }

    // The warmup's start, at the wait toil's init: the dressing's state begins here, read off
    // the pilot's own CompAbilities (the driver bound it at job start), and the glow appears in
    // front of the pilot toward the patient, fixed for the job since the pilot stands still.
    public override void PreWarmupAction()
    {
        base.PreWarmupAction();
        patient = pawn.GetComp<CompAbilities>()?.currentlyCastingTargets?.FirstOrDefault().Thing as Pawn;
        medicine = Suite?.AmmoDef;
        tended = 0;
        remaining = 0;
        ticksIntoWound = 0;
        connected = false;
        beaming = false;
        chargedAllAtOnce.Clear();
        glowOffset = Vector3.zero;
        if (patient != null)
            MaintainGlow(1f);
    }

    public override void WarmupToil(Toil toil)
    {
        base.WarmupToil(toil);
        toil.AddPreTickAction(WarmupTick);
    }

    private void WarmupTick()
    {
        if (patient == null)
            return;
        if (!InReach(patient))
        {
            pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            return;
        }
        MaintainGlow(1f);
        RefreshStun();
    }

    // The connect, at VEF's cast toil: no base.Cast, whose first line would start the cooldown
    // (EndCastJob starts it instead, after the warmdown) and whose remaining steps this def
    // does not use. Someone else may have finished the patient during the warmup, in which case
    // the beam toil passes straight to the warmdown.
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        if (patient == null || medicine == null || Suite == null)
            return;
        FillTendable(patient, tendableScratch);
        remaining = tendableScratch.Count;
        if (remaining == 0 || !Suite.HasKit)
            return;
        quality = TendUtility.CalculateBaseTendQuality(pawn, patient, medicine);
        maxQuality = medicine.GetStatValueAbstract(StatDefOf.MedicalQualityMax);
        connected = true;
        beaming = true;
        ticksIntoWound = 0;
        MaintainBeam();
        if (medicine.GetStatValueAbstract(StatDefOf.MedicalPotency) > ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency))
            SoundDefOf.TechMedicineUsed.PlayOneShot(new TargetInfo(patient.Position, patient.Map));
    }

    // One tick of the beam toil; true once the beam has ended, by any of its three exits. Each
    // interval tends the top of the tendable set as it stands then, so a wound taken mid-beam
    // joins by priority and lengthens the beam, and each wound is paid for as it is tended.
    public bool BeamTick()
    {
        if (!beaming)
            return true;
        if (patient == null || patient.Dead || !patient.Spawned)
            return EndBeam(BeamState.Broken);
        if (!InReach(patient))
            return EndBeam(BeamState.Broken);

        MaintainGlow(1f);
        MaintainBeam();
        RefreshStun();
        MaintainTendSound();

        ticksIntoWound++;
        if (ticksIntoWound < Mathf.Max(1, Extension.ticksPerTend))
            return false;
        ticksIntoWound = 0;

        FillTendable(patient, tendableScratch);
        remaining = tendableScratch.Count;
        if (remaining == 0)
            return EndBeam(BeamState.Complete);
        CompDressingSuite suite = Suite;
        if (suite == null || !suite.HasKit)
            return EndBeam(BeamState.Dry);
        string tendText = TendOne(tendableScratch[0], suite);

        FillTendable(patient, tendableScratch);
        remaining = tendableScratch.Count;
        if (remaining == 0)
            return EndBeam(BeamState.Complete, tendText);
        if (!suite.HasKit)
            return EndBeam(BeamState.Dry, tendText);
        ThrowText(tendText);
        return false;
    }

    // Returns the wound's floating text, which the caller throws, joined to the beam's outcome
    // when this wound ended it, so the two never overlap. Batch position 1 keeps vanilla's own
    // per-hediff text off, as Coagulate does; ours reads as vanilla's, with the quality this
    // wound rolled (CompTended's random offset on the base). The label is read before tending,
    // as vanilla's is, since a fresh missing part relabels once tended.
    private string TendOne(Hediff hediff, CompDressingSuite suite)
    {
        string label = hediff.Label;
        float severityPerKit = suite.SeverityPerKit;
        float cost = CostOf(hediff, severityPerKit, chargedAllAtOnce);
        hediff.Tended(quality, maxQuality, 1);
        float rolled = hediff.TryGetComp<HediffComp_TendDuration>()?.tendQuality ?? quality;
        suite.Draw(cost);
        tended++;
        if (pawn.skills != null && cost > 0f)
        {
            float lump = patient.RaceProps.Animal ? 175f : 500f;
            pawn.skills.Learn(SkillDefOf.Medicine, lump * medicine.MedicineTendXpGainFactor * cost / severityPerKit);
        }
        FleckMaker.ThrowMetaIcon(patient.Position, patient.Map, FleckDefOf.HealingCross);
        return "TextMote_Tended".Translate(label).CapitalizeFirst() + "\n" + "Quality".Translate() + " " + rolled.ToStringPercent();
    }

    // The outcome goes under the last wound's text when a tend ended the beam. A complete beam
    // has none: each wound's text already announced it.
    private bool EndBeam(BeamState state, string tendText = null)
    {
        beaming = false;
        tendSound?.End();
        tendSound = null;
        if (patient == null || !patient.Spawned)
            return true;
        int found = tended + remaining;
        string outcome = state switch
        {
            BeamState.Complete => null,
            BeamState.Dry => "MDWC_WoundsTendedDry".Translate(medicine.label, tended, found),
            _ => "MDWC_BeamBroken".Translate(tended, found),
        };
        string text = tendText == null ? outcome : outcome == null ? tendText : tendText + "\n" + outcome;
        if (text != null)
            ThrowText(text);
        return true;
    }

    // One tick of the warmdown: only the glow is kept, its alpha falling with the fraction of
    // the warmdown left, so the fade lasts exactly the warmdown at any setting. The beam and the
    // overlay lapse (fadeOutUnmaintained), and so does the stun.
    public void WarmdownTick(float fractionLeft) => MaintainGlow(Mathf.Clamp01(fractionLeft));

    // Every job end, from VEF's driver finish action: the cooldown starts here, after the
    // warmdown, so the gizmo stays enabled through the beam and a plain click on another
    // patient interrupts the running dressing as any player order does. The once-per-treatment
    // side effects of TendUtility.DoTend (records, the guest counter and quest signal, the
    // animal bond roll, the Ideology role) are done once per dressing that tended anything.
    // Notify_SelfTended is unreachable, since the pilot cannot target themself.
    public override void EndCastJob()
    {
        base.EndCastJob();
        tendSound?.End();
        tendSound = null;
        if (connected)
            cooldown = Find.TickManager.TicksGame + GetCooldownForPawn();
        bool pilotAlive = pawn != null && !pawn.Dead;
        if (beaming && patient != null && patient.Spawned)
            ThrowText("MDWC_WoundsTendedPartial".Translate(tended, tended + remaining));
        if (tended > 0 && patient != null && pilotAlive)
        {
            patient.records.Increment(RecordDefOf.TimesTendedTo);
            pawn.records.Increment(RecordDefOf.TimesTendedOther);
            if (pawn.Faction == Faction.OfPlayer)
            {
                if (patient.Faction != null && patient.Faction != pawn.Faction && !patient.IsPrisoner)
                    patient.mindState.timesGuestTendedToByPlayer++;
                QuestUtility.SendQuestTargetSignals(patient.questTags, "PlayerTended", patient.Named("SUBJECT"));
            }
            if (pawn.RaceProps.Humanlike && patient.RaceProps.Animal && patient.RaceProps.playerCanChangeMaster
                && RelationsUtility.TryDevelopBondRelation(pawn, patient, 0.004f)
                && pawn.Faction != null && pawn.Faction != patient.Faction)
                InteractionWorker_RecruitAttempt.DoRecruit(pawn, patient, useAudiovisualEffects: false);
            if (ModsConfig.IdeologyActive && pawn.Ideo != null && pawn.Ideo.GetRole(pawn)?.def.roleEffects is { } roleEffects)
                foreach (RoleEffect roleEffect in roleEffects)
                    roleEffect.Notify_Tended(pawn, patient);
        }
        patient = null;
        medicine = null;
        tended = 0;
        remaining = 0;
        ticksIntoWound = 0;
        connected = false;
        beaming = false;
        chargedAllAtOnce.Clear();
        glowOffset = Vector3.zero;
        glow = null;
        beam = null;
        overlay = null;
    }

    // Within the def's distanceToTarget with a clear line, the walk's own stopping rule.
    private bool InReach(Pawn patient) =>
        pawn.Spawned && patient.Spawned && patient.Map == pawn.Map
        && pawn.Position.DistanceTo(patient.Position) <= def.distanceToTarget
        && GenSight.LineOfSight(pawn.Position, patient.Position, pawn.Map, skipFirstCell: true);

    private void ThrowText(string text) =>
        MoteMaker.ThrowText(patient.DrawPos, patient.Map, text, Color.white, TextMoteFadeDelay);

    private void RefreshStun()
    {
        if (patient.Downed || patient.Dead)
            return;
        patient.stances?.stunner?.StunFor(StunRefreshTicks, pawn, addBattleLog: false, showMote: false);
    }

    private Vector3 GlowOffset()
    {
        if (glowOffset == Vector3.zero && patient != null)
        {
            Vector3 toward = (patient.DrawPos - pawn.DrawPos).Yto0();
            glowOffset = toward.sqrMagnitude > 0.001f ? toward.normalized * GlowDistance : new Vector3(0f, 0f, GlowDistance);
        }
        return glowOffset;
    }

    private void MaintainGlow(float alpha)
    {
        if (!pawn.Spawned)
            return;
        if (glow == null || glow.Destroyed)
            glow = MoteMaker.MakeAttachedOverlay(pawn, MDWC_DefOf.MDWC_Mote_DressingGlow, GlowOffset());
        if (glow == null)
            return;
        glow.instanceColor = new Color(1f, 1f, 1f, alpha);
        glow.Maintain();
    }

    private void MaintainBeam()
    {
        if (!pawn.Spawned || !patient.Spawned)
            return;
        if (beam == null || beam.Destroyed)
            beam = MoteMaker.MakeInteractionOverlay(MDWC_DefOf.MDWC_Mote_DressingBeam, pawn, patient, GlowOffset(), Vector3.zero);
        beam?.Maintain();
        if (overlay == null || overlay.Destroyed)
            overlay = MoteMaker.MakeAttachedOverlay(patient, MDWC_DefOf.MDWC_Mote_DressingOverlay, Vector3.zero);
        overlay?.Maintain();
    }

    // The beam's own tend loop: VEF's warmupSound sustainer lapses with the warmup toil.
    private void MaintainTendSound()
    {
        if (tendSound == null || tendSound.Ended)
            tendSound = SoundDefOf.Interact_Tend.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
        else
            tendSound.Maintain();
    }

    // Vanilla's cursor rule: the no-shoot cross over anything the dressing cannot be ordered
    // on, else the def's icon through the base. ValidateTarget is asked quietly, since VEF's
    // default would repeat its rejection message every frame; the vanilla Targeter hands a pawn
    // under the mouse through without its hit test, so the validity is decided here.
    protected override Texture2D MouseAttachment(GlobalTargetInfo target)
    {
        if (target.Thing is not Pawn patient || !ValidateTarget(patient, showMessages: false))
            return TexCommand.CannotShoot;
        return base.MouseAttachment(target);
    }

    // The label beside the cursor: the medicine and, over a valid patient, the base quality
    // the formula would give them (bed offset included), then the wounds and the kits they
    // cost, with what is loaded when that is less.
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        CompDressingSuite suite = Suite;
        ThingDef medicine = suite?.AmmoDef;
        if (medicine == null)
            return null;
        if (target.Thing is not Pawn patient || !ValidateTarget(patient, showMessages: false))
            return medicine.LabelCap;
        string first = "MDWC_TargetQuality".Translate(medicine.LabelCap, TendUtility.CalculateBaseTendQuality(pawn, patient, medicine).ToStringPercent());
        FillTendable(patient, tendableScratch);
        float kits = TotalCost(tendableScratch, suite.SeverityPerKit) / suite.SeverityPerKit;
        string kitsText = kits.ToString("0.0");
        string second = kits > suite.Available
            ? "MDWC_TargetCostShort".Translate(tendableScratch.Count, kitsText, suite.Available.ToString("0.0"))
            : "MDWC_TargetCost".Translate(tendableScratch.Count, kitsText);
        return first + "\n" + second;
    }

    public override Gizmo GetGizmo()
    {
        if (settingsGeneration != MedicWarcasketSettings.Generation)
            ReconcileSettings();
        return base.GetGizmo();
    }

    // A running cooldown is an end tick, so a lowered cooldown setting would not shorten it
    // and VEF's cooldown overlay would overflow the button; cap it at the new length.
    private void ReconcileSettings()
    {
        settingsGeneration = MedicWarcasketSettings.Generation;
        int ceiling = Find.TickManager.TicksGame + GetCooldownForPawn();
        if (cooldown > ceiling)
            cooldown = ceiling;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref patient, "patient");
        if (Scribe.mode == LoadSaveMode.Saving)
            medicineName = medicine?.defName;
        Scribe_Values.Look(ref medicineName, "medicine");
        Scribe_Values.Look(ref quality, "quality");
        Scribe_Values.Look(ref maxQuality, "maxQuality");
        Scribe_Values.Look(ref tended, "tended");
        Scribe_Values.Look(ref remaining, "remaining");
        Scribe_Values.Look(ref ticksIntoWound, "ticksIntoWound");
        Scribe_Values.Look(ref connected, "connected");
        Scribe_Values.Look(ref beaming, "beaming");
        Scribe_Collections.Look(ref chargedAllAtOnce, "chargedAllAtOnce", LookMode.Def);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            chargedAllAtOnce ??= new List<HediffDef>();
            medicine = medicineName == null ? null : DefDatabase<ThingDef>.GetNamedSilentFail(medicineName);
            if (medicine != null && !medicine.IsMedicine)
                medicine = null;
        }
        if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.PostLoadInit)
            medicineName = null;
    }
}
