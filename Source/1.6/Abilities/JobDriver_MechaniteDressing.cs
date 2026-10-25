using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedicWarcasket;

// The mechanite dressing's job, declared by MDWC_MechaniteDressing's driverClass
// (JobDefs/MechaniteDressing.xml) and named by the ability def's jobDef: the shared walk,
// cooldown wait, check, warmup and cast (JobDriver_MedicCast), then the beam and the warmdown. The ability (Ability_MechaniteDressing) holds the dressing's state; the toils here
// only sequence it.
//
// The beam and warmdown toils re-assert playerForced, which VEF's cast toil drops for a drafted
// pilot, so the melee-threat and hostility-response constant jobs stand down for them as well
// (the JobDef's checkOverrideOnDamage Never covers damage).
public class JobDriver_MechaniteDressing : JobDriver_MedicCast
{
    private Ability_MechaniteDressing Dressing => Casting as Ability_MechaniteDressing;

    protected override IEnumerable<Toil> PostCastToils()
    {
        yield return Beam();
        yield return Warmdown();
    }

    // Runs until the ability reports the beam over; a connect that found nothing to tend
    // passes straight through.
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
        int ticks = Def?.GetModExtension<DressingExtension>()?.warmdownTicks ?? 0;
        Toil toil = Toils_General.Wait(ticks, TargetIndex.A);
        toil.AddPreInitAction(() => job.playerForced = true);
        toil.AddPreTickAction(() => Dressing?.WarmdownTick(ticks > 0 ? (float)ticksLeftThisToil / ticks : 0f));
        toil.socialMode = RandomSocialMode.Off;
        return toil;
    }
}
