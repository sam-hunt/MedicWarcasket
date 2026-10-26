using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;
using VEF.Abilities;
using Command_Ability = VEF.Abilities.Command_Ability;

namespace MedicWarcasket;

// The Medic shoulders' active: the pilot walks to a patient (or is one), holds them for the
// jab, then gives a shot out of the shoulders' injector (vanilla's per-charge
// CompApparelReloadable, one reload item a shot, read off holder, which is why the two comps
// share the shoulders def). A shot is
// spent on one of two things: a patient with a heart attack has every one removed and nothing
// else, announced top-left as a positive event naming them; any other patient gets the
// MDWC_Adrenaline hediff at full severity, a fresh one or an existing one refreshed (set, not
// added: vanilla would merge the severities). Apply is the one path, used by the cast toil and
// by the dressing's autocast toil alike, so the charge, the cooldown and the sound are the
// same either way. Design in Docs/Specs/Adrenaline-Shot.md.
//
// Validation, in order: a pawn with a heart attack is always valid, hostile or not, since a
// cure-only shot stands nobody up; otherwise no pawn hostile to the pilot, downed or not,
// since a shot that lifts a downed raider's pain can stand them back up (stricter than the
// dressing's rule, on purpose); otherwise no patient already at the top level, so a double
// click cannot waste a shot, while a patient further down may be refreshed.
//
// Autocast is VEF's own scribed flag on this instance, toggled by VEF's right-click, offered
// (CanAutoCast) only while the pilot wears the armor whose dressing fires it
// (Ability_MechaniteDressing.ShotToFollow); VEF itself never fires an apparel ability. Init
// keeps the flag across the re-bind VEF runs on every wearer change and first gizmo draw,
// which would otherwise reset it (Toxin's idiom).
//
// Gizmo: one cached Command_AdrenalineShot (VEF builds a new command, tooltip and all, on every
// request), with the enable state refreshed every call and the description from a cache keyed
// on the settings generation and the autocast state, the two things in it that change. The
// description is the def's, then a "Reloads with" paragraph naming the injector's AmmoDef
// (the reload item is a mod setting, so the def's text cannot name it), then the autocast line.
// Drawing is a flag the command raises around VEF's draw so AutoCast reads false there and
// VEF's top-right check, where the count sits, is not drawn; the command draws it top-left.
public class Ability_AdrenalineShot : Ability_MedicTargeted
{
    private Command_AdrenalineShot gizmo;
    private string description;
    private int descriptionGeneration = -1;
    private bool descriptionAutoCast;

    // Raised by the command while VEF draws the button (see the header).
    public bool Drawing { get; set; }

    public CompApparelReloadable Injector => holder?.TryGetComp<CompApparelReloadable>();

    public string ShotsLabel => Injector is { } injector ? injector.LabelRemaining : null;

    public static Ability_AdrenalineShot WornBy(Pawn pilot) =>
        JobDriver_MedicCast.FindAbility(pilot, MDWC_DefOf.MDWC_AdrenalineShot) as Ability_AdrenalineShot;

    public static bool HasHeartAttack(Pawn patient) =>
        patient.health?.hediffSet.HasHediff(MDWC_DefOf.HeartAttack) ?? false;

    private static Hediff Adrenaline(Pawn patient) =>
        patient.health?.hediffSet.GetFirstHediffOfDef(MDWC_DefOf.MDWC_Adrenaline);

    private static bool AtTopLevel(Pawn patient) =>
        Adrenaline(patient) is { } hediff && hediff.CurStageIndex == hediff.def.stages.Count - 1;

    private Pawn Patient => pawn?.GetComp<CompAbilities>()?.currentlyCastingTargets?.FirstOrDefault().Thing as Pawn;

    public override bool CanAutoCast =>
        pawn != null && JobDriver_MedicCast.FindAbility(pawn, MDWC_DefOf.MDWC_MechaniteDressing) != null;

    public override bool AutoCast => !Drawing && pawn != null && CanAutoCast
        && (pawn.IsColonistPlayerControlled ? autoCast : pawn.Spawned);

    public override void Init()
    {
        bool wasAutoCast = autoCast;
        base.Init();
        autoCast = wasAutoCast;
        gizmo = null;
        description = null;
    }

    public override bool IsEnabledForPawn(out string reason)
    {
        if (!base.IsEnabledForPawn(out reason))
            return false;
        CompApparelReloadable injector = Injector;
        if (injector == null)
            return false;
        if (injector.RemainingCharges > 0)
            return true;
        reason = "MDWC_NoShotsLoaded".Translate();
        return false;
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages))
            return false;
        if (target.Thing is not Pawn patient || !patient.Spawned || patient.Dead)
            return false;
        if (HasHeartAttack(patient))
            return true;
        if (patient != pawn && patient.HostileTo(pawn))
        {
            if (showMessages)
                Messages.Message("MDWC_ShotHostile".Translate(patient.LabelShortCap), patient, MessageTypeDefOf.RejectInput, historical: false);
            return false;
        }
        if (AtTopLevel(patient))
        {
            if (showMessages)
                Messages.Message("MDWC_ShotAlreadyRushing".Translate(patient.LabelShortCap), patient, MessageTypeDefOf.RejectInput, historical: false);
            return false;
        }
        return true;
    }

    public override bool CheckBeforeWarmup(Pawn patient)
    {
        if (!ValidateTarget(patient))
            return false;
        if (Injector is { RemainingCharges: > 0 })
            return true;
        Messages.Message("MDWC_NoShotsLoaded".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
        return false;
    }

    // The jab's warmup holds a conscious patient; one carried out of reach ends the job with
    // nothing spent and no cooldown.
    public override void WarmupToil(Toil toil)
    {
        base.WarmupToil(toil);
        toil.AddPreTickAction(() =>
        {
            Pawn patient = Patient;
            if (patient == null)
                return;
            if (!InReach(patient))
            {
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                return;
            }
            RefreshStun(patient);
        });
    }

    // No base.Cast: its first line would start the cooldown and its remaining steps (goodwill,
    // extensions, flecks, castSound) are unused; Apply does the one thing the shot needs.
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        if (targets.Length > 0 && targets[0].Thing is Pawn patient)
            Apply(patient);
    }

    // The shot itself, for the cast toil and the dressing's autocast alike: a charge, then
    // the heart or the surge, the sound, and the cooldown where VEF's Cast would have set it.
    public void Apply(Pawn patient)
    {
        CompApparelReloadable injector = Injector;
        if (injector == null || injector.RemainingCharges <= 0 || patient == null || patient.Dead)
            return;
        injector.UsedOnce();
        if (HasHeartAttack(patient))
        {
            List<Hediff> attacks = patient.health.hediffSet.hediffs.Where(h => h.def == MDWC_DefOf.HeartAttack).ToList();
            foreach (Hediff attack in attacks)
                patient.health.RemoveHediff(attack);
            Messages.Message("MDWC_HeartAttackStopped".Translate(patient.LabelShortCap), patient, MessageTypeDefOf.PositiveEvent);
        }
        else if (Adrenaline(patient) is { } existing)
        {
            existing.Severity = MDWC_DefOf.MDWC_Adrenaline.initialSeverity;
        }
        else
        {
            patient.health.AddHediff(MDWC_DefOf.MDWC_Adrenaline);
        }
        if (patient.Spawned)
            MDWC_DefOf.Ingest_Inject.PlayOneShot(new TargetInfo(patient.Position, patient.Map));
        cooldown = Find.TickManager.TicksGame + GetCooldownForPawn();
    }

    // The label beside the cursor: the shots loaded and, over a patient with a heart attack,
    // what the shot will do about it.
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        CompApparelReloadable injector = Injector;
        if (injector == null)
            return null;
        string first = "MDWC_ShotsLabel".Translate(injector.RemainingCharges, injector.MaxCharges);
        if (target.Thing is Pawn patient && HasHeartAttack(patient) && ValidateTarget(patient, showMessages: false))
            return first + "\n" + "MDWC_ShotStopsHeartAttack".Translate();
        return first;
    }

    public override string GetDescriptionForPawn()
    {
        bool auto = AutoCast;
        int generation = MedicWarcasketSettings.Generation;
        if (description != null && descriptionGeneration == generation && descriptionAutoCast == auto)
            return description;
        descriptionGeneration = generation;
        descriptionAutoCast = auto;
        description = base.GetDescriptionForPawn();
        if (Injector?.AmmoDef is { } item)
            description += "\n\n" + "MDWC_ShotReloadsWith".Translate(item.label);
        if (CanAutoCast)
            description += "\n" + "MDWC_ShotAutocastDesc".Translate();
        return description;
    }

    public override Gizmo GetGizmo()
    {
        ReconcileSettings();
        if (gizmo == null)
        {
            gizmo = (Command_AdrenalineShot)(Command_Ability)base.GetGizmo();
            return gizmo;
        }
        gizmo.Disabled = !IsEnabledForPawn(out string reason);
        gizmo.disabledReason = reason.Colorize(ColorLibrary.RedReadable);
        gizmo.defaultDesc = GetDescriptionForPawn();
        return gizmo;
    }
}
