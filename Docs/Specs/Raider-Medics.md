# SPEC: Raider medics (the set's actives in AI hands)

Status: DRAFT 2026-11-01, not scheduled; section 4 lists the decisions to take before any
implementation starts. Written against the installed RimWorld 1.7.4995 rev309
(`Assembly-CSharp.dll` via `ilspycmd`, Core XML), the VEF checkout at
`../VanillaExpanded/VanillaExpandedFramework/` (58c884b9, 2026-10-21), the VFEP checkout's
`1.7/Defs/`, and the sibling `../ToxinWarcasket/` (`Source/1.6/Comps/CompToxTank.cs` and
`CompJetNozzle.cs`), whose raider auto-vent and auto-jet are the family's AI precedent. Every
engine fact below was read on 1.7 and is taken to hold on 1.6; the author verifies by hand.

The set already sits on VFEP's raid tables (`Set-Tuning.md` 2.7): a raider in it is a
Siegebreaker with the baseline shield and a full medicine suite nobody uses. This spec is the
missing half, the two actives cast by an AI wearer on its own side. Each active is wired on
its own, because VFEP's tag-driven apparel generation hands a raider pieces one at a time and
a raider with the armor but not the shoulders, or the reverse, is the common case, not the
exception.

## 0. The brief

| Request                                                                              | Section |
| ------------------------------------------------------------------------------------ | ------- |
| A raider in the Medic armor dresses downed pawns of its own side                     | 3.2     |
| A raider in the Medic shoulders gives its downed side the shot                       | 3.3     |
| Armor and shoulders work alone; together the shot follows the dressing as for the pilot | 3.1, 3.3 |
| Toxin's comp-tick AI, not a think-tree patch                                         | 2.4, 3.1 |
| Thematic over balanced: a medic behaves like a medic whatever it costs the raid     | 1, 4    |
| Nothing the player sees changes for a colonist pilot                                 | 3.5     |

## 1. Fiction

A combat medic goes where the wounded are. On the raid's side that means a Veteran or a
Major (section 2.1) in the set stepping out of the firing line to kneel by a downed comrade,
stream the mechanites and, if the shoulders are worn, jab them back onto their feet. Whether
that helps the raid is beside the point of the fiction: a dressing that stops a bleed leaves
one more live body for the colony to capture, and a shot that stands a raider up may send
them walking off the map (section 2.3). Both are what a medic would do. The balance question
is real and is left open on purpose (section 4), to be settled when this is scheduled.

## 2. Verified engine facts

### 2.1 Who wears the set in a raid

The armor's tags are `WarcasketVeteran` and `WarcasketAll`
(`1.6/Defs/ThingDefs_Misc/Warcasket_Medic.xml`). `WarcasketVeteran` is listed by the
Junkers' `VFEP_Veteran` (combatPower 400) and the Mercenaries' `VFEP_Major` (300)
(`PawnKindDefs_Humanlikes/PawnKinds_Junkers.xml`, `PawnKinds_Mercenaries.xml`) and by
Odyssey's salvager warcasket kind. The pool behind the tag holds every 10th-generation VFEP
set and the warcasket weapons of the Vanilla Weapons Expanded compat roots, so a Medic piece
is one draw among many and the three pieces are drawn separately.

Both reloadables start full: vanilla's `CompApparelReloadable` initialises at `MaxCharges`,
so a raider's suite holds ten kits and its injector its full count of shots.

### 2.2 Why AI never casts today

VEF's single AI consumer of abilities is its postfix on `Pawn.TryGetAttackVerb`
(`Source/VEF/Abilities/Harmony/Pawn_TryGetAttackVerb.cs`): it reads
`CompAbilities.LearnedAbilities` only, never an apparel comp's `GivenAbilities`, and it
offers an ability only as an attack verb. Vanilla's raid duties cast vanilla
`RimWorld.Ability`s through `JobGiver_AICastAbility` in the `Abilities_Aggressive` subtree
(`ThinkTreeDefs/SubTrees_Ability.xml`, listed in the `AssaultColony` duty of
`DutyDefs/Duties_Misc.xml`); a VEF ability is a different type and that giver cannot see it.

Nor do hostiles tend at all: the Humanlike tree's self-tend node
(`ThinkNode_ConditionalNPCCanSelfTendNow`) returns false for any faction hostile to the
player. A raider medic is therefore the only tending a raid would ever do.

### 2.3 What going down does to a raider

`Pawn_HealthTracker.MakeDowned` calls `Lord.Notify_PawnLost(pawn, Incapped)`.
`LordJob.ShouldRemovePawn` returns true and `LordJob_AssaultColony` does not override it, so
the lord removes the pawn and increments `numPawnsLostViolently`, the counter behind the
fraction-lost flee trigger. A downed raider is lordless from that tick.

`MakeUndowned` calls `pawn.GetLord()?.Notify_PawnUndowned`, which finds no lord for a pawn
removed on downing. A revived raider therefore runs the Humanlike main tree without a duty,
whose final non-colonist fallback is `JobGiver_ExitMapBest`; Core's own comment on that node
names "pawns who are downed during combat, then later self-heal". They walk off the map.

`Lord.AddPawn(p)` is public and calls `UpdateAllDuties`, so a revived raider can be handed
back to the medic's lord and resume the assault; `Lord.CanAddPawn` (the current toil's check)
must be asked first, since `AddPawn` logs an error otherwise. The loss counter is not
decremented by an add.

### 2.4 Starting a job for an AI pawn

`Pawn_JobTracker.TryTakeOrderedJob`, the path the pilot's click takes through
`Ability_MedicTargeted.CreateCastJob`, works for any pawn: it sets `job.playerForced`,
checks the current job is interruptible and starts the new one. It also reads
`KeyBindingDefOf.QueueOrder.IsDownEvent` from the live keyboard, so a raider ordered while
the player holds Shift would queue instead of start. The AI path sets `playerForced` itself
and calls `StartJob(job, JobCondition.InterruptForced)`.

`playerForced` is what keeps the job running: `JobGiver_ReactToCloseMeleeThreat` returns
null when `PawnUtility.PlayerForcedJobNowOrSoon` is true, and the beam and warmdown toils
already re-assert the flag (`JobDriver_MechaniteDressing`). The constant think tree runs
every 30 ticks (`JobTrackerTick`) and the lord's duties are re-evaluated on toil change
through `UpdateAllDuties`; the forced flag covers both as it does for a drafted pilot.

Toxin's precedent (`CompToxTank.AIVentCheck`, `CompJetNozzle.AIJetCheck`): a check on the
comp's tick every `aiCheckInterval` ticks, gated on `!IsColonistPlayerControlled`, `Spawned`,
not `Downed`, `Awake()` and not `InMentalState`, with the trigger numbers as
`CompProperties` fields rather than mod settings.

### 2.5 What the actives already do for a non-player pilot

- `Ability_MechaniteDressing.ValidateTarget` excludes a conscious pawn hostile to the
  pilot; a downed faction mate is neither, so it passes. `Ability_AdrenalineShot.ValidateTarget`
  excludes any pawn hostile to the pilot and any patient already at the top level; a downed
  faction mate passes.
- The stun is skipped for a downed patient (`Ability_MedicTargeted.RefreshStun`).
- `Ability_MechaniteDressing.EndCastJob` already guards the guest counter and the quest
  signal on `pawn.Faction == Faction.OfPlayer`; the records, the bond roll and the Ideology
  role hook run for any pilot, as vanilla's `TendUtility.DoTend` would.
- `ShotToFollow` reads the shot's `AutoCast`, which for a raider is the def's
  `autocastPlayerDefault` (on) since the scribed flip starts false: a raider with both pieces
  already gives the shot after the dressing with no extra wiring.
- Three messages reach the player's log for any pilot: `MDWC_NoMedicineLoaded` and
  `MDWC_NoShotsLoaded` in the two `CheckBeforeWarmup`s, and the hostile and already-rushing
  rejects in `ValidateTarget` when called with messages on. The AI chooses only targets that
  pass, so these are rare, but each needs a faction guard.
- The dressing's motes (`MDWC_WoundsTendedPartial` and the per-wound text) and the glow,
  beam and overlay draw for any pilot. They stay: the player should see the medic at work.
- `Gizmo_DressingSuite` is already not draggable for a non-player wearer
  (`Mechanite-Dressing.md` 6, test 2).

## 3. Design

### 3.1 Two checks, one idiom

Each piece gets its own AI check on its reloadable's tick, Toxin's shape: the armor's
`CompDressingSuite` decides the dressing, the shoulders' injector comp decides the shot. A
raider with both runs both checks, and the dressing's shot toil fires the shot for a patient
it has just dressed exactly as it does for the pilot, so the shoulders' own check mostly finds
nothing left to do; the two never need to know about each other. The shoulders' reloadable is
vanilla's `CompApparelReloadable` today; the check needs a comp of ours on the shoulders
(a `ThingComp` beside it, or a subclass that keeps the vanilla props), to be chosen at
implementation.

Trigger numbers are `CompProperties` fields, as Toxin's are, with no mod setting: an
`aiCheckInterval` (Toxin's 60), a search radius for patients, and a quiet window (3.4).

### 3.2 The dressing's check (armor)

Gates, cheapest first: the wearer is not `IsColonistPlayerControlled`, is spawned, up,
awake, not in a mental state and not already running one of our jobs; the suite holds at
least one kit; the wearer's lord, if any, is not on an exit toil (`LordToil_ExitMap` or its
kin: the medic leaves with the raid); the quiet window holds (3.4).

Patient: the nearest pawn within the search radius that is of the wearer's faction, downed,
spawned, not dead, reachable, and has tendable hediffs the dressing would take
(`FillTendable` is the rule, helmet included). Nearest first, bleeding first if two are
equal, is enough; there is no triage beyond what the beam already does per wound.

Order: `JobMaker.MakeJob(MDWC_MechaniteDressing, patient)`, `playerForced = true`,
`StartJob(..., InterruptForced)`. The job then runs the walk, warmup, beam, shot and
warmdown unchanged.

### 3.3 The shot's check (shoulders)

Same gates, with the injector's count in place of the suite's. Patient: the nearest downed
faction mate the shot validates for (`ValidateTarget`, messages off), which already rules
out a patient at the top level. The job is `MDWC_AdrenalineShot` by the same order path.

After `Apply`, if the patient stands (`!Downed` on the next check, or at once if the hediff's
pain factor lifts them in `Apply`), the re-add: if the medic has a lord and
`lord.CanAddPawn(patient)`, `lord.AddPawn(patient)`. Done from the shot's `Apply` for a
non-player pilot, so the dressing's shot toil gets it too. Whether to re-add at all is
decision 4.2.

### 3.4 Not while fighting

A medic under fire keeps shooting. Vanilla's self-tend node refuses within 300 ticks of
`mindState.lastHarmTick` and `lastEngageTargetTick`; the checks borrow both windows as the
quiet window, and skip while `mindState.meleeThreat` is set. A hostile within the dressing's
reach of the patient is not checked: the beam's own interruption rules apply once the job
runs, as for the pilot.

### 3.5 The pilot is untouched

Every gate above starts with `!IsColonistPlayerControlled`, so a colonist pilot never
reaches a check. A colonist who loses player control (a mental break, a prisoner in the set)
is handed to the checks as Toxin hands its vent, and tends its own side by the same rules.

## 4. Decisions to take first

1. **Dressing alone.** A raider with the armor but no shoulders stops bleeds and leaves more
   captives. Thematic, and a gift to the player. Ship it, or gate the dressing's check on
   the shoulders being worn so only a reviving medic acts? The brief leans to shipping it.
2. **Walk-off.** A shot without a re-add stands a raider up to walk off the map. Thematic
   again (a medic sends the wounded to the rear), and harmless. With the re-add the raid
   keeps its numbers and the flee counter still ticks. Pick one; 3.3 drafts the re-add.
3. **Stage.** Should the AI shot aim lower than the top level, a cheaper jab that lifts a
   downed raider without the full rush? `Apply` sets full severity today; a second entry
   point is a small change and keeps the pilot's shot as it is.
4. **Scope of "own side".** Faction mates only, or anyone not hostile to the medic (a raid's
   allied faction in a joint raid, an animal of the raid)? Faction only is the draft.

## 5. Localization surface

None: no new player-facing text. The three guarded messages keep their keys.

## 6. Verification

1. Spawn a Veteran raid with dev mode, give one raider the armor and ten kits, down another
   near them: the medic walks over, the glow and beam show, the wounds tend, no message
   appears in the player's log, and the medic returns to the assault.
2. Give the medic the shoulders too: the shot follows the beam, the patient stands, and
   (with the re-add) rejoins the assault rather than walking to the map edge.
3. Shoulders only: the raider gives the shot and nothing else.
4. Down a raider while the medic is being shot at: no dressing until 300 ticks after the
   last hit. Trigger the flee: the medic leaves with the raid, patient or not.
5. A colonist pilot: no behaviour change, gizmos and autocast as before. Draft a colonist,
   let a tantrum break: they dress a downed colonist on their own.

## 7. Implementation checklist

- [ ] Decisions 4.1 to 4.4 taken and recorded here.
- [ ] `CompDressingSuite`: the AI check, props on `CompProperties_DressingSuite`.
- [ ] Shoulders: a comp of ours for the shot's check.
- [ ] Faction guards on the three messages.
- [ ] The re-add in `Ability_AdrenalineShot.Apply`, if 4.2 says so.
- [ ] Def headers updated (the armor's "AI pawns never cast" line goes); `Set-Tuning.md` 2.7
      and `Field-Dressing.md` "Player-only in practice" pointed here.
- [ ] `Tests/1.6/`: the gates as pure functions where they can be.
