using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;

namespace MedicWarcasket;

// The mechanite dressing's job, declared by MDWC_MechaniteDressing's driverClass
// (JobDefs/MechaniteDressing.xml) and named by the ability def's jobDef. Its toils, in order:
// the walk, a wait for a running cooldown, the check, VEF's warmup and cast toils
// (JobDriver_CastAbilityOnce: a progress-bar wait of GetCastTimeForPawn ticks that calls the
// ability's PreWarmupAction and WarmupToil, then an instant cast toil), the beam and the
// warmdown. The ability (Ability_MechaniteDressing) holds the dressing's state; the toils
// here only sequence it. VEF's driver finish action calls the ability's EndCastJob on every
// job end, which settles whatever the job leaves open.
//
// The binding: VEF's driver reads CompAbilities.currentlyCasting and currentlyCastingTargets
// throughout, which VEF's own path writes at order time. The dressing is ordered through
// TryTakeOrderedJob so it can be queued, and a queued job started against order-time comp
// state would run whatever ability was bound last. So the binding happens here, in
// TryMakePreToilReservations, and only when the job is starting (StartJob sets CurJob before
// calling it): vanilla also calls this at ORDER time through the job's cached driver, while the
// pilot may be mid-dressing or mid-cast of another VEF ability whose cast toil and finish action
// read the comp, so at order time it returns true without touching the comp or calling the base,
// which would dereference a possibly null currentlyCasting. The pilot's dressing instance is
// the worn apparel's GivenAbilities entry whose def is the dressing's; a job that starts with
// the armor or the ability gone returns false, which ends a queued job quietly
// (QueuedNoLongerValid). GetReport reads the def directly, since vanilla reports queued jobs
// through the cached driver too.
//
// The walk is modelled on VEF's JobDriver_GotoTargetAndCastAbilityOnce with one thing left
// out: that driver orders a pawn target to stand and face the caster, which pulls a patient out
// of bed (losing vanilla's bed tend-quality offset) and makes a downed patient take a job. Here
// the patient is left to whatever they were doing; the ability's stun holds a conscious one
// still. The pilot paths to touch the patient and stops as soon as they stand within the def's
// distanceToTarget (the range setting) with line of sight, skipping their own cell, and are
// not mid-door. A patient the pilot cannot reach ends the job rather than repathing every tick.
//
// Interruption: the base driver's FailOnDespawnedOrNull covers every toil (death leaves a
// corpse, which is a despawn; being carried is one too). The JobDef's checkOverrideOnDamage
// Never keeps damage from interrupting any phase whatever playerForced says, and the beam and
// warmdown toils re-assert playerForced, which VEF's cast toil drops for a drafted pilot, so
// the melee-threat and hostility-response constant jobs stand down for them as well. A player
// order, a draft change, a mental break or the pilot going down still interrupt.
public class JobDriver_MechaniteDressing : JobDriver_CastAbilityOnce
{
    private Ability_MechaniteDressing Dressing => CompAbilities?.currentlyCasting as Ability_MechaniteDressing;

    private Pawn Patient => TargetA.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.CurJob != job)
            return true;
        Ability_MechaniteDressing dressing = FindDressing(pawn);
        CompAbilities comp = CompAbilities;
        if (dressing == null || comp == null || Patient == null)
            return false;
        dressing.pawn = pawn;
        comp.currentlyCasting = dressing;
        comp.currentlyCastingTargets = [new RimWorld.Planet.GlobalTargetInfo(Patient)];
        return base.TryMakePreToilReservations(errorOnFailed);
    }

    public static Ability_MechaniteDressing FindDressing(Pawn pilot)
    {
        if (pilot?.apparel == null)
            return null;
        foreach (Apparel apparel in pilot.apparel.WornApparel)
        {
            List<Ability> abilities = apparel.TryGetComp<CompAbilitiesApparel>()?.GivenAbilities;
            if (abilities == null)
                continue;
            for (int i = 0; i < abilities.Count; i++)
                if (abilities[i].def == MDWC_DefOf.MDWC_MechaniteDressing && abilities[i] is Ability_MechaniteDressing dressing)
                    return dressing;
        }
        return null;
    }

    public override string GetReport() => MDWC_DefOf.MDWC_MechaniteDressing.JobReportString;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return GotoPatient();
        yield return AwaitCooldown();
        yield return Check();
        foreach (Toil toil in base.MakeNewToils())
            yield return toil;
        yield return Beam();
        yield return Warmdown();
    }

    private Toil GotoPatient()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () => pawn.pather.StopDead();
        toil.tickAction = () =>
        {
            LocalTargetInfo target = job.targetA;
            IntVec3 cell = target.Cell;
            Map map = pawn.Map;
            pawn.rotationTracker.FaceTarget(cell);
            if (pawn.Position.DistanceTo(cell) <= MDWC_DefOf.MDWC_MechaniteDressing.distanceToTarget
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
    // pilot waits at the patient until it ends. With the default cooldown of 0 it never waits.
    private Toil AwaitCooldown()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () => pawn.pather.StopDead();
        toil.tickAction = () =>
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
            if (Dressing == null || Dressing.cooldown <= Find.TickManager.TicksGame)
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
            if (Dressing == null || !Dressing.CheckBeforeWarmup(Patient))
                EndJobWith(JobCondition.Incompletable);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    // Runs until the ability reports the beam over; a connect that found nothing to tend
    // passes straight through to the warmdown.
    private Toil Beam()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            job.playerForced = true;
            pawn.pather.StopDead();
        };
        toil.tickAction = () =>
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
            if (Dressing == null || Dressing.BeamTick())
                ReadyForNextToil();
        };
        toil.WithProgressBar(TargetIndex.A, () => Dressing?.BeamProgress ?? 0f);
        toil.handlingFacing = true;
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private Toil Warmdown()
    {
        int ticks = MDWC_DefOf.MDWC_MechaniteDressing.GetModExtension<DressingExtension>()?.warmdownTicks ?? 0;
        Toil toil = Toils_General.Wait(ticks, TargetIndex.A);
        toil.AddPreInitAction(() => job.playerForced = true);
        toil.AddPreTickAction(() => Dressing?.WarmdownTick(ticks > 0 ? (float)ticksLeftThisToil / ticks : 0f));
        toil.socialMode = RandomSocialMode.Off;
        return toil;
    }
}
