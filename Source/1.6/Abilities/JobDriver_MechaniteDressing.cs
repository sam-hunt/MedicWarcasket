using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using VEF.Abilities;

namespace MedicWarcasket;

// The field dressing's cast job: walk to the patient, then VEF's wait-and-cast toils
// (JobDriver_CastAbilityOnce: a progress-bar wait of GetCastTimeForPawn ticks that calls the
// ability's PreWarmupAction and WarmupToil, then an instant cast toil). Modelled on VEF's
// JobDriver_GotoTargetAndCastAbilityOnce, with one thing left out: that driver orders a pawn
// target to stand and face the caster (VFEA_StandAndFaceTarget), which pulls a patient out of
// bed, so the cast then finds no bed and loses vanilla's bed tend-quality offset, and makes a
// downed patient take a job. Here the patient is left to whatever they were doing; the
// ability's stun (Ability_MechaniteDressing.PreWarmupAction) holds a conscious one still for the
// cast, and a bed patient stays in bed.
//
// The walk: the pilot paths to touch the patient and the cast starts as soon as they stand
// within the def's distanceToTarget (the range setting) with line of sight, skipping their own
// cell, and are not mid-door. A patient the pilot cannot reach ends the job rather than
// repathing every tick. The target despawning (death leaves a corpse, which is a despawn)
// fails the job through the base driver; a patient going down does not.
//
// Declared by MDWC_MechaniteDressing's driverClass (JobDefs/MechaniteDressing.xml) and named by the
// ability def's jobDef. The base driver sets playerForced for the walk, so damage taken on the
// way does not interrupt it (Pawn_JobTracker.Notify_DamageTaken); a player order, a draft, a
// mental break or being downed still does, and nothing is spent until the cast toil runs.
public class JobDriver_MechaniteDressing : JobDriver_CastAbilityOnce
{
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        if (pawn != TargetA.Thing)
            yield return GotoPatient();
        foreach (Toil toil in base.MakeNewToils())
            yield return toil;
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
            if (pawn.Position.DistanceTo(cell) <= CompAbilities.currentlyCasting.def.distanceToTarget
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
}
