# SPEC: Raider medics (the set's actives in AI hands)

> Implemented. The code, its def headers and its tests are the tested current state and supersede this spec wherever the two differ.

Status: IMPLEMENTED 2026-11-01; section 4 records the decisions, section 7 what landed. Written
against the installed RimWorld 1.7.4995 rev309
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
| A raider in the Medic armor dresses downed pawns of its own side or an allied one    | 3.2     |
| A raider in the Medic shoulders gives its downed side, or an ally, the shot          | 3.3     |
| Armor and shoulders work alone; together the shot follows the dressing as for the pilot | 3.1, 3.3 |
| Toxin's comp-tick AI, not a think-tree patch                                         | 2.4, 3.1 |
| Thematic over balanced: a medic behaves like a medic whatever it costs the raid     | 1, 4    |
| Nothing the player sees changes for a colonist pilot                                 | 3.5     |

## 1. Fiction

A combat medic goes where the wounded are. On the raid's side that means a Veteran or a
Major (section 2.1) in the set stepping out of the firing line to kneel by a downed comrade,
stream the mechanites and, if the shoulders are worn, jab them back onto their feet. Whether
that helps the raid is beside the point of the fiction: a dressing that stops a bleed leaves
one more live body for the colony to capture, and a shot that stands a raider up hands them
back to the raid (section 2.3). Both are what a medic would do. The same medic on an allied
faction's side, defenders sent to the colony's aid, kneels by a downed colonist as readily:
vanilla both lets and rewards a player tending another faction's pawns, and a medic who
returned the favour only to its own is the asymmetry a player would notice. Balance is
second to that (section 4): the plain armor already trails VFEP's spacer torsos on stats
(`Set-Tuning.md`), and its passives are what it has.

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
names "pawns who are downed during combat, then later self-heal". They walk off the map, at
`Walk` urgency and without the avoid grid.

On a map that cannot be left (`Map.CanEverExit`: Odyssey's `Space` biome sets `canExitMap`
false; pocket maps) `JobGiver_ExitMap.TryGiveJob` returns null and the tree falls through to
its last node, `JobGiver_WanderAnywhere`: a lordless hostile idles in the colony until shot.
The same flag keeps `Lord` from adding its panic-flee toil there, so a space raid never runs.

`Lord.AddPawn(p)` is public and calls `UpdateAllDuties`, so a revived raider can be handed
back to the medic's lord and takes the lord's current duty, the assault or, on an exit toil,
the raid's exit at its urgency and avoid grid; `Lord.CanAddPawn` (the current toil's check)
must be asked first, and the pawn must have no lord, since `AddPawn` logs an error on either.
The loss counter is not decremented by an add, while `numPawnsEverGained`, the flee
fraction's denominator, goes up by one.

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

Each piece gets its own AI check in its comp's `CompTick`, Toxin's shape: the armor's
`CompDressingSuite` decides the dressing, the shoulders' injector comp decides the shot. A
raider with both runs both checks, and the dressing's shot toil fires the shot for a patient
it has just dressed exactly as it does for the pilot, so the shoulders' own check mostly finds
nothing left to do; the two never need to know about each other. Implemented as one
`ThingComp` of ours, `CompMedicAI`, declared on both pieces beside their reloadables: it finds
the piece's own granted active and runs the check for it, so the armor's suite and the
shoulders' vanilla reloadable are untouched and a third active would get its AI use by the
same entry. The rules the comp applies (the quiet window, the leaving lord, the side, the
preference between patients, the re-add) are `MedicAI`'s static functions, which the tests
reach.

How the comps tick: a worn piece is ticked through its wearer (`Thing.DoTick` walks the
pawn's child holders, the apparel tracker among them, and calls `DoTick` on each held thing),
and `DoTick` runs `Tick`, so `CompTick`, only for a def whose `tickerType` is `Normal`.
VFEP's `VFEP_WarcasketArmorBase` sets `Normal`, so the armor's comps already tick every game
tick (vanilla's `CompApparelReloadable.CompTick` runs there today, for a replenish rule the
suite does not use). `VFEP_WarcasketShoulderPadBase` leaves the part base's `Never`, so the
shoulders tick not at all: the shoulders def redeclares `tickerType Normal`, as Toxin's
shoulders and helmet do for the same reason. The check itself runs once per
`aiCheckInterval` ticks through `IsHashIntervalTick`, staggered by thing id; every other tick
is one early-out. A `Normal` piece on the ground or on a shelf ticks too, at the same
early-out, as every vanilla shield belt does.

Trigger numbers are `CompProperties` fields, as Toxin's are: an `aiCheckInterval` (Toxin's
60), a search radius for patients, and a quiet window (3.4). Each piece also carries a bool
(`aiCasts`, default true) that gates its check; it is a mod setting, one checkbox in the
ability's own settings section (the dressing's for the armor, the shot's for the shoulders),
written onto the comp props by `ApplyToDefs` as the set's other settings are. A colonist pilot
never reaches the check (3.5), so the box reads as "raiders use this ability". Toxin ships its
AI without a setting; this set does not follow it there.

### 3.2 The dressing's check (armor)

Gates, cheapest first: the wearer is not `IsColonistPlayerControlled`, is spawned, up,
awake, not in a mental state and not already running one of our jobs; the suite holds at
least one kit; the wearer's lord, if any, is not on an exit toil (`LordToil_ExitMap` or its
kin: the medic leaves with the raid); the quiet window holds (3.4).

Patient: the nearest pawn within the search radius that is on the wearer's side (below),
downed, spawned, not dead, reachable, and has tendable hediffs the dressing would take
(`FillTendable` is the rule, helmet included). Nearest first, bleeding first if two are
equal, is enough; there is no triage beyond what the beam already does per wound.

On the wearer's side (decision 4.4): the patient has a faction, and it is the wearer's or
one the wearer's faction holds as an ally, `wearer.Faction.RelationKindWith(patient.Faction)
== FactionRelationKind.Ally`, goodwill 75 and up. The same-faction test goes first:
`Faction.RelationWith` logs an error when asked about itself (vanilla's own callers guard it
the same way, `IdeoUtility`). A factionless pawn, a wild animal, and any prisoner (a captured
raider keeps the raid's faction, and a medic does not tend the colony's prisoners) never
qualify; a colonist does when the wearer's faction is the player's ally, so an allied
faction's defender in the set (`LordJob_AssistColony`) dresses the colony's downed.

Order: `JobMaker.MakeJob(MDWC_MechaniteDressing, patient)`, `playerForced = true`,
`StartJob(..., InterruptForced)`. The job then runs the walk, warmup, beam, shot and
warmdown unchanged.

### 3.3 The shot's check (shoulders)

Same gates, with the injector's count in place of the suite's. Patient: the nearest downed
pawn on the wearer's side (3.2's rule) the shot validates for (`ValidateTarget`, messages
off), which already rules out a patient at the top level. The job is `MDWC_AdrenalineShot`
by the same order path, and the shot is the pilot's shot, full severity (decision 4.3).

After `Apply`, if the patient stands (`!Downed` on the next check, or at once if the hediff's
pain factor lifts them in `Apply`), the re-add (decision 4.2): if the patient is of the
medic's own faction, is nobody's prisoner or guest, has no lord, and the medic has a lord
whose `CanAddPawn(patient)` holds, `lord.AddPawn(patient)`. An allied patient, a colonist above all, is never added to the
medic's lord; they keep whatever they had. Done from the shot's `Apply` for a non-player
pilot, so the dressing's shot toil gets it too.

Then the rearm, after the re-add since the toil's duty refresh ends the current job: a
patient of the medic's own faction with no primary starts vanilla's Equip
(`JobGiver_PickupDroppedWeapon.PickupWeaponJob`, forbidden ignored, the job
`Hediff_DeathRefusal` starts for a pawn it resurrects) on the weapon `MakeDowned` remembered
in `mindState.droppedWeapon`, or when that is gone the nearest usable one within 8 cells.
The reclaim node vanilla has for this sits under `ThinkNode_ConditionalColonist`, and the
raid duties' opportunistic pickup is gated on a flag only the refugee betrayal sets, so a
revived raider would otherwise fight bare-handed.

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

1. **Dressing alone.** DECIDED: ship it, on by default. A raider with the armor but no
   shoulders stops bleeds and leaves more captives: thematic, and the armor without it is
   a blander, weaker torso than the VFEP spacer sets it is drawn beside (`Set-Tuning.md`),
   whose passives need no wearer's intent. A medic who only acts when it can also revive
   is the kind of rule a player has to be told about.
2. **Walk-off.** DECIDED: the re-add, on every map, with no setting of its own. A lordless
   revived raider walks out at `Walk` urgency with no avoid grid, through the colony's
   fire, and on a map with no exit idles in the colony (2.3); the re-add hands them the
   raid's current duty instead, the assault while it stands and the raid's own exit, at a
   jog on the avoid grid, once it flees. The shot is a combat stimulant: the medic's part
   ends at standing them up, and where they go is the raid's call. The flee fraction is
   left as it falls (2.3); a revive that should not count as a loss is a later tweak.
3. **Stage.** DECIDED: full severity, the pilot's shot, one entry point. A weaker AI jab
   would be an asymmetry with no fiction behind it, and one more thing for the player to
   be told.
4. **Scope of "own side".** DECIDED: the wearer's faction and its allies (3.2), so a
   defender an allied faction sends to the colony tends colonists too; a neutral faction's
   pawn is not on the medic's side, and an animal of the raid is in the faction, so it
   qualifies on its own. The re-add stays own-faction (3.3). If the relation query turns
   out not to hold in play (an allied faction's lord that fights the colony, a quest
   faction with a relation of its own), own-faction only is the fallback; the seam is one
   predicate.

## 5. Localization surface

Two Keyed strings, the checkbox labels, with their tooltips: the two `aiCasts` settings.
The three guarded messages keep their keys; nothing else the player reads changes.

## 6. Verification

1. Spawn a Veteran raid with dev mode, give one raider the armor and ten kits, down another
   near them: the medic walks over, the glow and beam show, the wounds tend, no message
   appears in the player's log, and the medic returns to the assault.
2. Give the medic the shoulders too: the shot follows the beam, the patient stands, and
   (with the re-add) rejoins the assault rather than walking to the map edge, picking their
   weapon back up first. Haul the weapon away before the shot: they take the nearest one
   they can use within 8 cells, or fight unarmed if there is none.
3. Shoulders only: the raider gives the shot and nothing else.
4. Down a raider while the medic is being shot at: no dressing until 300 ticks after the
   last hit. Trigger the flee: the medic leaves with the raid, patient or not.
5. A colonist pilot: no behaviour change, gizmos and autocast as before. Draft a colonist,
   let a tantrum break: they dress a downed colonist on their own.
6. Allied: set a faction to ally in dev mode, spawn one of its pawns in the set beside a
   downed colonist: the dressing and the shot land, the colonist stands and stays the
   player's, with no lord. Set the faction neutral: nothing happens. Each box off in
   settings: that piece's check never fires, the other's still does.

## 7. Implementation checklist

- [x] Decisions 4.1 to 4.4 taken and recorded here.
- [x] The AI check: `CompMedicAI` with `CompProperties_MedicAI` on both pieces (3.1), the
      rules in `MedicAI`.
- [x] Shoulders: the comp entry, and `tickerType Normal` on the def.
- [x] `MedicWarcasketSettings`: the two `aiCasts` checkboxes (`ToggleSetting`), one per
      ability section, and their two Keyed labels with tooltips.
- [x] The three messages guarded for a player pilot, at the one call a non-player pilot
      reaches (`Ability_MedicTargeted.ReportsToPlayer`).
- [x] The re-add in `Ability_AdrenalineShot.Apply` (4.2), gated on `CanAddPawn` and no lord.
- [x] The rearm after it (`MedicAI.TryRearm`): the remembered weapon, else the nearest usable.
- [x] Def headers updated; `Set-Tuning.md` 2.7 and `Field-Dressing.md` pointed here.
- [x] `Tests/1.6/MedicAITests.cs`: the quiet window, the leaving toils and the preference.
- [ ] Section 6 run in game, on 1.7 and 1.6.
