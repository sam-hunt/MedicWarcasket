using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace MedicWarcasket;

// The job behind each of the set's actives (Ability_MedicTargeted): the walk, a wait for a
// running cooldown, the check, then VEF's warmup and cast toils (JobDriver_CastAbilityOnce: a
// progress-bar wait of GetCastTimeForPawn ticks that calls the ability's PreWarmupAction and
// WarmupToil, then an instant cast toil), then whatever a subclass adds after the cast
// (PostCastToils: the dressing's beam, shot and warmdown; nothing for the shot). VEF's driver
// finish action calls the ability's EndCastJob on every job end.
//
// Which ability: the pilot's worn apparel's GivenAbilities entry whose def names this job as
// its jobDef (FindAbility), so each active has its own JobDef and the driver needs no
// per-ability DefOf. GetReport reads that def directly, since vanilla reports queued jobs
// through the cached driver too, before any binding.
//
// The binding: VEF's driver reads CompAbilities.currentlyCasting and currentlyCastingTargets
// throughout, which VEF's own path writes at order time. These jobs are ordered through
// TryTakeOrderedJob so they can be queued, and a queued job started against order-time comp
// state would run whatever ability was bound last. So the binding happens here, in
// TryMakePreToilReservations, and only when the job is starting (StartJob sets CurJob before
// calling it): vanilla also calls this at ORDER time through the job's cached driver, while the
// pilot may be mid-cast of another VEF ability whose cast toil and finish action read the comp,
// so at order time it returns true without touching the comp or calling the base, which would
// dereference a possibly null currentlyCasting. A job that starts with the piece or the ability
// gone returns false, which ends a queued job quietly (QueuedNoLongerValid).
//
// The walk is modelled on VEF's JobDriver_GotoTargetAndCastAbilityOnce with one thing left
// out: that driver orders a pawn target to stand and face the caster, which pulls a patient out
// of bed (losing vanilla's bed tend-quality offset) and makes a downed patient take a job. Here
// the patient is left to whatever they were doing; the ability's stun holds a conscious one
// still. The pilot paths to touch the patient and stops as soon as they stand within the def's
// distanceToTarget with line of sight, skipping their own cell, and are not mid-door. A patient
// the pilot cannot reach ends the job rather than repathing every tick. A self-target skips
// the walk.
//
// Interruption: the base driver's FailOnDespawnedOrNull covers every toil (death leaves a
// corpse, which is a despawn; being carried is one too). The JobDefs' checkOverrideOnDamage
// Never keeps damage from interrupting any phase whatever playerForced says. A player order, a
// draft change, a mental break or the pilot going down still interrupt.
public abstract class JobDriver_MedicCast : JobDriver_CastAbilityOnce
{
    private AbilityDef abilityDef;

    protected Ability_MedicTargeted Casting => CompAbilities?.currentlyCasting as Ability_MedicTargeted;

    protected Pawn Patient => TargetA.Pawn;

    // The def of the ability this job casts, found by its jobDef; cached per driver instance,
    // which vanilla rebuilds on load.
    protected AbilityDef Def => abilityDef ??= FindAbility(pawn, job.def)?.def;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.CurJob != job)
            return true;
        Ability_MedicTargeted ability = FindAbility(pawn, job.def);
        CompAbilities comp = CompAbilities;
        if (ability == null || comp == null || Patient == null)
            return false;
        ability.pawn = pawn;
        comp.currentlyCasting = ability;
        comp.currentlyCastingTargets = [new RimWorld.Planet.GlobalTargetInfo(Patient)];
        return base.TryMakePreToilReservations(errorOnFailed);
    }

    // The pilot's instance of the ability whose def names jobDef, among the worn apparel's
    // granted abilities.
    public static Ability_MedicTargeted FindAbility(Pawn pilot, JobDef jobDef)
    {
        if (pilot?.apparel == null || jobDef == null)
            return null;
        foreach (Apparel apparel in pilot.apparel.WornApparel)
        {
            List<Ability> abilities = apparel.TryGetComp<CompAbilitiesApparel>()?.GivenAbilities;
            if (abilities == null)
                continue;
            for (int i = 0; i < abilities.Count; i++)
                if (abilities[i].def.jobDef == jobDef && abilities[i] is Ability_MedicTargeted ability)
                    return ability;
        }
        return null;
    }

    // The pilot's instance of the ability of the given def, likewise; the pawn is bound so the
    // instance can be asked questions (IsEnabledForPawn, AutoCast) before its first gizmo draw.
    public static Ability_MedicTargeted FindAbility(Pawn pilot, AbilityDef def)
    {
        if (pilot?.apparel == null || def == null)
            return null;
        foreach (Apparel apparel in pilot.apparel.WornApparel)
        {
            List<Ability> abilities = apparel.TryGetComp<CompAbilitiesApparel>()?.GivenAbilities;
            if (abilities == null)
                continue;
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i].def != def || abilities[i] is not Ability_MedicTargeted ability)
                    continue;
                ability.pawn ??= pilot;
                return ability;
            }
        }
        return null;
    }

    public override string GetReport() => Def?.JobReportString ?? base.GetReport();

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return GotoPatient();
        yield return AwaitCooldown();
        yield return Check();
        foreach (Toil toil in base.MakeNewToils())
            yield return toil;
        foreach (Toil toil in PostCastToils())
            yield return toil;
    }

    protected virtual IEnumerable<Toil> PostCastToils()
    {
        yield break;
    }

    private Toil GotoPatient()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () => pawn.pather.StopDead();
        toil.tickAction = () =>
        {
            LocalTargetInfo target = job.targetA;
            if (target.Thing == pawn)
            {
                ReadyForNextToil();
                return;
            }
            IntVec3 cell = target.Cell;
            Map map = pawn.Map;
            pawn.rotationTracker.FaceTarget(cell);
            if (pawn.Position.DistanceTo(cell) <= (Def?.distanceToTarget ?? 0f)
                && GenSight.LineOfSight(pawn.Position, cell, map, skipFirstCell: true)
                && (!pawn.pather.Moving || pawn.pather.nextCell.GetDoor(map) == null))
            {
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(cell);
                ReadyForNextToil();
            }
            else if (!pawn.pather.Moving)
            {
                if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                pawn.pather.StartPath(target, PathEndMode.Touch);
            }
        };
        toil.handlingFacing = true;
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    // Reachable only from the queue, since the gizmo is disabled while the cooldown runs: the
    // pilot waits at the patient until it ends. With a cooldown of 0 it never waits.
    private Toil AwaitCooldown()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () => pawn.pather.StopDead();
        toil.tickAction = () =>
        {
            if (job.targetA.Thing != pawn)
                pawn.rotationTracker.FaceTarget(job.targetA);
            if (Casting == null || Casting.cooldown <= Find.TickManager.TicksGame)
                ReadyForNextToil();
        };
        toil.handlingFacing = true;
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private Toil Check()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            if (Casting == null || !Casting.CheckBeforeWarmup(Patient))
                EndJobWith(JobCondition.Incompletable);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}
