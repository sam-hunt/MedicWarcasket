using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Ability = VEF.Abilities.Ability;

namespace MedicWarcasket;

// What the set's two actives share: a pawn-targeted ability the pilot walks to and casts on
// through its own job (JobDriver_MedicCast), ordered and targeted as vanilla orders are. The
// mechanite dressing (Ability_MechaniteDressing) and the adrenaline shot
// (Ability_AdrenalineShot) derive from it and keep their own validation, cast and state.
//
// Ordering: CreateCastJob hands the job to Pawn_JobTracker.TryTakeOrderedJob rather than VEF's
// StartAbilityJob, so a plain click interrupts the current job and clears the queue as any
// vanilla order does, a Shift-click appends (QueueOrder) and a click on the patient already
// being treated is a no-op (JobIsSameAs). It also leaves CompAbilities.currentlyCasting alone:
// VEF writes it at order time, which a queued job would read stale; here the driver binds it
// when the job starts. ITargetingSource is re-listed so MultiSelect can be re-implemented as
// true, which keeps the vanilla Targeter open after each Shift-click; the Targeter re-enters
// through OrderForceTarget without VEF's DoTargeting, so that override orders directly instead
// of indexing currentTargets.
//
// Targeting: any pawn on the map can be clicked; the range is not a limit (GetRangeForPawn
// returns a number VEF treats as unlimited) and the defs' requireLineOfSight is off, because
// the job walks the pilot to the patient and enforces sight where it matters, at the def's
// distanceToTarget. MouseAttachment applies vanilla's cursor rule, the no-shoot cross over an
// invalid target and def.icon over a valid one; ValidateTarget is asked quietly there, since
// VEF's default would repeat the rejection message every frame. HidePawnTooltips is on, as on
// Ideology's Convert and Counsel: the hovered pawn's tooltip (TooltipGiverList) opens at the
// cursor where the cursor label already sits, and both reflow with the cursor's distance to
// the screen edge, so no offset keeps them apart.
//
// Autocast is pinned off here: VEF's only auto-cast consumer reads CompAbilities.LearnedAbilities,
// never an apparel comp's GivenAbilities, so an apparel ability's toggle would show and never
// fire. The shot turns it back on and fires it itself.
//
// The stun (StunHandler.StunFor without battle log or mote) is refreshed each tick an ability
// wants a conscious patient held, with a window of a few ticks, so it lapses on its own on any
// exit; StopStun is never called, since it would also cut a stun from elsewhere. A downed
// patient is skipped: StunHandlerTick zeroes a downed pawn's stun every tick, and they need no
// holding. A sleeper is not woken. The pilot is never stunned, for a self-target.
//
// The mod settings write onto the defs; a running cooldown stored as an end tick is capped to
// the new length on the next gizmo request (ReconcileSettings, keyed on the settings
// generation), since VEF's cooldown overlay would otherwise overflow the button.
public abstract class Ability_MedicTargeted : Ability, ITargetingSource
{
    // "Unlimited" as a finite number so VEF's range arithmetic and ring drawing stay sane.
    private const float UnlimitedRange = 10000f;

    // The stun refreshed each tick lasts this long past the last refresh.
    private const int StunRefreshTicks = 5;

    protected const float TextMoteFadeDelay = 3.65f;

    private int settingsGeneration = -1;

    public override float GetRangeForPawn() => UnlimitedRange;

    public override bool CanAutoCast => false;

    public override bool AutoCast => false;

    bool ITargetingSource.MultiSelect => true;

    public override bool HidePawnTooltips => true;

    // The job's check toil, with the pilot in reach: the patient is still valid and the
    // ability can still be paid for. Nothing has been spent yet; a false ends the job with a
    // message where one is due.
    public abstract bool CheckBeforeWarmup(Pawn patient);

    // The Targeter's re-entry after a Shift-click, which skips VEF's DoTargeting and would
    // otherwise write currentTargets[-1] through the base.
    public override void OrderForceTarget(LocalTargetInfo target) => CreateCastJob(target);

    // Vanilla's order semantics in place of VEF's StartAbilityJob (see the header). The defs
    // have no AbilityExtension_AbilityMods and the pilot is never a caravan member while the
    // gizmo shows, so skipping the base's Valid, PreCast and caravan branches loses nothing.
    public override void CreateCastJob(params GlobalTargetInfo[] targets)
    {
        currentTargetingIndex = -1;
        currentTargets = new GlobalTargetInfo[def.targetCount];
        if (targets.Length == 0 || targets[0].Thing is not Pawn target || pawn.jobs == null)
            return;
        Job job = JobMaker.MakeJob(def.jobDef, target);
        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }

    // Vanilla's cursor rule: the no-shoot cross over anything the ability cannot be ordered
    // on, else the def's icon through the base. The vanilla Targeter hands a pawn under the
    // mouse through without its hit test, so the validity is decided here.
    protected override Texture2D MouseAttachment(GlobalTargetInfo target)
    {
        if (target.Thing is not Pawn patient || !ValidateTarget(patient, showMessages: false))
            return TexCommand.CannotShoot;
        return base.MouseAttachment(target);
    }

    public override Gizmo GetGizmo()
    {
        ReconcileSettings();
        return base.GetGizmo();
    }

    // A running cooldown is an end tick, so a lowered cooldown setting would not shorten it
    // and VEF's cooldown overlay would overflow the button; cap it at the new length. Once per
    // settings write.
    protected void ReconcileSettings()
    {
        if (settingsGeneration == MedicWarcasketSettings.Generation)
            return;
        settingsGeneration = MedicWarcasketSettings.Generation;
        int ceiling = Find.TickManager.TicksGame + GetCooldownForPawn();
        if (cooldown > ceiling)
            cooldown = ceiling;
    }

    // Within the def's distanceToTarget with a clear line, the walk's own stopping rule; the
    // pilot is always within reach of themself.
    public bool InReach(Pawn patient) =>
        pawn.Spawned && patient.Spawned && patient.Map == pawn.Map
        && (patient == pawn
            || (pawn.Position.DistanceTo(patient.Position) <= def.distanceToTarget
                && GenSight.LineOfSight(pawn.Position, patient.Position, pawn.Map, skipFirstCell: true)));

    protected void RefreshStun(Pawn patient)
    {
        if (patient == pawn || patient.Downed || patient.Dead)
            return;
        patient.stances?.stunner?.StunFor(StunRefreshTicks, pawn, addBattleLog: false, showMote: false);
    }

    protected static void ThrowText(Pawn patient, string text) =>
        MoteMaker.ThrowText(patient.DrawPos, patient.Map, text, Color.white, TextMoteFadeDelay);
}
