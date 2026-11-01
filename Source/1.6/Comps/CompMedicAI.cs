using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;

namespace MedicWarcasket;

// A non-player wearer's use of the piece's active: on the armor the mechanite dressing, on the
// shoulders the adrenaline shot, one comp on each beside its reloadable, in the shape of
// Toxin's raider auto-vent (its CompToxTank.AIVentCheck). Every checkInterval ticks, staggered
// by the piece's id, the comp looks for a downed patient on the wearer's side within
// searchRadius and starts the active's own job on them, the job the pilot's click would start,
// so the walk, the warmup, the beam, the shot that follows a dressing and the warmdown run
// unchanged. The two pieces never consult each other: a wearer with both runs both checks, and
// the dressing's shot toil usually leaves the shoulders' check nothing to do. Design in
// Docs/Specs/Raider-Medics.md; the rules it applies are MedicAI's.
//
// The ability is the piece's own, found among the parent's CompAbilitiesApparel.GivenAbilities.
// VEF binds an ability's pawn only when the wearer's gizmos draw, which an AI wearer's never
// do, and only when the wearer differs from the one VEF last drew for; so the check binds it
// for every wearer, a colonist taking the piece back from a raider included, re-running Init on
// the change as VEF would at the next draw, and does so before the player-control gate, since
// that colonist's gizmo would otherwise read the raider's state.
//
// Gates, cheapest first: aiCasts (a mod setting); a worn piece, since a piece on the ground or
// a shelf ticks too (the shoulders def redeclares tickerType Normal for this comp; the armor
// base's is Normal already); not IsColonistPlayerControlled, so a colonist pilot never reaches
// the rest (a mental break or a prisoner in the set does, and tends its own side by the same
// rules); spawned, up, awake, not in a mental state, with VEF's CompAbilities (the job's
// driver binds its cast state there and would otherwise fail on every start); not already in
// one of the set's jobs (JobDriver_MedicCast); the lord not leaving and the quiet window holding (MedicAI); the
// ability enabled for the wearer, which is VEF's cooldown and the piece's own kit or shot
// count (IsEnabledForPawn). A hostile near the patient is not checked: the job's own
// interruption rules apply once it runs, as for the pilot.
//
// The patient: among the map's spawned pawns, downed, alive, not a prisoner (a captured raider
// keeps the raid's faction, and the medic is not here to tend the colony's prisoners), within
// the radius, on the wearer's side (MedicAI.OnSide), valid for the ability with messages off
// (its ValidateTarget: something to tend, or a shot they can take) and reachable to touch; the
// nearer wins, the bleeding one at equal distance (MedicAI.Prefer). The validation and the reach test, a path query, are
// asked last and only of a candidate that would beat the best so far.
//
// The order: the ability's JobDef on the patient, playerForced, started with InterruptForced,
// what Pawn_JobTracker.TryTakeOrderedJob does for the pilot's click less its read of the live
// keyboard (a Shift held by the player would queue a raider's job). playerForced keeps the
// constant think tree's melee-threat giver off (PawnUtility.PlayerForcedJobNowOrSoon), as it
// does for a drafted pilot, and the JobDefs' checkOverrideOnDamage Never keeps a hit from
// re-running the main tree; the assault duty's own constant node, the opportunistic weapon
// pickup, ignores the flag but gives an armed raider nothing. The lord's duties re-evaluate
// on a toil change, and the exit toil interrupts the job as it interrupts any other, so the
// medic leaves with the raid.
public class CompMedicAI : ThingComp
{
    public CompProperties_MedicAI Props => (CompProperties_MedicAI)props;

    public override void CompTick()
    {
        if (!Props.aiCasts || !parent.IsHashIntervalTick(Props.checkInterval))
            return;
        if ((parent as Apparel)?.Wearer is not { } wearer)
            return;
        Ability_MedicTargeted ability = Active(wearer);
        if (ability != null)
            TryCast(wearer, ability);
    }

    private void TryCast(Pawn wearer, Ability_MedicTargeted ability)
    {
        if (wearer.IsColonistPlayerControlled || !wearer.Spawned || wearer.Dead || wearer.Downed
            || !wearer.Awake() || wearer.InMentalState || wearer.jobs == null || wearer.jobs.curDriver is JobDriver_MedicCast
            || wearer.GetComp<CompAbilities>() == null)
            return;
        if (MedicAI.Leaving(wearer.GetLord()?.CurLordToil) || !MedicAI.Quiet(wearer.mindState, Props.quietTicks))
            return;
        if (!ability.IsEnabledForPawn(out _))
            return;
        Pawn patient = FindPatient(wearer, ability);
        if (patient == null)
            return;
        Job job = JobMaker.MakeJob(ability.def.jobDef, patient);
        job.playerForced = true;
        wearer.jobs.StartJob(job, JobCondition.InterruptForced);
    }

    // The piece's own active, bound to its wearer.
    private Ability_MedicTargeted Active(Pawn wearer)
    {
        List<Ability> abilities = parent.TryGetComp<CompAbilitiesApparel>()?.GivenAbilities;
        if (abilities == null)
            return null;
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] is not Ability_MedicTargeted ability)
                continue;
            if (ability.pawn != wearer)
            {
                ability.pawn = wearer;
                ability.Init();
            }
            return ability;
        }
        return null;
    }

    private Pawn FindPatient(Pawn wearer, Ability_MedicTargeted ability)
    {
        float radiusSq = Props.searchRadius * Props.searchRadius;
        Pawn best = null;
        int bestDistSq = int.MaxValue;
        bool bestBleeding = false;
        IReadOnlyList<Pawn> pawns = wearer.Map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn candidate = pawns[i];
            if (candidate == wearer || !candidate.Downed || candidate.Dead || candidate.IsPrisoner)
                continue;
            int distSq = (candidate.Position - wearer.Position).LengthHorizontalSquared;
            if (distSq > radiusSq || !MedicAI.OnSide(wearer.Faction, candidate.Faction))
                continue;
            bool bleeding = candidate.health.hediffSet.BleedRateTotal > 0f;
            if (!MedicAI.Prefer(distSq, bleeding, bestDistSq, bestBleeding))
                continue;
            if (!ability.ValidateTarget(candidate, showMessages: false)
                || !wearer.CanReach(candidate, PathEndMode.Touch, Danger.Deadly))
                continue;
            best = candidate;
            bestDistSq = distSq;
            bestBleeding = bleeding;
        }
        return best;
    }
}
