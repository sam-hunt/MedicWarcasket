using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;

namespace MedicWarcasket;

// The Medic armor's active: one cast tends every tendable wound on a targeted person or
// animal at the quality vanilla's formula gives this pilot with the loaded medicine, spending
// one dose from the armor's suite (CompDressingSuite, read off holder, which is why the
// two comps share the armor def). With the medic helmet worn the cast also tends diseases and
// infections. Modelled on Biotech's Coagulate, which likewise ignores vanilla's one-treatment
// batch rule (TendUtility.GetOptimalHediffsToTendWithSingleTreatment) and tends everything.
//
// Targeting: any pawn on the map can be clicked; the range is not a limit (GetRangeForPawn
// returns a number VEF treats as unlimited: no range ring, no range line in the tooltip,
// CanHitTarget passes the distance test) and the def's requireLineOfSight is off, because the
// job (JobDriver_MechaniteDressing) walks the pilot to the patient and enforces sight where it
// matters, at the def's distanceToTarget. The pilot is excluded by the targeting parameters'
// canTargetSelf, which the vanilla Targeter honours, and once more in ValidateTarget. A
// hostile that is up and about is excluded too: faction is otherwise not checked, so a downed
// raider can be dressed before capture, but a conscious enemy would only be walked up to and
// held still for the cast.
//
// The medicine is fixed at the click: DoAction snapshots the suite's selection for the
// targeter, CreateCastJob carries it into pendingMedicine (scribed, so a save mid-walk keeps
// it) when the target is confirmed, and the walk and the cast read that; changing the gizmo's
// selection mid-walk, or opening and cancelling a second targeting, changes the next cast,
// not this one. A pending type with no dose left at cast time, reachable only through dev
// tools, aborts the cast with the disabled reason and no cooldown; so does a patient someone
// else finished tending during the walk.
//
// Stun: Verb_CastAbility.TryStartCastOn's stunTargetWhileCasting is vanilla's, with no VEF
// equivalent, so PreWarmupAction mirrors it (StunFor without battle log or mote) to hold a
// conscious patient still for the cast; a stun neither ends a bed patient's rest nor moves a
// downed one, and a sleeper is not woken, since RestUtility.WakeUp would end the rest job and
// with it the bed's tend-quality offset.
//
// Tend quality and XP follow vanilla's DoTend where it applies: CalculateBaseTendQuality with
// the dose's stat bases (the patient's bed offset and the self-tend factor come with it), the
// tend records, the guest counter and quest signal, the glitterworld sound. The per-hediff
// "Tended / Quality" text vanilla throws for batch position 0 is suppressed (every hediff is
// tended at position 1, as Coagulate does) in favour of one text with the count and the base
// quality. Biotech's NumWoundsTended key is not in Core, so the text is this mod's own.
// Medicine XP is a flat 60 (one base-length tend job's worth at SkillTuning.XpPerTickDefault
// over 600 ticks), since a per-wound scale would reward chip damage.
//
// The mod settings write onto the def (cast time, cooldown, distanceToTarget) and the suite's
// props (maxCharges); the ability reads them live. The one thing a settings write leaves on an
// instance is a running cooldown stored as an end tick, capped to the new length on the next
// gizmo request (ReconcileSettings), as Shipcracker's jump does.
public class Ability_MechaniteDressing : Ability
{
    // "Unlimited" as a finite number so VEF's range arithmetic and ring drawing stay sane.
    private const float UnlimitedRange = 10000f;

    // One vanilla tend job's worth of Medicine XP: the wait toil learns XpPerTickDefault per
    // tick over its 600 base ticks.
    private const float TreatmentXp = 600f * SkillTuning.XpPerTickDefault;

    private const float TextMoteFadeDelay = 3.65f;

    // The selection snapshotted at the click, for the targeter; then the type the ordered cast
    // spends. Scribed by defName so a medicine whose mod is gone loads as nothing rather than
    // as a missing-def error.
    private ThingDef targetingMedicine;
    private ThingDef pendingMedicine;
    private string pendingMedicineName;

    private CompDressingSuite suite;
    private int settingsGeneration = -1;
    private readonly List<Hediff> tendableScratch = new();

    public CompDressingSuite Suite => suite ??= holder?.TryGetComp<CompDressingSuite>();

    // The type the targeter shows: the click's snapshot while targeting, else the selection.
    private ThingDef TargetingMedicine => targetingMedicine ?? Suite?.Selected;

    public override float GetRangeForPawn() => UnlimitedRange;

    // VEF's only auto-cast consumer reads CompAbilities.LearnedAbilities, never an apparel
    // comp's GivenAbilities, so an apparel ability's toggle would show and never fire.
    public override bool CanAutoCast => false;

    public override bool AutoCast => false;

    public override void Init()
    {
        base.Init();
        suite = null;
    }

    public static bool WearsMedicHelmet(Pawn pilot) =>
        pilot?.apparel != null && pilot.apparel.WornApparel.Any(a => a.def == MDWC_DefOf.MDWC_WarcasketHelmet_Medic);

    public bool TreatsDiseases => WearsMedicHelmet(pawn);

    // Every hediff one cast tends, in tend-priority order: injuries and fresh missing parts,
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

    public override bool IsEnabledForPawn(out string reason)
    {
        if (!base.IsEnabledForPawn(out reason))
            return false;
        CompDressingSuite suite = Suite;
        if (suite == null)
            return false;
        ThingDef medicine = suite.Selected;
        if (suite.HasDose(medicine))
            return true;
        reason = "MDWC_NoMedicineLoaded".Translate(medicine.label);
        return false;
    }

    public override void DoAction()
    {
        targetingMedicine = Suite?.Selected;
        base.DoAction();
    }

    public override void CreateCastJob(params GlobalTargetInfo[] targets)
    {
        pendingMedicine = TargetingMedicine;
        targetingMedicine = null;
        base.CreateCastJob(targets);
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

    // Runs at the wait toil's init, with the pilot in reach of the patient. The targets are
    // read off the pilot's own CompAbilities rather than VEF's cached Comp, which Init never
    // resets and so may still be a previous wearer's.
    public override void PreWarmupAction()
    {
        base.PreWarmupAction();
        int ticks = GetCastTimeForPawn();
        if (ticks <= 0 || pawn.GetComp<CompAbilities>()?.currentlyCastingTargets?.FirstOrDefault().Thing is not Pawn patient || patient == pawn)
            return;
        patient.stances?.stunner?.StunFor(ticks, pawn, addBattleLog: false, showMote: false);
    }

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        CompDressingSuite suite = Suite;
        ThingDef medicine = pendingMedicine ?? suite?.Selected;
        pendingMedicine = null;
        if (suite == null || medicine == null || targets.Length == 0 || targets[0].Thing is not Pawn patient)
            return;

        var tended = new List<Hediff>();
        FillTendable(patient, tended);
        if (tended.Count == 0)
        {
            Messages.Message(NothingToTendMessage(patient), patient, MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        if (!suite.TryConsume(medicine))
        {
            Messages.Message("MDWC_NoMedicineLoaded".Translate(medicine.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        base.Cast(targets);

        float quality = TendUtility.CalculateBaseTendQuality(pawn, patient, medicine);
        float maxQuality = medicine.GetStatValueAbstract(StatDefOf.MedicalQualityMax);
        for (int i = 0; i < tended.Count; i++)
            tended[i].Tended(quality, maxQuality, 1);

        if (patient.Spawned)
        {
            string text = "MDWC_WoundsTended".Translate(tended.Count) + "\n" + "Quality".Translate() + " " + quality.ToStringPercent();
            MoteMaker.ThrowText(patient.DrawPos, patient.Map, text, Color.white, TextMoteFadeDelay);
            if (medicine.GetStatValueAbstract(StatDefOf.MedicalPotency) > ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency))
                SoundDefOf.TechMedicineUsed.PlayOneShot(new TargetInfo(patient.Position, patient.Map));
        }

        patient.records.Increment(RecordDefOf.TimesTendedTo);
        pawn.records.Increment(RecordDefOf.TimesTendedOther);
        pawn.skills?.Learn(SkillDefOf.Medicine, TreatmentXp);
        if (pawn.Faction == Faction.OfPlayer)
        {
            if (patient.Faction != null && patient.Faction != pawn.Faction && !patient.IsPrisoner)
                patient.mindState.timesGuestTendedToByPlayer++;
            QuestUtility.SendQuestTargetSignals(patient.questTags, "PlayerTended", patient.Named("SUBJECT"));
        }
    }

    // The cursor over a target: the medicine's item icon when the dressing can be applied
    // there (the def has no icon of its own), vanilla's no-shoot cross otherwise. The vanilla
    // Targeter hands a pawn under the mouse through without its hit test, so the validity is
    // decided here.
    protected override Texture2D MouseAttachment(GlobalTargetInfo target)
    {
        if (target.Thing is not Pawn patient || !ValidateTarget(patient, showMessages: false))
            return TexCommand.CannotShoot;
        Texture2D medicineIcon = TargetingMedicine?.uiIcon;
        return medicineIcon != null ? medicineIcon : TexCommand.Attack;
    }

    // The label beside the cursor: the medicine, and over a valid patient the base quality the
    // formula would give them, bed offset included.
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        ThingDef medicine = TargetingMedicine;
        if (medicine == null)
            return null;
        if (target.Thing is Pawn patient && ValidateTarget(patient, showMessages: false))
            return "MDWC_TargetQuality".Translate(medicine.LabelCap, TendUtility.CalculateBaseTendQuality(pawn, patient, medicine).ToStringPercent());
        return medicine.LabelCap;
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
        if (Scribe.mode == LoadSaveMode.Saving)
            pendingMedicineName = pendingMedicine?.defName;
        Scribe_Values.Look(ref pendingMedicineName, "pendingMedicine");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            pendingMedicine = pendingMedicineName == null ? null : DefDatabase<ThingDef>.GetNamedSilentFail(pendingMedicineName);
            if (pendingMedicine != null && !pendingMedicine.IsMedicine)
                pendingMedicine = null;
        }
        if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.PostLoadInit)
            pendingMedicineName = null;
    }
}
