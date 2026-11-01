using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MedicWarcasket;

// The rules behind a non-player medic's casts (CompMedicAI), as functions over the engine's own
// state so Tests/1.6/MedicAITests can check them without a game: the quiet window, a lord on
// its way out, which side a patient is on, which of two patients to prefer, and the re-add of
// a raider the shot stood up. Design in Docs/Specs/Raider-Medics.md.
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
}
