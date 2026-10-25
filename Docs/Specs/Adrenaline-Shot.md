# SPEC: Adrenaline shot (the shoulders' active)

Status: LOCKED 2026-10-25, ready for implementation, after two review passes the same day
(their changes are in the brief table; the readings in section 4 are settled). Written against the installed RimWorld 1.7.4985 rev1269
(`Assembly-CSharp.dll` via `ilspycmd`, Core XML), the 1.6 Krafs reference 1.6.4871 for the
member checks, the VEF checkout at `../VanillaExpanded/VanillaExpandedFramework/` (58c884b9,
2026-10-21) with every VEF member cited confirmed in the Workshop `VEF.dll` players run (the 1.6
and 1.7 assemblies of 2023507013), and the siblings `../ToxinWarcasket/` (an apparel ability
that fires its own autocast) and `../ShipcrackerWarcasket/` (the gizmo's corner count). Every
1.6 fact below is a signature check on the reference assembly; behaviour was read on 1.7 and is
taken to hold on 1.6, as Ludeon frames the update; the author verifies by hand before release.

The shot is the set's second active and the shoulders' own: the Workshop page already promises
"Grants adrenaline shot" on the shoulders. It is deliberately small next to the mechanite
dressing (`Mechanite-Dressing.md`): one hediff, one reloadable, one cast, and it borrows the
dressing's ordering, walk and cursor idioms rather than inventing its own. Section 4 lists the
readings this draft takes of the brief, for the review to confirm or move.

## 0. The brief

| Request                                                                               | Section  |
| ------------------------------------------------------------------------------------- | -------- |
| A gizmo and targeter like the mechanite dressing's                                    | 3.3, 3.5 |
| A go-juice-like hediff that fades over 3 hours                                        | 3.2      |
| A rest cost as the shot fades: a rest fall factor on the hediff (review: not a stage hook) | 3.2, 4   |
| Review: the factor climbs a level at a time, x2, x3, x4                                | 3.2, 3.7 |
| Charge noun "shot", "charge" or whatever fits and translates                          | 3.1, 4   |
| Reloaded with go-juice, one go-juice loading several shots                            | 3.1, 3.6 |
| Stops a heart attack outright, at any severity                                        | 3.3      |
| Review: a pawn with a heart attack can always be targeted                             | 3.3      |
| Review: on a heart attack the shot cures instead of stimulating                       | 3.3      |
| Review: a top-left message naming the pawn, not a floating text                       | 3.3      |
| An autocast toggle, offered while the armor is worn, firing as a dressing ends        | 3.4      |

## 1. Fiction

The shoulders' field-medicine manipulators carry an injector fed from a vial of go-juice. The
suit does not inject the drug raw: it refines the vial into a few measured shots of a short,
sharp stimulant and delivers one into whoever the pilot reaches for, the pilot included. For a
few hours the patient feels almost no pain and moves, sees and thinks faster, the surge fading
in steps, and all the while the body burns through its reserves: the patient will need sleep
sooner. A shot into a failing heart is spent on the heart alone: it jolts it back into rhythm,
which is the one thing the shot does that no amount of bandaging can, and there is nothing left
over to stimulate.

This fiction explains each mechanic:

- **A refined go-juice** is why the shot is go-juice's effect on a shorter clock, with no
  addiction and no tolerance: the carrier chemicals never reach the bloodstream.
- **The fade in steps** is the hediff's stages; **the reserves burning** is the faster rest fall
  under every stage.
- **The heart** is why the shot cures a heart attack and nothing else, and why a shot spent on
  one gives no surge.
- **The injector is on the shoulders**, so the shot comes with the shoulders alone; the autocast
  needs the armor's dressing to follow, which is why the toggle appears only with the armor.
- **The pilot's own shot** matters less than it sounds: a warcasket pilot already feels no pain
  (VFEP's trait, 2.4), so only the capacity offsets and the heart matter to them.

## 2. Verified engine facts

### 2.1 Go-juice and its high (Core XML)

- `GoJuiceHigh` (`Data/Core/Defs/Drugs/GoJuice.xml`) is a plain `HediffDef` with no parent
  (there is no `DrugHigh` base), `hediffClass` `Hediff_High` (which only shows the severity as a
  percentage), `isBad` false, `maxSeverity` 1, label colour (1, 0, 0.5), one unlabelled stage:
  `painFactor` 0.1, Consciousness +0.20, Sight +0.35, Moving +0.50, and a
  `HediffCompProperties_SeverityPerDay` of -0.75 with `showHoursToRecover`. Ingesting the drug
  applies severity 0.5 (a 16 h high), offsets Rest by +0.4 and psyfocus by +0.15, through
  `IngestionOutcomeDoer_GiveHediff` and `IngestionOutcomeDoer_OffsetNeed`; the latter is a plain
  `need.CurLevel += effect`.
- `GoJuice` the item: market value 53, mass 0.1, stack 150 (from `DrugBase`), Industrial, hard
  drug, made from 2 neutroamine and 1 yayo at the drug lab after `GoJuiceProduction`. Its
  ingestion sound is `Ingest_Inject` (`SoundDefs/Interact_Ingest.xml`), which has no `SoundDefOf`
  handle. `ThingDefOf.GoJuice` exists on 1.6 and 1.7 with no `MayRequire`.
- `WakeUpHigh` for comparison: Consciousness +0.1, Moving +0.1, `RestFallRateFactor` x0.8, with
  a `HediffGiver_Random` for `HeartAttack` (mtb 120 days) in its stage; the drug fills Rest
  outright (+1.0).

### 2.2 The heart attack

- `HeartAttack` (`HediffDefs/Hediffs_Global_Misc.xml`): `hediffClass` `Hediff_HeartAttack`,
  `initialSeverity` 0.4, `lethalSeverity` 1, `tendable`, **no comps**, three stages (painful,
  debilitating at 0.6, fatal at 1). Its course is hard-coded in the class: every
  `5000 × Rand(0.1, 2)` ticks the severity moves by `Rand(-0.4, 0.6)`, and a tend succeeds with
  chance `0.65 × quality` and subtracts 0.3. A pawn dies the moment severity reaches 1. Nothing
  in any DLC patches the def; Anomaly's shambler lists it under `removesHediffs`.
- `Pawn_HealthTracker.RemoveHediff(Hediff)` (1.6 and 1.7) is the clean removal: `PreRemoved`,
  list removal, cache dirtying, `PostRemoved`, `CheckForStateChange`. `HediffDefOf` has no
  `HeartAttack` field on either version (only `AdrenalHeart`), so the def is resolved by name.

### 2.3 Stages, ticking, removal and the rest need

- `Hediff.CurStageIndex` is `def.StageAtSeverity(Severity)`: the last stage whose `minSeverity`
  the severity meets, so **indexes grow with severity** and a fading hediff's index falls.
  `ConfigErrors` requires ascending `minSeverity`.
- For the record, since the first draft used it: the stage hook is `Hediff`'s `protected
  virtual void OnStageIndexChanged(int newIndex)` (1.6 and 1.7), called by the `Severity`
  setter on any index move; `HediffComp` has no stage virtual. The design below needs neither.
- `HediffComp_SeverityPerDay` adds `severityPerDay × 200 / 60000` every 200 ticks from
  `CompPostTickInterval`, scaled by the stage's `severityGainFactor`; it **caches the per-day
  value per instance** in `CompPostPostAdd` (`Props.CalculateSeverityPerDay()`), so a later write
  to the props reaches only hediffs added after it. `showHoursToRecover` puts the hours left in
  the label's brackets.
- `Hediff.ShouldRemove` is `Severity <= 0`; the health tick sweeps such hediffs through
  `RemoveHediff`, so a fading hediff ends with `PostRemoved` on the live pawn. The setter clamps
  to `[def.minSeverity, def.maxSeverity]`. `PostMake` raises the severity to `initialSeverity`.
- **Re-applying a hediff merges.** `HediffSet.AddDirect` tries `TryMergeWith` on every existing
  hediff first, which for a same-def, same-part hediff adds the severities and zeroes the age;
  VEF's `ApplyHediff` goes through `AddHediff` and therefore merges too (2.5). A refresh that
  must *set* the severity writes the existing hediff's `Severity` directly.
- Labels: `Hediff.Label` is `LabelBase (stage label, comp extras)`, so a labelled stage shows
  as "heart attack (painful)" and a per-day comp adds "3h" after it.
- **Rest.** `Need_Rest` falls `1.5833e-5` per tick while rested (×0.7 tired, ×0.3 very tired,
  ×0.6 exhausted), times `HediffSet.RestFallFactor` (**the product of every hediff stage's
  `restFallFactor`, plus the `restFallFactorOffset`s, read live on every need tick**) and the
  pawn's `RestFallRateFactor` stat, ticked every 150 ticks; resting gains are unaffected by
  either. A rested pawn loses 1.0 in about 25 hours, **0.04 an hour**. `Tired` starts under
  0.28, `VeryTired` under 0.14. Animals have the need (`RaceProperties.needsRest` is true for
  everything but mechanoids).
- `HediffStage` carries `label`, `painFactor`, `capMods`, `restFallFactor` (1), `restFallFactorOffset`,
  `statOffsets`, `statFactors`, `blocksSleeping` and the rest; `HediffDef`
  carries `initialSeverity` (0.5), `maxSeverity`, `minSeverity`, `isBad` (true),
  `everCurableByItem` (true), `scenarioCanAdd`, `defaultLabelColor`, `labelNoun`, `tags`.

### 2.4 The pilot

VFEP's `VFEP_WarcasketTrait` (1.7 XML; the 1.6 tree has the pain factor only) gives the pilot
`painFactor` 0 and `RestFallRateFactor` x1.3, and the medic trait mirrors it at startup
(`Medic-Work-Trait.md`). A shot on the pilot therefore has no pain to suppress, its capacity
offsets apply as to anyone, and its rest cost is a flat amount the pilot's faster fall does not
scale.

### 2.5 VEF: apparel abilities, autocast, hediffs

- **VEF's autocast never fires an apparel ability.** `Ability.autoCast` is a public scribed
  bool; `AutoCast` (virtual) returns it for a player colonist; `CanAutoCast` (virtual) gates the
  toggle. `Ability.DoAction` flips `autoCast` on a right-click when `CanAutoCast`, which vanilla
  routes to the command's action because `Command_Ability` offers no float menu; a disabled
  command swallows the click, so the toggle is unreachable while the ability is disabled.
  `Command_Ability.GizmoOnGUI` draws `UI/CheckAuto` 24 px square in the button's **top-right
  corner** while `AutoCast` is true, and `GetDescriptionForPawn` appends
  `VFEA.RClickToAuto`/`RClickToNoAuto` while `CanAutoCast`. The only consumer is a postfix on
  `Pawn.TryGetAttackVerb` that reads `CompAbilities.LearnedAbilities`; `GivenAbilities` is never
  read. Toxin's `Ability_AbsorbGas` is the worked precedent: it reads `autoCast` itself from its
  own tick.
- **`Init()` resets `autoCast`** to `CanAutoCast && def.autocastPlayerDefault`, and
  `CompAbilitiesApparel.CompGetWornGizmosExtra` re-runs `Init()` whenever its unsaved wearer
  cache differs from the wearer, which includes the first gizmo request after every load. Toxin's
  `Init` override saves the flag across the base call. `AutoCast` dereferences `pawn` unguarded,
  and `pawn` is null on an unworn piece until the first gizmo request.
- `CompAbilitiesApparel.Initialize` builds one ability per listed def and scribes the list deep;
  a piece saved before the comp existed loads an **empty** list, so shoulders worn in an older
  save never gain the shot (an unreleased-build concern only; the CHANGELOG line about such saves
  already exists).
- `AbilityDef` has `castTime`, `cooldownTime`, `distanceToTarget` (1.5), `requireLineOfSight`,
  `showUndrafted`, `jobDef` (optional; `StartAbilityJob` defaults it to `VFEA_UseAbility`),
  `castSound` (played at the caster by `CastEffects`), `fleckOnTarget`, `targetFlecks`,
  `autocastPlayerDefault`, `gizmoClass` (must derive from `Command_Ability`; `ConfigErrors`
  checks it, `GetGizmo` activates it, and `GetGizmo` is virtual), `needsTicking` /
  `needsTickingInterval` (required for a subclass declaring `Tick` / `TickInterval`, else a
  config error). Hediff application from XML is one `AbilityExtension_Hediff` per def (`hediff`,
  `severity`, `applyAuto`, `applyToCaster`, `targetOnlyEnemies`); `Ability.Cast` runs the
  cooldown, goodwill, extensions, `ApplyHediffs`, `CastEffects`, `TargetEffects`, `PostCast` in
  that order, and a `Cast` override without the base call skips all of it.
- `JobDriver_CastAbilityOnce` and the ordering, binding, stun and cursor facts are in
  `Mechanite-Dressing.md` 2.1 and 2.2 and are reused unchanged (3.3).

### 2.6 The vanilla reloadable with a refill cost

`CompApparelReloadable` (bodies read on 1.7; every member present on 1.6):

- `ammoCountToRefill` and `ammoCountPerCharge` are exclusive (`ConfigErrors`). With
  `ammoCountToRefill` set, `ReloadFrom` destroys exactly that many items and sets
  `remainingCharges = MaxCharges`; `NeedsReload(allowForcedReload: false)`, the idle
  `JobGiver_Reload` path, is `remainingCharges == 0`, while the forced path (the float menu, the
  dev button) is `remaining != max`. So colonists fetch a vial only once the last shot is spent,
  and a forced early reload pays a whole vial. VFEP's Controller set uses the same shape
  (`ammoCountToRefill` 1 of `ComponentSpacer`), as do Core's smokepop and firefoam packs.
- `PostPostMake` fills the charges on creation. `CompApparelVerbOwner.CompGetWornGizmosExtra`
  yields one command **per verb**, so a reloadable on a def with no `<verbs>` shows no gizmo of
  its own (only the dev "Reload to full"); its count reaches the player through
  `CompInspectStringExtra` ("Shots remaining: 2 / 3"), `CompTipStringExtra` and the stat sheet,
  and the props print the refill line (`Stat_Thing_ReloadRefill`, "1 go-juice") on both cards.
  `UsedOnce()` (virtual) decrements. `displayGizmoWhileUndrafted` gates only those verb commands.
- Vanilla's own charge nouns: `charge` (default), `smokepop`, `firefoam pop`, `jump`, `shield`,
  `grenade`, `incendiary`, `flare`, `shell`, `tox pack`. The noun is printed through
  `{CHARGENOUN_plural}` and lookup slots only, never bare.

## 3. Design

### 3.1 Pieces and defs

**Shoulders** (`ThingDefs_Misc/WarcasketShoulders_Medic.xml`) gain two comps, the Aerial
shoulders' pairing:

- a plain vanilla `CompProperties_ApparelReloadable`: `maxCharges` 3, `ammoDef` `GoJuice`,
  `ammoCountToRefill` 1, `baseReloadTicks` 60, `soundReload` `Standard_Reload`, `chargeNoun`
  "shot". No comp subclass: the ability spends through `UsedOnce()` and vanilla does the rest
  (2.6). The free first fill is three shots, one vial (53 silver), to be priced into the
  shoulders' cost in the tuning pass. `maxCharges` is a mod setting (3.7).
- `VEF.Abilities.CompProperties_AbilitiesApparel` granting `MDWC_AdrenalineShot`.

The ability reads the reloadable off `holder`, so both comps sit on the shoulders, as the
dressing's pair sits on the armor. The shoulders carry the shot on their own; nothing here
checks for the armor except the autocast (3.4).

**New defs, one per file:**

- `AbilityDefs/AdrenalineShot.xml`: `VEF.Abilities.AbilityDef` `MDWC_AdrenalineShot`,
  `abilityClass` `MedicWarcasket.Ability_AdrenalineShot`, `jobDef` `MDWC_AdrenalineShot`,
  `targetMode` Pawn with an explicit `targetingParametersList` entry (humans and animals, no
  mechs, **self allowed**, no buildings or locations), `distanceToTarget` 2.9, `requireLineOfSight`
  false (the walk enforces both, as the dressing's does), `castTime` 30 (0.5 s), `cooldownTime` 0,
  `showUndrafted` true, `autocastPlayerDefault` false, `castSound` unset (3.3 plays the sound at
  the patient). No `iconPath` until the ability art lands: the settings write go-juice's `uiIcon`
  onto `def.icon`, as the dressing's medicine icon is written (`TODOs.md` already names go-juice
  as the shot's placeholder). No `AbilityExtension_Hediff`: the shot applies its hediff itself
  (3.3), since VEF's path would merge severities (2.3) and cannot refresh.
- `JobDefs/AdrenalineShot.xml`: `MDWC_AdrenalineShot`, `driverClass`
  `MedicWarcasket.JobDriver_AdrenalineShot`, `reportString` "giving an adrenaline shot.",
  `checkOverrideOnDamage` Never, `carryThingAfterJob` true, as the dressing's job.
- `HediffDefs/Adrenaline.xml`: `MDWC_Adrenaline`, section 3.2, XML only (not the ability's
  name: a `DefOf` field is named by its def, and the class already holds the ability's).

`MDWC_DefOf` gains `MDWC_AdrenalineShot` (AbilityDef), `MDWC_Adrenaline` (HediffDef), `HeartAttack` (HediffDef, by
name since `HediffDefOf` lacks it) and `Ingest_Inject` (SoundDef, likewise).

### 3.2 The hediff: three levels, burning rest all the way down

`MDWC_Adrenaline`, pure XML: `hediffClass` `Hediff_High` (go-juice's, which shows the
severity as a percentage), label "adrenaline shot", `labelNoun` "an adrenaline shot", `isBad`
false, `everCurableByItem` false (a healer mech serum must not spend itself on a stimulant),
`scenarioCanAdd` false, `initialSeverity` 1, `maxSeverity` 1, label colour go-juice's
(1, 0, 0.5), a `HediffCompProperties_SeverityPerDay` of **-8** with `showHoursToRecover` (1.0
fades in 3 h), and three stages, go-juice's effect at the top and tapering by thirds while the
`restFallFactor` climbs a step per level:

| Level (stage label) | `minSeverity` | `painFactor` | Consciousness | Sight | Moving | `restFallFactor` |
| ------------------- | ------------- | ------------ | ------------- | ----- | ------ | ---------------- |
| rush                | 0.667         | 0.1          | +0.20         | +0.35 | +0.50  | 2                |
| fading              | 0.334         | 0.4          | +0.13         | +0.23 | +0.33  | 3                |
| wearing off         | 0             | 0.7          | +0.07         | +0.12 | +0.17  | 4                |

Each level lasts an hour by default. The health tab reads "adrenaline shot (rush, 3h)".

**The comedown is the rest fall factor** (2.3), vanilla's own lever (wake-up's high slows the
fall the same way): under a shot the patient's rest falls twice as fast, then three times,
then four as the surge fades, so the bill climbs as the boost thins and a full shot that runs
its course costs about 0.24 of rest beyond the 0.12 three waking hours cost anyway, roughly six
hours of wakefulness for three hours of go-juice-grade boost. It is paid continuously rather
than at each level's end, which is what the hediff set reads and what the need's own tired and
very-tired brakes already shape; a sleeping patient pays nothing, since the factor only scales
the fall. The ladder's first step is a setting, the next two always one higher each (3.7),
written onto the stages and read live, so it reaches shots already running. The pilot's own
`RestFallRateFactor` x1.3 (2.4) multiplies on top for a self-shot.

**No merge.** The shot refreshes rather than stacks (3.3), so the severity never exceeds 1 and a
second shot restarts the clock from the top level.

### 3.3 The shot (`Ability_AdrenalineShot`)

**Valid target** (`ValidateTarget`, after the base), a spawned, living pawn, humanlike or
animal, the pilot included, by three rules in this order:

1. **A pawn with a heart attack is always valid**, whoever they are and whatever else they
   carry: the shot will be spent on the heart (below) and stands nobody up, so hostility does
   not enter. Raiders and prisoners do get them (2.2's givers sit on the age-related heart
   hediffs every humanlike can carry, and on wake-up's high), so a captive worth keeping can be
   saved. A conscious hostile is held by the warmup's stun like any other patient.
2. Otherwise, **never a pawn hostile to the pilot, downed or not** ("{0} is hostile"): a shot
   that lifts the pain of a downed raider can stand them back up, which is the dressing's
   hostile rule with the downed exception removed, and the reason the two differ.
3. Otherwise, a patient already at the top level is refused ("{0} is already running on
   adrenaline"), so a double click cannot waste a shot; a patient at a lower level, or under
   none, is valid, and a refresh is the way to keep a squadmate going.

**Enabled** (`IsEnabledForPawn`, after the base cooldown check): the shoulders' reloadable
holds a shot; else "No shots loaded" (vanilla's `CommandReload_NoCharges` wording through our
own key, since the comp's `DisabledReason` is built for a verb).

**Ordering, walk, cursor:** the dressing's idioms, hoisted (3.5): a plain click orders through
`TryTakeOrderedJob` and clears the queue, Shift-click chains and queues, the pilot walks to
within `distanceToTarget` with a line of sight, a downed patient is reached in bed, a self-target
skips the walk (the pilot is already within reach of themself). The job's check toil re-runs
`ValidateTarget` and the charge check with the pilot in reach, spending nothing on failure. The
warmup (0.5 s) holds a conscious patient with the dressing's refreshed stun and skips a downed
one. The `MouseAttachment` cursor rule and `HidePawnTooltips` are the dressing's; the cursor
label reads "Shots: {loaded} / {max}" and, over a patient with a heart attack, a second line
"Stops the heart attack".

**`Apply(patient)`**, the one path both the cast toil and the autocast (3.4) use, in order:

1. Spend one shot: `reloadable.UsedOnce()`.
2. **Either** cure: if the patient has any `HeartAttack` hediff, remove every one through
   `pawn.health.RemoveHediff` (2.2) and post a message naming the pawn, "{0}'s heart attack has
   stopped.", as a positive event with the patient as its jump target
   (`Messages.Message(text, patient, MessageTypeDefOf.PositiveEvent)`): a life saved rates the
   top-left of the screen, where vanilla announces the attack itself, not a floating label the
   player can miss. No stimulant is applied: the shot was spent on the heart.
3. **Or** stimulate: if the patient already carries `MDWC_Adrenaline`, set its `Severity`
   to 1; else make the hediff and `AddHediff` it. The hediff needs no body part. No text: the
   sound and the health tab carry a routine shot, so nothing overlaps the dressing's wound
   texts.
4. `Ingest_Inject` plays at the patient (2.1).
5. Start the cooldown: `cooldown = TicksGame + GetCooldownForPawn()`, where VEF's own `Cast`
   would have.

`Cast` calls `Apply` and nothing else (no base call: VEF's goodwill, extensions and fleck steps
are unused, and the one order the shot needs is the one above). `EndCastJob` has nothing to
settle. No records, no XP: vanilla's drug administering grants neither.

**Fleck:** none in this draft; the sound and the hediff carry the moment. A fleck on the patient
is one `fleckOnTarget` line away once the art pass picks one (section 4).

### 3.4 Autocast: a shot as the dressing ends

The toggle is VEF's own flag on the shoulders' ability, read by our code, Toxin's way (2.5):

- `CanAutoCast` is true while the pilot wears the Medic armor, found as the dressing's own
  ability on the worn apparel (`JobDriver_MechaniteDressing.FindDressing`, hoisted in 3.5);
  false otherwise, and false while `pawn` is null. So the right-click line and the toggle appear
  only with the armor, and come back the moment it is worn again. `AutoCast` is the base's,
  guarded for a null pawn.
- `Init()` saves the flag across the base call, as Toxin's does, so a load or a wearer re-bind
  does not clear the player's choice; VEF scribes the flag itself.
- `GetDescriptionForPawn` adds, after VEF's lines, one line of our own under the right-click
  hint: "Autocast: a shot to each patient as their dressing ends." (3.6 caches the description
  on the autocast state and the settings generation, as Toxin does.)

**The dressing fires it.** `JobDriver_MechaniteDressing` gains one toil between the beam and
the warmdown, **Shot**, so the hold the stun gives the patient covers it and the glow's fade
stays the closing phase. The toil is `castTime` ticks of the shot def (0.5 s) with the dressing's
stun refreshed each tick, then `Apply(patient)`. It runs only when every condition holds at the
toil's start, and completes at once otherwise:

- the pilot wears the shoulders with `AutoCast` on and a shot loaded, and the shot's cooldown is
  clear (a running cooldown skips the patient rather than waiting);
- the patient is in the **dressing's** reach (the beam just ended there) and `ValidateTarget`
  passes without messages: a heart attack always passes, else no hostile and no patient
  already at the top level;
- the patient is not lying in a bed, unless they have a heart attack: a hospital dressing is
  not a fight, and a shot there would only cost the patient the sleep they are about to get.

The beam's outcome does not matter: a complete or a dry beam both go on to the shot; a broken
beam fails the reach check and skips it. A dressing whose connect found nothing to tend still
reaches the toil, since the pilot was ordered to that patient. `Apply` is the same method as the
manual cast, so the shot's cooldown, charge and texts are identical either way; the shot's own
`distanceToTarget` is a walking rule for manual orders only.

### 3.5 Shared idioms, hoisted (no behaviour change for the dressing)

The dressing's ordering and walk are the right ones for any targeted medic ability, so the shot
reuses them through two small base classes rather than a copy:

- `Ability_MedicTargeted : Ability, ITargetingSource` (`Source/1.6/Abilities/`): the unlimited
  `GetRangeForPawn`, `CanAutoCast`/`AutoCast` pinned off (the shot overrides them back on, 3.4),
  `MultiSelect`, `HidePawnTooltips`, `OrderForceTarget`, `CreateCastJob` through
  `TryTakeOrderedJob`, `MouseAttachment` over a quiet `ValidateTarget`, the `InReach` test
  against `def.distanceToTarget`, `RefreshStun`, `ThrowText`, and `ReconcileSettings` keyed on
  `MedicWarcasketSettings.Generation`. `Ability_MechaniteDressing` and `Ability_AdrenalineShot`
  derive from it and keep their own validation, cast and state.
- `JobDriver_MedicCast : JobDriver_CastAbilityOnce` (`Source/1.6/Abilities/`): the walk, the
  cooldown wait, the check, and the start-only binding. The binding finds the pilot's ability as
  the worn apparel's `GivenAbilities` entry **whose def's `jobDef` is this job's def**, so the
  driver needs no per-ability DefOf and `FindDressing` becomes `FindAbility(pilot, AbilityDef)`
  on the base. `GetReport` reads that def's `jobReportString`. `JobDriver_MechaniteDressing`
  adds the beam, the shot (3.4) and the warmdown; `JobDriver_AdrenalineShot` adds nothing, since
  VEF's warmup and cast toils already apply the shot.

The dressing's header comments move the shared paragraphs to the base classes and keep the
dressing's own. Section 6 re-runs the dressing's queue test to show nothing moved.

### 3.6 Gizmo and readouts

VEF's command draws its autocast check in the top-right corner (2.5), which is where vanilla
draws a charge count, so the shot's command is its own: `Ability_AdrenalineShot.GetGizmo`
returns one cached `Command_AdrenalineShot : Command_Ability` (the def's `gizmoClass` names it,
to satisfy `ConfigErrors`), refreshed per request as Toxin's is (enabled state, reason,
description), with:

- `TopRightLabel` "2 / 3", the loaded shots over the maximum, Shipcracker's corner count;
- the autocast check drawn by the command itself in the **top-left** corner when `AutoCast` is
  on, by overriding `GizmoOnGUI` to draw VEF's `UI/CheckAuto` there after the base draws the
  button, and by returning false from the ability's `AutoCast` for the duration of that base
  call only, so VEF's own top-right draw is skipped. The flag is read through a private
  "drawing" guard on the ability, reset in a `finally`. The command has no hotkey, so the
  top-left is free.

The shoulders' inspect line, tooltip and stat sheet are vanilla's ("Shots remaining: 2 / 3",
the refill line "1 go-juice"), untouched. No gauge: three whole shots need no bar.

### 3.7 Settings

A second section, **Adrenaline shot**, between the dressing's and the work section, its header
carrying the shot's tooltip over go-juice's icon as the dressing's carries its medicine's.
Defaults stay in the def XML, read by `LoadDefaultsFromDefs` and written by `ApplyToDefs`:

| Setting             | Def field                                                        | Default | Range           |
| ------------------- | ---------------------------------------------------------------- | ------- | --------------- |
| Shots per vial      | shoulders' reloadable `maxCharges`                               | 3       | 1..10, step 1   |
| Duration (hours)    | hediff `HediffCompProperties_SeverityPerDay.severityPerDay`      | 3       | 0.5..12, step 0.5 |
| Rest fall at a shot's start (×) | the hediff's stage `restFallFactor`s, the top stage's value; each lower stage one higher | 2 | 1..5, step 0.5 |
| Cooldown (seconds)  | ability `cooldownTime` (ticks ÷ 60)                              | 0       | 0..300, step 5  |

- The duration is stored as hours and written as `severityPerDay = -24 / hours`; the default is
  read back as `-24 / severityPerDay`. A shot already in flight keeps the rate it was applied
  with (2.3), as a dressing in flight keeps its colour; the next shot takes the new one. The
  rest fall row writes the top stage's `restFallFactor` and, for each stage below it, one more
  (the XML ladder's step), read live, so it reaches every shot at once; its default is the top
  stage's XML value.
- `ApplyToDefs` also writes go-juice's `uiIcon` onto the shot def's `icon` while it has no
  `iconPath`, and trims worn shoulders above a lowered `maxCharges` (`remainingCharges` is
  protected; a tiny `CompApparelReloadable` subclass is **not** worth it: the dev "reload to
  full" and `ReloadFrom` both write `MaxCharges`, so the trim is a `UsedOnce()` loop down to the
  new maximum, which spends nothing real).
- The warmup (30 ticks) and the range (2.9) are def fields without settings; the level count is
  the stage list. Section 4 lists them as levers.
- Summary line: "The shoulders hold {0} shots per vial of go-juice. A shot lasts {1} h, fading
  in three levels, and the patient's rest falls {2} times as fast at first, faster with each level, with {3} s
  of cooldown between shots." A second key ends "..., and the next shot can follow at once." when
  the cooldown is 0.
- `TryGetDefs` grows by the shot's three defs; all-or-nothing as now, with the warning naming
  what is missing.

### 3.8 Charging, from the player's side

One vial of go-juice loads three shots, each about 18 silver of a 53-silver vial, for 3 hours of
go-juice's full effect tapering to a third, with no addiction and no tolerance, against rest
falling three times as fast until it ends. Go-juice itself gives 16 hours and +0.4 rest for 53 silver
and an addiction roll, so the shot is the costlier way to use the drug, bought for its safety,
its reach from the shoulders, the heart, and the fact that it is the pilot who carries it into
a fight. A shot spent on a heart attack buys the cure alone, at the same price. Colonists fetch a new vial only once the last shot is spent; a forced early reload
pays a whole vial.

## 4. Readings taken, and open levers

Readings of the brief for the review to confirm:

1. **The rest cost is a fall factor, not a per-level deduction** (review): the first draft
   paid 0.1 at the end of each level through a hediff class on the stage hook; the factor is
   vanilla's own lever, needs no C#, reads a settings change live, and lets the tired brakes
   and sleep itself behave as they always do. The one thing lost is the lump: a patient who
   sleeps through the shot pays nothing. The ladder x2, x3, x4 (settled on the second
   pass) lands on the first draft's total (3.2) and makes the fade felt in the bill as well as
   in the boost; the setting moves the first step and the ladder keeps its unit steps.
2. **Charge noun "shot".** It reads as vanilla's own nouns do ("Shots remaining: 2 / 3", "No
   shots left"), has a one-word equivalent in every roster language, and is the word the
   Workshop page already uses. "Charge" would be VFEP's generic and says less.
3. **Three shots per vial.** Vanilla's refill packs hold three (smokepop) at a chemfuel cost;
   three keeps the first fill at one vial and the corner count legible.
4. **The hostile rule is stricter than the dressing's** (3.3): the dressing may tend a downed
   raider for capture, the shot may not stand one up. **A heart attack overrides it** (review):
   such a pawn is always a target, and the shot spent on them is cure only, so nothing stands
   up; the message names the pawn.
5. **Self-target is allowed** (3.3): a medic who cannot inject themself is a worse medic, the
   walk is skipped, and the pilot's painlessness only trims what they gain from it.
6. **The autocast toil sits before the warmdown** (3.4), inside the stun, so the shot lands on
   a patient still held and the fade stays last.
7. **Autocast skips bed patients without a heart attack** (3.4). A toggle that jabbed every
   hospital patient would drain vials and sleep for nothing.

Open levers, each small:

1. **Warmup and range settings** for the shot: both def fields, one Tunable each; left out to
   keep the section to the four numbers that change play.
2. **A fleck on the patient**, one `fleckOnTarget` line when the art pass picks one; the sound
   pass in `TODOs.md` should also hear `Ingest_Inject` against the dressing's sounds.
3. **The taper.** Equal stages (go-juice's numbers at every level) are a three-line XML change;
   the taper is here so the fade is felt before the bill arrives.
4. **Autocast after a dry beam.** It fires now (the patient still has wounds, the stimulant
   still helps); skipping it would read "no medicine, no shot", which is not the fiction.

## 5. Localization surface

Drafts, no em dashes (player-facing):

- Ability `label` "adrenaline shot". `description`: "Inject a nearby person or animal, or the
  pilot, with a refined shot of go-juice. For a few hours they feel almost no pain and move, see
  and think faster, while their rest falls much faster than usual. The effect fades in levels.
  On a patient with a heart attack the shot is spent on the heart instead: it stops the attack
  outright and gives no surge.\n\nThe shoulders hold several
  shots per vial of go-juice, and reload once the last is spent. With the medic armor worn,
  right-click to give every patient a shot as their dressing ends." `jobReportString` "Giving
  an adrenaline shot."; the JobDef's `reportString` "giving an adrenaline shot.".
- Hediff `label` "adrenaline shot", `labelNoun` "an adrenaline shot", `description` "A refined
  shot of go-juice, fading in levels. Pain is dulled and movement, sight and thought are
  quickened, while the body burns through its rest faster and faster as the surge fades." Stage
  labels "rush",
  "fading", "wearing off".
- Shoulders: `chargeNoun` "shot" (vanilla prints "Shots remaining", "No shots left" and the refill
  line "1 go-juice"); vanilla's `cooldownGerund` default joins the DefInjected set as the armor's did.
  The `shortDescription` and first paragraph gain the injector: "fitted with field-medicine
  manipulators and an adrenaline injector." The shared paragraphs stay with the descriptions
  item in `TODOs.md`.
- Keyed, new: `MDWC_SectionShot` "Adrenaline shot"; `MDWC_Shots` "Shots per vial",
  `MDWC_ShotsDesc` "The number of shots the shoulders refine from one vial of go-juice. Shoulders
  already holding more than this lose the excess. Default: {0}."; `MDWC_ShotDuration` "Duration
  (hours)", `MDWC_ShotDurationDesc` "How long a shot lasts, fading in three levels. A shot
  already given keeps its own clock. Default: {0}."; `MDWC_RestFall` "Rest fall at a shot's start (x)",
  `MDWC_RestFallDesc` "How many times faster a patient's rest falls while a shot is fresh. Each
  level it fades through adds one. Sleep restores rest as usual. Default: {0}."; `MDWC_ShotCooldown` "Cooldown (seconds)", `MDWC_ShotCooldownDesc` "The time
  the pilot must wait between shots. Queued shots wait it out; the autocast skips a patient
  instead. Default: {0}."; `MDWC_ShotSummary` (four arguments, 3.7) and
  `MDWC_ShotSummaryNoCooldown` (three); `MDWC_NoShotsLoaded` "No shots loaded.";
  `MDWC_ShotHostile` "{0} is hostile."; `MDWC_ShotAlreadyRushing` "{0} is already running on
  adrenaline."; `MDWC_ShotsLabel` "Shots: {0} / {1}" (the cursor); `MDWC_ShotStopsHeartAttack`
  "Stops the heart attack" (the cursor's second line); `MDWC_HeartAttackStopped` "{0}'s heart attack
  has stopped." (the top-left message, `{0}` the patient's name); `MDWC_ShotAutocastDesc` "Autocast: a shot to each patient as their
  dressing ends." The warmup and range keys of the dressing are not reused: the shot's rows do
  not exist.
- DefInjected: `VEF.Abilities.AbilityDef/` gains `MDWC_AdrenalineShot` (`label`, `description`,
  `jobReportString`, `verbProperties.label`); `JobDef/` gains `MDWC_AdrenalineShot.reportString`;
  `HediffDef/` is a new folder with `MDWC_Adrenaline`'s `label`, `labelNoun`, `description` and
  `stages.N.label` (the probe decides whether the three stage labels are dumped); `ThingDef/` gains the shoulders' `comps.CompApparelReloadable.chargeNoun` and
  `cooldownGerund`. Regenerate `Scripts/expected-injections.json`. The translate skill's surface
  list, Keyed list and the glossary's charge-noun section gain "shot" beside "kit"; the
  `WordInfo` note for "kit" applies to "shot" in the same languages.
- Workshop page: the shoulders' bullet already says "Grants adrenaline shot"; a short section
  under the dressing's follows the landed code, per `.steamworkshop/CLAUDE.md`.

## 6. Verification

Dev mode, VFEP chain plus this mod, fresh saves:

1. **Settings.** The new section shows go-juice's icon and the shot's tooltip, four rows, and a
   summary reading "3 shots", "3 h", "2 times as fast at first", "and the next shot can follow at
   once". Set the
   cooldown to 20: the summary's last clause changes. Reset restores all four. Lower the shots
   to 2 on worn shoulders holding 3: the corner count reads "2 / 2".
2. **A shot.** Click the gizmo: the cursor shows go-juice's icon and "Shots: 3 / 3" over a
   colonist, the cross over a raider (downed or not) and over a pawn already at "rush", and
   "Stops the heart attack" as a second line over a dev-given heart attack, on a raider and on a
   "rush" patient too. Click the colonist:
   the pilot walks within 2.9 cells, holds them for 0.5 s, `Ingest_Inject` plays at the patient,
   the health tab shows "adrenaline shot (rush, 3h)" with pain ×0.1 and the three offsets, the
   corner count reads "2 / 3", the shoulders' inspect line "Shots remaining: 2 / 3", and no
   text mote appears. Click the pilot: no walk, the same result on the pilot, whose pain stays
   0.
3. **The comedown.** Speed time with the patient awake: the rest need falls about 0.08 an hour
   instead of 0.04 during "rush" (the Needs tab's fall rate reads x2 under the hediff's name),
   0.12 after the label turns "(fading, 2h)" at 1 h, 0.16 after "(wearing off, 1h)" at 2 h, and
   at 3 h the hediff is gone and the fall is normal again. Put a patient under a shot to bed:
   rest rises at the usual rate. Set the start to 1 in the settings while a shot runs: the
   fall drops to x1, x2, x3 at once. Re-shoot a
   patient at "fading": the label returns to "rush, 3h".
4. **The heart.** Dev-give a heart attack to a colonist, a downed raider and a patient at
   "rush": the shot removes it at any stage, the message "{name}'s heart attack has stopped."
   appears top-left and jumps to the pawn, a shot is spent, and **no** adrenaline hediff is
   applied (the "rush" patient's existing one is left as it was). The pilot's own heart attack
   is stopped the same way with no walk.
5. **Reload.** Spend all three: a colonist fetches one go-juice and the count returns to
   "3 / 3". At "1 / 3" nobody reloads; the float menu on a vial offers a reload that spends the
   vial and returns "3 / 3". The dev "Reload to full" button works.
6. **Autocast.** Without the armor the right-click line is absent and right-click does nothing.
   With the armor, right-click shows the check in the gizmo's **top-left** with the count still
   in the top-right, and the tooltip gains the autocast line. Dress a wounded, standing colonist:
   after the last wound's text the patient is held another 0.5 s, the shot lands, then the glow
   fades. Dress a bed patient: no shot; give them a heart attack first: the shot cures it and
   nothing else. Dress a downed raider: no shot; give them a heart attack: cured. Dress a patient at "rush": no shot. Empty the shoulders: no shot and
   no message. Save and reload: the check is still on and the shot still fires.
7. **Queue.** The dressing's queue test (`Mechanite-Dressing.md` 6.8) passes unchanged, and the
   same test on the shot: Shift-click three colonists, a waypoint between two of them, a plain
   click replacing the queue, a queued shot that starts with the shoulders empty failing at its
   check with nothing spent.
8. **Interruption.** Draft the pilot during a shot's warmup: nothing is spent and no hediff
   appears. Damage mid-warmup does not interrupt.
9. **Gates.** `python3 Scripts/integration-smoke-test.py` and
   `python3 Scripts/check-translations.py` are clean after the sidecar regen, and a Release build
   passes with warnings as errors.

## 7. Implementation checklist

1. Defs (3.1, 3.2): the shoulders' two comps, the ability, the job, the hediff; `MDWC_DefOf`
   fields.
2. The hoist (3.5): `Ability_MedicTargeted` and `JobDriver_MedicCast`, the dressing rebased on
   them with its headers trimmed to what is its own, the queue test re-run before anything new
   is added.
3. `Ability_AdrenalineShot` (3.3, 3.4), `JobDriver_AdrenalineShot` (3.5),
   `Command_AdrenalineShot` (3.6); the dressing driver's shot toil (3.4).
4. Settings (3.7): the section, four rows, the summary keys, the icon write, the trim.
5. Player-facing text (section 5); regenerate the sidecar.
6. Build, deploy, run section 6.
7. Supporting documentation, as a side effect of the landed code: `CLAUDE.md` (the overview's
   feature line and spec pointer, the project structure gaining `HediffDefs/`, the settings
   bullet gaining the shot's fields and the stage factors, the `Abilities/` line), `TODOs.md` (close the adrenaline
   item; add the first fill of one vial to the tuning item; the art item already names go-juice),
   `CHANGELOG.md` `[Unreleased]`, the Workshop description, `README.md`, the translate skill and
   its glossary.
