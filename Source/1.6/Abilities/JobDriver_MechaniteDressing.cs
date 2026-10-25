using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedicWarcasket;

// The mechanite dressing's job, declared by MDWC_MechaniteDressing's driverClass
// (JobDefs/MechaniteDressing.xml) and named by the ability def's jobDef: the shared walk,
// cooldown wait, check, warmup and cast (JobDriver_MedicCast), then the beam, the shot and the
// warmdown. The ability (Ability_MechaniteDressing) holds the dressing's state; the toils here
// only sequence it.
//
// The beam and warmdown toils re-assert playerForced, which VEF's cast toil drops for a drafted
// pilot, so the melee-threat and hostility-response constant jobs stand down for them as well
// (the JobDef's checkOverrideOnDamage Never covers damage).
//
// The shot toil is the adrenaline shot's autocast (Docs/Specs/Adrenaline-Shot.md 3.4): with
// the shoulders' toggle on and a shot the patient can take, the pilot holds them a jab longer
// and gives it, before the warmdown so the hold the stun gives covers it and the glow's fade
// stays last. The ability decides whether there is a shot to give (ShotToFollow) at the toil's
// start; a dressing without one passes straight through.
public class JobDriver_MechaniteDressing : JobDriver_MedicCast
{
    private Ability_MechaniteDressing Dressing => Casting as Ability_MechaniteDressing;

    protected override IEnumerable<Toil> PostCastToils()
    {
        yield return Beam();
        yield return Shot();
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

    // The shot's warmup, ticksLeftThisToil (scribed) counting down the shot def's castTime,
    // then the shot; skipped at once when there is none to give.
    private Toil Shot()
    {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            job.playerForced = true;
            Ability_AdrenalineShot shot = Dressing?.ShotToFollow();
            if (shot == null)
            {
                ReadyForNextToil();
                return;
            }
            ticksLeftThisToil = shot.GetCastTimeForPawn();
        };
        toil.tickAction = () =>
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
            if (Dressing == null)
            {
                ReadyForNextToil();
                return;
            }
            Dressing.ShotTick();
            if (--ticksLeftThisToil > 0)
                return;
            Dressing.GiveShot();
            ReadyForNextToil();
        };
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
