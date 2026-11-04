using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MedicWarcasket;

// The rules behind a non-player medic's casts (CompMedicAI), as functions over the engine's own
// state so Tests/1.6/MedicAITests can check them without a game: the quiet window, a lord on
// its way out, which side a patient is on, which of two patients to prefer, and the re-add
// and rearming of a raider the shot stood up. Design in Docs/Specs/Raider-Medics.md.
//
// Quiet: vanilla's self-tend node (ThinkNode_ConditionalNPCCanSelfTendNow) refuses within 300
// ticks of mindState.lastHarmTick and of lastEngageTargetTick; a medic borrows both windows,
// and stands down while mindState.meleeThreat is set, the state JobGiver_ReactToCloseMeleeThreat
// acts on.
//
// Leaving: a lord toil that forbids self-tending (LordToil.AllowSelfTend false) is one whose
// pawns are on their way out or in flight: LordToil_ExitMap and what derives from it (the
// assault's flee), ExitMapNear, ExitMapTraderFighting, ExitMapAndEscortCarriers, PanicFlee,
// StealCover, TakeWoundedGuest and PrepareCaravan_Leave. LordToil_ExitMapRandom and
// LordToil_ExitMapAndDefendSelf leave the flag at its default and are named here; no raid lord
// job uses either (visitors, beggars and the xenogerm reimplant do). A lordless medic is never
// leaving.
//
// OnSide: the patient's faction is the wearer's, or one the wearer's faction holds as an ally
// (FactionRelationKind.Ally, goodwill 75 and up). The same-faction test goes first, since
// Faction.RelationWith logs an error when asked about itself, and a pair with no relation (it
// logs there too, unless allowed) is no side; nor is a pawn without a faction, a wild animal,
// or a medic without one.
//
// Prefer: the nearer patient; at equal distance the bleeding one.
//
// TryRejoin: a patient the shot stood up, of the medic's own faction, nobody's prisoner or
// guest (HostFaction), on its map and without a lord, joins the medic's lord when its current toil allows (Lord.CanAddPawn, which AddPawn
// logs an error without, as it does for a pawn that already has a lord). The toil's
// UpdateAllDuties then hands them the lord's current duty, and the assault toil and the exit
// toil both interrupt the job the revival started (an exit at Walk urgency with no avoid grid,
// JobGiver_ExitMapBest's, the lordless fallback). The patient has no lord to begin with because
// MakeDowned's Notify_PawnLost removed them from it and MakeUndowned finds none to notify. An
// allied patient, a colonist above all, is never added. Verified on 1.7.
//
// TryRearm: the same patient, of the medic's own faction and unarmed, goes for a weapon before
// taking up the duty. MakeDowned dropped and forbade everything with rememberPrimary, which
// keeps the primary in mindState.droppedWeapon (cleared when the weapon or the pawn despawns
// and by the Equip job itself), but the node that reclaims it, JobGiver_PickupDroppedWeapon,
// sits under ThinkNode_ConditionalColonist in the Humanlike tree, and the raid duties' own
// JobGiver_PickUpOpportunisticWeapon is gated on PawnDuty.pickupOpportunisticWeapon, which only
// the refugee betrayal sets; a revived raider would fight bare-handed. The remembered weapon
// comes first when it is still spawned on the map and takeable: reservable and reachable
// (CanReserveAndReach, Touch, Danger.Deadly, as PickupWeaponJob asks), not burning, and
// EquipmentUtility.CanEquip, where VEF's heavy-weapon postfix lets a warcasket torso pass a
// warcasket weapon. Otherwise the nearest takeable weapon within RearmRadius cells, the radius
// JobGiver_PickUpOpportunisticWeapon searches, covers a weapon destroyed or carried off. The
// job is vanilla's own, JobGiver_PickupDroppedWeapon.PickupWeaponJob with ignoreForbidden,
// since the dropped gear is forbidden and JobDriver_Equip fails on a forbidden target
// otherwise; it is the job Hediff_DeathRefusal starts for a non-player pawn it resurrected.
// Started after the re-add, with InterruptForced: Lord.AddPawn runs the toil's
// UpdateAllDuties, and the assault toil ends the current job and starts the duty's at once,
// so a job started before it is thrown away, while one started after runs out and the duty's
// think tree resumes when it ends. A pawn with a primary, or unable to manipulate, or with
// violent work disabled, is left alone. Verified on 1.7; the signatures are the same on 1.6.
public static class MedicAI
{
    public static bool Quiet(int ticksGame, int lastHarmTick, int lastEngageTargetTick, int quietTicks) =>
        ticksGame >= lastHarmTick + quietTicks && ticksGame >= lastEngageTargetTick + quietTicks;

    public static bool Quiet(Pawn_MindState mind, int quietTicks) =>
        mind == null
        || (mind.meleeThreat == null && Quiet(Find.TickManager.TicksGame, mind.lastHarmTick, mind.lastEngageTargetTick, quietTicks));

    public static bool Leaving(LordToil toil) =>
        toil != null && (!toil.AllowSelfTend || toil is LordToil_ExitMapRandom || toil is LordToil_ExitMapAndDefendSelf);

    public static bool OnSide(Faction wearer, Faction patient) =>
        wearer != null && patient != null
        && (patient == wearer || wearer.RelationWith(patient, allowNull: true)?.kind == FactionRelationKind.Ally);

    public static bool Prefer(int distSq, bool bleeding, int bestDistSq, bool bestBleeding) =>
        distSq < bestDistSq || (distSq == bestDistSq && bleeding && !bestBleeding);

    public static bool TryRejoin(Pawn medic, Pawn patient)
    {
        if (medic == null || patient == null || patient.Dead || patient.Downed || !patient.Spawned)
            return false;
        if (patient.Faction == null || patient.Faction != medic.Faction || patient.HostFaction != null || patient.GetLord() != null)
            return false;
        Lord lord = medic.GetLord();
        if (lord == null || lord.Map != patient.Map || !lord.CanAddPawn(patient))
            return false;
        lord.AddPawn(patient);
        return true;
    }

    public const float RearmRadius = 8f;

    public static bool TryRearm(Pawn medic, Pawn patient)
    {
        if (medic == null || patient == null || patient.Dead || patient.Downed || !patient.Spawned)
            return false;
        if (patient.Faction == null || patient.Faction != medic.Faction || patient.jobs == null)
            return false;
        if (patient.equipment == null || patient.equipment.Primary != null)
            return false;
        if (!patient.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || patient.WorkTagIsDisabled(WorkTags.Violent))
            return false;
        Thing weapon = RememberedWeapon(patient) ?? NearbyWeapon(patient);
        if (weapon == null)
            return false;
        Job job = JobGiver_PickupDroppedWeapon.PickupWeaponJob(patient, weapon, ignoreForbidden: true);
        if (job == null)
            return false;
        patient.jobs.StartJob(job, JobCondition.InterruptForced);
        return true;
    }

    private static Thing RememberedWeapon(Pawn pawn)
    {
        Thing weapon = pawn.mindState?.droppedWeapon;
        if (weapon == null || weapon.Destroyed || !weapon.Spawned || weapon.Map != pawn.Map || !Takeable(weapon, pawn))
            return null;
        return weapon;
    }

    private static Thing NearbyWeapon(Pawn pawn) =>
        GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon),
            PathEndMode.OnCell, TraverseParms.For(pawn), RearmRadius, weapon => Takeable(weapon, pawn));

    private static bool Takeable(Thing weapon, Pawn pawn) =>
        !weapon.IsBurning()
        && pawn.CanReserveAndReach(weapon, PathEndMode.Touch, Danger.Deadly)
        && EquipmentUtility.CanEquip(weapon, pawn);
}
