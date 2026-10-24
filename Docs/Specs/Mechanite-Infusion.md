# SPEC: Mechanite infusion (rework of the field dressing)

Status: DESIGN, for review before implementation. Written 2026-10-25 against the installed
RimWorld 1.7.4985 rev1269 (`Assembly-CSharp.dll` via `ilspycmd`, Core XML and `Assets.zip`), the
1.6 Krafs reference 1.6.4871 for the `DefOf` checks, the VEF checkout at
`../VanillaExpanded/VanillaExpandedFramework/` (58c884b9, 2026-10-21), and the sibling
`../ToxinWarcasket/` for the gauge and reload-target precedent. VEF facts come from that source;
a reviewer should spot-check the ones this design leans on (2.1) against the Workshop `VEF.dll`
players run. 1.7 behaviour is taken to hold on 1.6, as Ludeon frames the update; the author
verifies by hand before release (`TODOs.md`).

This spec reworks the implemented field dressing (`Field-Dressing.md`). Once it lands it
supersedes that spec's sections 3.2 to 3.5 and the settings table in 3.6. The doctor passives
(3.1), the engine facts in its section 2 and the medic trait (`Medic-Work-Trait.md`) stand
unchanged. The name is **tentative**: section 3.10 keeps the identifier rename a separate,
droppable step.

## 0. The brief

| Request                                                                        | Section  |
| ------------------------------------------------------------------------------ | -------- |
| Green tinted beam, resurrect-style overlay, `Mote_ResurrectFlash`, healing crosses | 3.7  |
| A soft green glow in front of the pilot from the warmup through the beam, fading over a warmdown | 3.5, 3.7 |
| Tentative rename to "mechanite infusion"                                       | 1, 3.10  |
| Warmup (cast time) default 1 s                                                 | 3.5, 3.9 |
| Variable beam: one wound per second by default, so more wounds mean a longer beam and stun | 3.5 |
| A 1 s warmdown after the beam, separate from the cooldown                      | 3.5      |
| Default range 2.9                                                              | 3.9      |
| Medicine chosen in mod settings only, default glitterworld, no herbal          | 3.1      |
| The targeting cursor's medicine icon removed                                   | 3.8      |
| No medicine-based tint                                                         | 3.7      |
| Charging by vanilla's tend economy, spent per wound tended (severity budget)   | 2.4, 3.2, 3.6 |
| A tank gauge like Toxin's instead of the gizmo's corner count                  | 3.8      |
| Default cooldown 0 s, so infusions can be queued and waypointed                | 3.3, 3.9 |
| Sounds reviewed in a later session                                             | `TODOs.md` |

Readings of the brief that a reviewer should confirm are in section 4.

## 1. Fiction

The suite breaks loaded medicine kits down and seeds them into carrier mechanites. A soft glow
gathers at the suit's emitter as it primes. The pilot then streams the carriers into the patient
along a guided beam and steers them through the suit, wound by wound. They deliver what the kit
contained, then burn out.

This fiction explains each mechanic:

- **The kit's potency** is what the carriers can deliver. Vanilla's glitterworld kit already
  contains "nanite diagnostic and healing assisters".
- **A kit covers a fixed amount of injury**, as one vanilla treatment does.
- **The pilot's Medicine skill** matters because the pilot steers the carriers.
- **The helmet's diagnostic visor** finds diseases and infections.
- **The beam** needs range and a line of sight.
- **Herbal medicine** is too crude to seed.
- **The carriers tend, they do not heal.** They cannot rebuild tissue, so the ability stays below
  the mech serums and does not compete with them.

Def descriptions are still a `TODOs.md` item; section 5 has first drafts in this voice.

## 2. Verified engine facts (new to this spec)

### 2.1 VEF's cast path

- `Ability.StartAbilityJob` (`VEF/Abilities/Misc/Ability.cs:441`) ends the current job and
  `StartJob`s the cast job with `InterruptForced`. It never queues. It writes
  `CompAbilities.currentlyCasting` and `currentlyCastingTargets` on the pilot's comp **at order
  time** (`Comps/CompAbilities.cs:23-24`, scribed at 108-112). `CreateCastJob(params
  GlobalTargetInfo[])` (417) is virtual and is what calls it; `StartAbilityJob` is not virtual.
- `JobDriver_CastAbilityOnce` (`AI/JobDrivers/JobDriver_CastAbilityOnce.cs`) reads that comp state
  throughout:
  - `TryMakePreToilReservations` reads `currentlyCasting.def.reserveTargets`, and `GetReport`
    reads `currentlyCasting.def`.
  - `MakeNewToils` sets `job.playerForced = true` and builds a
    `Toils_General.Wait(GetCastTimeForPawn(), A)` wait toil with `PreWarmupAction` as a pre-init
    action, passing it to the ability's virtual `WarmupToil(Toil)`.
  - It then adds an instant cast toil (`atomicWithPrevious`) that sets
    `job.playerForced = !pawn.Drafted` and calls `currentlyCasting.Cast(currentlyCastingTargets)`.
  - A driver finish action calls `currentlyCasting.EndCastJob()` on **every** job end:
    completion, interruption or failure.

  A job queued and started later would therefore run against whatever ability and targets were
  ordered last. Toils a subclass yields after the base toils run after the cast toil, with the
  job no longer `playerForced` for a drafted pilot.
- `Ability.WarmupToil` (266): plays `def.warmupStartSound` at pre-init. On each tick it maintains
  `def.warmupSound` as a per-tick sustainer, which stops when the toil ends.
  `def.warmupMote` is one static mote near the caster, so it cannot follow a beam.
- `Ability.Cast` (535) sets `cooldown = TicksGame + GetCooldownForPawn()`, then runs goodwill,
  mod extensions, hediffs and cast effects, none of which this def uses. `IsEnabledForPawn` (67)
  reports the cooldown first. `Command_Ability`'s cooldown overlay
  (`Commands/Command_Ability.cs:57-60`) divides by `GetCooldownForPawn()`, but only while
  `cooldown > TicksGame`. A zero cooldown is safe because `ReconcileSettings` already caps a
  running cooldown to the new length before the gizmo draws.
- Targeting: `DoTargeting` calls `Find.Targeter.BeginTargeting(this)` for a pawn target (386).
  `OrderForceTarget` (853) writes `currentTargets[currentTargetingIndex]` and re-enters
  `DoTargeting`; `CreateCastJob` resets the index to -1. `MultiSelect` (933) is a non-virtual
  get-only auto-property, so it is always false.
- `AbilityModExtensions` (35) filters `def.modExtensions` to `AbilityExtension_AbilityMod`, so a
  plain `DefModExtension` on the ability def is ignored by VEF.
- `AbilityDef.icon` defaults to `BaseContent.BadTex` without an `iconPath`
  (`Defs/AbilityDef.cs:80`). The default `MouseAttachment` (899) returns `TexCommand.Attack` for
  `BadTex` and never consults `ValidateTarget`.

### 2.2 Vanilla jobs, targeting and drawing

- `Pawn_JobTracker.TryTakeOrderedJob(job, tag, requestQueueing)` queues the job when
  `KeyBindingDefOf.QueueOrder.IsDownEvent` (Shift by default) or `requestQueueing`. Otherwise it
  interrupts the current job and clears the queue. Vanilla's `RimWorld.Ability.QueueCastingJob`
  orders this way.
- `Targeter`: after a click, `if (targetingSource.MultiSelect && Event.current.shift)
  BeginTargeting(targetingSource, ...)` reopens the targeter, so Shift-clicks can chain. The
  reopen goes straight to `BeginTargeting` and bypasses VEF's `DoTargeting`, so at the next click
  VEF's `OrderForceTarget` would index `currentTargets[-1]`.
- `Pawn_JobTracker.StartJob` order: `MakeDriver`, `TryMakePreToilReservations`,
  `SetInitialPosture`, `Notify_Starting`, `SetupToils`, `ReadyForNextToil`. On load,
  `JobDriver.ExposeData` calls `SetupToils` at `PostLoadInit`: the toils are rebuilt and tick
  actions are re-added, but the current toil's init actions do not re-run.
- `Pawn_JobTracker.Notify_DamageTaken` only considers an override when `!curJob.playerForced` and
  the job def's `checkOverrideOnDamage` (an enum, default `Always`) is not `Never`.
- `StunHandler.StunFor(ticks, ...)` sets `stunTicksLeft = max(stunTicksLeft, ticks)`.
  `StopStun()` zeroes it outright, which would also cut an overlapping stun from elsewhere.
- `Graphic_Mote.DrawMote` draws `graphicColor * mote.instanceColor` with its alpha multiplied by
  `mote.Alpha`, so lowering `instanceColor.a` fades a maintained mote under code control.
- `MoteMaker.MakeInteractionOverlay(ThingDef, A, B, offsetA, offsetB)` returns a
  `MoteDualAttached`. `MoteMaker.MakeAttachedOverlay(Thing, ThingDef, offset, ...)` returns a
  `Mote`. `Mote.Maintain()` keeps a `needsMaintenance` mote alive. `FleckMaker.ThrowMetaIcon(cell,
  map, fleck)` throws a rising icon.
- `FleckDefOf.HealingCross` is Core. `ThingDefOf.Mote_ResurrectFlash` is `[MayRequireBiotech]`;
  `Mote_ResurrectAbility` has no `DefOf` field.
- `TendUtility.SortByTendPriority` sorts by `TendPriority` descending, then severity.
- Medicine tech levels (Core XML): herbal `Neolithic`, industrial `Industrial`, glitterworld
  `Ultra`.
- Core sounds with `SoundDefOf` handles: `MechSerumUsed`, `Interact_Tend` (a sustainer, with
  `Tend_Start`), `TechMedicineUsed`.

### 2.3 Gauge and reloadable

- `Gizmo_Slider` (vanilla's base for the refuel and launcher gauges) draws a title, a bar and a
  centred `BarLabel`. With `IsDraggable` it draws a draggable target that snaps to `Increments`
  steps. `GetBarThresholds()` (virtual) draws tick marks at the given fractions on either kind of
  bar. Its bar textures are made on first draw, so the owner keeps one instance. Toxin's
  `Gizmo_ToxTank` is the worked subclass. Its `CompToxTank` returns it from
  `CompGetWornGizmosExtra` and holds a scribed `targetCharges` (-1 = full). Its reload patches
  (`Patches/CompApparelReloadable_Reload.cs`) prefix `NeedsReload` (`remaining < target`) and
  `MaxAmmoNeeded` (`perCharge * (target - remaining)`). `ReloadFrom` and `MinAmmoNeeded` are left
  alone, since both go through the patched `NeedsReload`.
- `CompApparelReloadable`: with `ammoCountPerCharge` set, `NeedsReload` is `remaining != max`,
  `ReloadFrom` adds `min(stack / perCharge, max - remaining)`, and `MaxAmmoNeeded` is
  `perCharge * (max - remaining)`. `UsedOnce()` (virtual) decrements.
  `CompApparelVerbOwner_Charged`'s `CompInspectStringExtra` and `SpecialDisplayStats` are virtual
  and print the integer `LabelRemaining`. The props print "{CHARGENOUN_plural}" (max) and "Reload
  cost per {CHARGENOUN}", and they read `ammoDef` when the info card opens.

### 2.4 Vanilla's tend economy

`TendUtility.DoTend` spends **one medicine per treatment**, whatever its type.
`GetOptimalHediffsToTendWithSingleTreatment(patient, usingMedicine, ...)` picks what one
treatment covers:

- the top tend-priority hediff, always, whatever its size;
- if that hediff's def is `tendAllAtOnce` (Core: only `Asthma`), every other hediff of the same
  def;
- else if it is a `Hediff_Injury` **and medicine is used**, further injuries in priority order
  while the summed severity stays at or under **20** (a literal in that method). The scan skips
  any injury that does not fit and keeps going, so smaller ones can still fill the gap.

The medicine's type never enters the batch: it changes only the quality (potency, quality max).
Without medicine a treatment covers the one top hediff. A disease, an infection or a fresh
missing part (`Hediff_MissingPart` is not a `Hediff_Injury`) is never batched, so each takes a
whole medicine. `Medicine.GetMedicineCountToFullyHeal` simulates exactly this to decide how many
medicines a doctor carries.

For scale, an unarmoured hit's severity is roughly its damage. Vanilla bullets: autopistol 10,
assault rifle 11, revolver and LMG 12, charge rifle 16, shotgun 18, sniper 25. Two rifle hits
(22) overflow one treatment, so a kit covers about one gunshot, or two or three scratches and
burns.

### 2.5 Visual assets

- Core defines the resurrector serum's overlay `Mote_ResurrectAbility`: a `MoteAttached`
  maintained mote, `Graphic_PawnBodySilhouette`, colour (0.45, 0.85, 0.52), shader
  `MoteMultiplyAddScroll` scrolling `Cloudy_B` and `Cloudy_C` with a `Speckles` detail layer.
  Core also defines its finishing flash `Mote_ResurrectFlash`: `Graphic_MoteWithAgeSecs`,
  `RadiationDistortion_A/B`, `MoteMultiplyAddCircularGrayscale`, `growthRate` 2.5. Both are in
  `Data/Core/Defs/Effects/Mote_Visual.xml`.
- Core's `Mote_ActivatorCountdownGlow` is a soft maintained glow: `Things/Mote/BrightFlash` under
  `MoteGlow`, drawSize 1.3, already tinted green (135, 215, 40, 140).
- Biotech defines `Mote_GraserBeamBase`: `MoteDualAttached`, `MoteBeam` shader,
  `Things/Mote/GraserBeam` with `BeamSecondaryNoise_A/B` scrolling at -4 and 3, `_Intensity` 2,
  `scaleToConnectTargets`. Odyssey's beam repeater reuses the texture.
- `Assets.zip`'s `Resources/Textures/Things/Mote/` (Core) holds `GraserBeam`,
  `BeamSecondaryNoise_A/B`, `BrightFlash`, `Cloudy_A`, `Cloudy_B`, `Speckles`, `HealingCross`,
  `Transparent` and `RadiationDistortion_A/B`. **`Cloudy_C` was not found in any archive or loose
  file of the install**, although the vanilla overlay uses it. The shader types `MoteBeam`,
  `MoteGlow`, `MoteMultiplyAddScroll` and `MoteMultiplyAddCircularGrayscale` are in Core's
  `Misc/ShaderTypeDefs/ShaderTypes.xml`.
- **Not verified:** whether `MoteBeam` multiplies in `graphicData.color` (section 6).

## 3. Design

### 3.1 Medicine: one type, chosen in the mod settings

The suite holds **one** medicine type: the armor's `ammoDef`, written by a mod setting (3.9). The
def XML's `ammoDef` becomes `MedicineUltratech` (glitterworld), and that is the default. There is
no in-game switching.

- **Choices:** every `ThingDef.IsMedicine` with `techLevel >= TechLevel.Industrial`, ordered by
  `MedicalPotency` descending. In vanilla that is industrial and glitterworld; modded medicines
  qualify by their own tech level.
- **A changed setting keeps the loaded count.** Kits are a vanilla charge count with no type of
  their own, so the kits already loaded count as the new medicine, and the next reload fetches
  it. The setting's tooltip says so.
- **Stock drain:** colonists reload the suite to its gauge's target (3.8) whenever the medicine
  is reachable, by default five glitterworld kits per medic. The gauge's target, or the
  industrial setting, conserves glitterworld stock. The free first fill on creation is five
  glitterworld kits (250 silver at market value), for the tuning pass to price in.

### 3.2 The suite: kits and a severity budget

The vanilla charge is a **kit**, one medicine item (`ammoCountPerCharge` 1, `chargeNoun` "kit").
Each kit treats **20 severity**, vanilla's per-treatment cap (2.4). The amount is a new props
field `severityPerKit` (default 20) on `CompProperties_InfusionSuite :
CompProperties_ApparelReloadable`.

- **State.** `remainingCharges` counts the kits loaded, **including the one in use**. A scribed
  float `openUsed` (0 ≤ `openUsed` < `severityPerKit`, 0 while no kit is loaded) is the severity
  already drawn from that kit. What is loaded, in kits, is
  `Available = remainingCharges - openUsed / severityPerKit`. Because the open kit still counts
  as a charge, vanilla's capacity and reload arithmetic never overfill and never waste a
  partly used kit.
- **Cost of tending one hediff:** a `Hediff_Injury` costs `min(severity, severityPerKit)`. Any
  other tendable hediff (disease, infection, fresh missing part) costs a whole kit. A
  `tendAllAtOnce` hediff costs a kit for the first of its def in an infusion and nothing for the
  rest. This is vanilla's accounting without its batching waste: a kit is used to the last point
  instead of leaving what no injury fits, so an infusion never costs more medicine than vanilla's
  treatments would. Worked example: rifle hits of 12 and 11, a 4-severity scratch and the flu cost
  vanilla 3 kits (12+4, then 11, then the flu). Here they cost 27/20 + 1 = 2.35.
- **`Draw(cost)`** draws from the open kit. When it reaches `severityPerKit`, it calls `UsedOnce()`
  and resets `openUsed`, and any remaining cost carries into the next kit. A tend is allowed
  whenever a kit is loaded, and a shortfall on the last kit is forgiven, as vanilla never refuses
  the top wound to its last medicine. A small epsilon closes a kit against float drift.
- **Reload to a target**, Toxin's way: a scribed `targetKits` (-1 = full, clamped to
  `MaxCharges`), set by dragging the gauge. A new `Patches/CompApparelReloadable_Reload.cs`
  mirrors Toxin's file, gated on `CompInfusionSuite`:
  - `NeedsReload` returns `remainingCharges < TargetKits`;
  - `MaxAmmoNeeded` returns `TargetKits - remainingCharges`;
  - `AmmoDef` is right unpatched, now that it is the props' own field.

  An open kit counts toward the target, so the suite reloads only once a whole kit is used up.
  The old AmmoDef/ReloadFrom file, `CompApparelReloadable_Suite.cs`, is deleted.
- **Overfill trim** stays as now: on a lowered `maxCharges`, clamp `remainingCharges` from
  `TrimWornSuites`, `Notify_Equipped` and `PostLoadInit`, and zero `openUsed` when no kit is left.
- **Readouts:** `CompInspectStringExtra` is overridden to print "Kits loaded: 2.6 / 5".
  `SpecialDisplayStats` is overridden to show the same fractional count in place of vanilla's
  integer, plus a "Severity per kit: 20" entry. The vanilla props lines read "Kits: 5" and
  "Reload cost per kit: 1 glitterworld medicine".
- Everything per-type from the dressing goes: the buckets, `Selected`/`Select`, `Reconcile`,
  `MoveLoaded`, `Types` and the per-type scribe.

### 3.3 Ordering and queueing

- **`CreateCastJob` override** (no base call): make the job (`def.jobDef`, target A = the
  patient) and hand it to `pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc)`. A plain click
  interrupts and clears the queue, as VEF's path did. A Shift-click appends, so Shift-right-click
  move orders interleave as waypoints. Reset `currentTargetingIndex = -1` and `currentTargets`,
  as the base does. The def has no `AbilityExtension_AbilityMod`s and is never cast from a
  caravan, so skipping `PreCast`/`StartAbilityJob` loses nothing.
- **`OrderForceTarget` override:** call `CreateCastJob(target)` directly, so a chained Shift-click
  from the reopened targeter never indexes `currentTargets[-1]` (2.2). The
  `CreateCastJob(LocalTargetInfo)` entry stays the call site, since VEF comments it as a
  Multiplayer hook point. Multiplayer is not a supported target here.
- **Chained targeting:** re-implement `ITargetingSource` on the subclass with
  `bool ITargetingSource.MultiSelect => true`. The other interface members map to the inherited
  public ones, so with Shift held the targeter stays open after each patient.
- **The job binds its own ability.** The driver's `TryMakePreToilReservations` runs first in
  `StartJob` (2.2). It finds the pilot's infusion instance: the worn apparel's
  `CompAbilitiesApparel.GivenAbilities` entry whose def is the infusion's, via a new
  `MDWC_DefOf` field. It writes that instance into `CompAbilities.currentlyCasting` and
  `[job.targetA]` into `currentlyCastingTargets`, then calls the base. It returns false when the
  armor or ability is gone, which ends a queued job quietly. On load the comp's own scribe
  restores both fields. `GetReport` reads the infusion def's `jobReportString`.
- The gizmo checks medicine and cooldown only at order time. Queued orders reserve no medicine;
  one that starts with the suite empty fails at its check (3.4) with "No {medicine} loaded", and
  the queue moves on.

### 3.4 The job (`JobDriver_MechaniteInfusion`)

The JobDef gains `checkOverrideOnDamage` `Never`, so damage never interrupts any phase whatever
`playerForced` says (2.2). The beam toil also re-asserts `playerForced = true`, undoing VEF's
cast toil for a drafted pilot (2.1).

Toils, in order:

1. **Walk** (the current `GotoPatient`): path to touch the patient and stop once within
   `distanceToTarget` with a line of sight. It reads the infusion def directly.
2. **Await cooldown:** completes at once unless the cooldown is running, then waits facing the
   patient until it ends. This is reachable only from a queue, since the gizmo is disabled during
   a cooldown; with the default 0 s it never waits.
3. **Check** (instant): the patient is valid with something to treat and the suite holds a kit.
   Otherwise end the job `Incompletable` with a message. Nothing has been spent.
4. **Warmup:** VEF's wait toil, `castTime` (default 60 ticks), with the infusion's actions added
   through `WarmupToil` (3.5).
5. **Connect:** VEF's cast toil calls `Cast`, which connects the beam (3.5).
6. **Beam** (new, variable length): see 3.5.
7. **Warmdown** (new, `warmdownTicks`, default 60): see 3.5.

The driver's finish action, VEF's `EndCastJob()`, settles whatever the job leaves open.
Interruption works as `Field-Dressing.md` 2.4 describes, apart from damage (above): a player
order, a draft change, a mental break, the pilot downed, or the patient despawned.

### 3.5 The infusion

The timing fields are a new `DefModExtension` on the ability def,
`MedicWarcasket.InfusionExtension`: `ticksPerTend` (60) and `warmdownTicks` (60). VEF ignores it
(2.1). The infusion's state lives on the ability and is scribed in its `ExposeData`, as
`pendingMedicine` is today, since one infusion runs per pilot at a time. The state is: the
patient (reference), the medicine (by defName), the quality, the counts tended and found, ticks
into the current wound, and whether the beam connected. `pendingMedicine` and
`targetingMedicine` go.

- **Warmup** (toil 4):
  - At init, the glow appears in front of the pilot (3.7), with `MechSerumUsed` (XML
    `warmupStartSound`).
  - Each tick, the glow is maintained and the patient's stun refreshed (below). The XML
    `warmupSound` (`Interact_Tend`) loops, maintained by VEF.
  - The range and line-of-sight check runs; a patient carried off ends the job `Incompletable`,
    with nothing spent and no cooldown.
- **Connect** (`Cast`, no `base.Cast`): recheck that something is tendable. If nothing is, skip
  straight to the warmdown, as someone else finished the patient during the warmup. Otherwise:
  - compute the quality once with `CalculateBaseTendQuality(pilot, patient, medicine)`, so the bed
    offset and self-tend factor come with it;
  - spawn the beam and the overlay (3.7);
  - play `TechMedicineUsed` once when the medicine beats industrial potency, as vanilla's
    `DoTend` does.

  No medicine is spent yet. The cooldown is set when the job ends, not here.
- **Beam** (toil 6), one wound per `ticksPerTend`:
  - Each tick: maintain the beam, the overlay, the glow, the stun and the tend loop sustainer (the
    def's `warmupSound`). Check range and line of sight; on failure the beam breaks.
  - Each time `ticksPerTend` elapses: re-run `FillTendable` (injuries and fresh missing parts,
    plus every other tendable hediff with the helmet worn, in tend-priority order). Tend its first
    entry with `Tended(quality, maxQuality, 1)`; batch position 1 keeps suppressing vanilla's
    per-hediff text. Then `Draw` its cost (3.2) and grant Medicine XP for that cost (below), and
    throw one `HealingCross` at the patient.

    The **first wound lands one interval after the beam connects**, so a beam over `n` wounds
    runs `n × ticksPerTend`. Each wound is paid for as it is tended: an interrupted beam has
    spent exactly what it tended. A wound taken mid-beam joins the queue by priority and lengthens
    the beam. The beam runs as long as anything is tendable and a kit is loaded.
  - The beam ends when nothing is left to tend (**complete**), when the suite runs dry with work
    left (**dry**), or when range or sight is lost (**broken**). All three show their outcome text
    on the patient at once and go on to the warmdown. A complete beam also spawns the flash.
  - The toil's progress bar is tended / (tended + still tendable).
- **Warmdown** (toil 7): the pilot holds still. The beam and overlay are no longer maintained and
  fade on their own (`fadeOutUnmaintained`). The stun is no longer refreshed and lapses. The glow
  is still maintained while its `instanceColor` alpha falls linearly to 0 over `warmdownTicks`
  (2.2), so the fade lasts exactly the warmdown at any setting. The warmdown is part of this job
  and separate from the ability's cooldown, which starts after it.
- **Settling** (`EndCastJob`, every job end):
  - if the beam connected, set `cooldown = TicksGame + GetCooldownForPawn()`;
  - if the job was interrupted during the beam, show "Interrupted: {n} of {m} wounds tended" when
    the patient is still spawned;
  - if at least one wound was tended, increment the tend records, the guest counter and the quest
    signal once;
  - clear the state.

  An interruption during the warmdown, or after a beam that ended on its own, only cuts the fade
  short. Guard against a dead or despawned pilot or patient.
- **XP: 60 per kit's worth of severity drawn**, 3 per point. That is the 60 of one base-length
  vanilla treatment, which is one kit (2.4). It is granted per tend, so partial results earn
  their share. It scales with medicine spent, not wound count, so chip damage cannot farm it,
  which was the old flat-per-cast rule's reason.
- **Stun, maintained** from the warmup's first tick through the beam's last: each tick refreshes
  `StunFor(StunRefreshTicks, pilot, addBattleLog: false, showMote: false)` with a window of a few
  ticks. The stun therefore lasts the warmup plus `n` intervals and lapses shortly after on any
  exit, without `StopStun` (2.2). A sleeping patient is not woken, and a bed patient keeps the
  bed's offset, as now.
- **Save mid-infusion:** the job resumes in its toil with the tick actions re-added (2.2), and
  the scribed state carries on. Motes are not scribed; the tick recreates any that are missing,
  as VEF's own `warmupMote` code does.

### 3.6 Charging, from the player's side

A kit is spent as it is used, wound by wound, never in advance. One kit covers about one gunshot,
two or three smaller wounds, or one illness, as in vanilla. A glitterworld kit treats no more
than an industrial one, only better. The cursor shows a patient's cost before the click (3.8),
and the gauge shows what is left. With no herbal option and no doctor-only tending, every wound
the infusion tends costs medicine.

### 3.7 Visuals

Four own mote defs, one per file, under `1.6/Defs/Effects/`. They are copies, not
`ParentName`s: `Mote_GraserBeamBase` is a Biotech def, and the resurrect motes are not all
reachable through vanilla `DefOf`s (2.2). They depend only on Core textures and shaders, and all
share the overlay's green, (0.45, 0.85, 0.52). No colour follows the medicine.

- **`MDWC_Mote_InfusionGlow`** (`Mote_InfusionGlow.xml`), the emitter: a copy of
  `Mote_ActivatorCountdownGlow` (`BrightFlash`, `MoteGlow`) as a `MoteAttached`, with
  `needsMaintenance`, `fadeOutUnmaintained`, a short `fadeOutTime` for an interrupted job, the
  green at about 0.55 alpha, drawSize about 1 and no `drawOffset`. It is attached to the pilot at
  warmup start with `MakeAttachedOverlay(pilot, def, offset)`, where the offset is about 0.5
  cells toward the patient: "in front of the caster", fixed for the job since the pilot stands
  still. It is held through the beam and faded by code over the warmdown (3.5).
- **`MDWC_Mote_InfusionBeam`** (`Mote_InfusionBeam.xml`): `Mote_GraserBeamBase`'s fields
  verbatim plus `graphicData.color`. It is spawned at connect with `MakeInteractionOverlay(def,
  pilot, patient, glowOffset, zero)`, so the beam leaves from the glow, and maintained each beam
  tick. Two in-game checks: the tint takes, and the noise flows from pilot to patient. If the tint
  does not take, ship a green copy of the texture under `Textures/Things/Mote/MedicWarcasket/`. If
  the flow runs backwards, flip the two scroll speeds' signs.
- **`MDWC_Mote_InfusionOverlay`** (`Mote_InfusionOverlay.xml`): `Mote_ResurrectAbility`'s fields
  verbatim plus `fadeOutUnmaintained`. It is attached to the patient at connect and maintained
  each beam tick. It keeps vanilla's `Cloudy_C` unless the in-game check (2.5) shows it missing;
  then `Cloudy_A` takes that slot.
- **`MDWC_Mote_InfusionFlash`** (`Mote_InfusionFlash.xml`): `Mote_ResurrectFlash`'s fields
  verbatim, attached to the patient once when a beam completes, so the flash means "done".
- **Healing crosses:** vanilla `FleckDefOf.HealingCross` through `ThrowMetaIcon`, one per
  tended hediff.
- **Sound** is placeholder (3.5) until the sound review in `TODOs.md`.

New `MDWC_DefOf` fields: the four motes and the infusion's `AbilityDef` (3.3).

### 3.8 Gauge, gizmo and cursor

- **Gauge** (`Source/1.6/Gizmos/Gizmo_InfusionSuite.cs : Gizmo_Slider`), returned from the
  suite's `CompGetWornGizmosExtra` as one cached instance, Toxin's pattern (2.3):
  - `Title`: the loaded medicine's `LabelCap`.
  - `ValuePercent`: `Available / MaxCharges`. `BarLabel`: "2.6 / 5", kits to one decimal.
  - `GetBarThresholds()`: a tick at each kit boundary.
  - `IsDraggable` for the player's wearers. `Target` is `targetKits / MaxCharges`, with
    `Increments` = `MaxCharges`, so the handle snaps to whole kits.
  - Colours are the infusion green: fill (0.45, 0.85, 0.52), highlight about (0.70, 0.95, 0.74),
    drag marker about (0.28, 0.58, 0.34), as starting values. Toxin's reason for a dark marker
    applies: vanilla's pale one is lost against a light fill.
  - Tooltip (Keyed): the sealed kits, the open kit's remainder, the severity a kit treats, and the
    reload target.
- **Ability gizmo** (`Command_MechaniteInfusion`): the right-click menu goes, along with the
  disabled-button right-click workaround that existed only to reach it; a right-click now does
  nothing (VEF's auto-cast branch, with `CanAutoCast` false). `TopRightLabel` goes too, since the
  gauge replaces it. The icon stays the loaded medicine's `uiIcon`, read from the props, as a
  placeholder until the ability art in `TODOs.md` lands. The button is enabled whenever a kit is
  loaded and the cooldown is clear.
- **Cursor:** the `MouseAttachment` override keeps vanilla's no-shoot cross over a target that
  fails `ValidateTarget`. Over a valid one it returns null, so no medicine icon is drawn and the
  label stays. `ExtraLabelMouseAttachment` reads "{medicine}, quality {q}" and, on a second line,
  "{n} wounds, {k} kits". It adds "({a} loaded)" when the cost is more than the suite holds. The
  cost is 3.2's rule summed over the current tendable set.

### 3.9 Settings

Defaults stay in the def XML, read by `LoadDefaultsFromDefs` and written by `ApplyToDefs`.

| Setting               | Def field                                     | Default      | Range                   |
| --------------------- | --------------------------------------------- | ------------ | ----------------------- |
| Medicine              | suite `ammoDef`                               | glitterworld | eligible list (3.1)     |
| Kits held             | suite `maxCharges`                            | 5            | 1..20, step 1           |
| Warmup (s)            | ability `castTime` (ticks ÷ 60)               | 1            | 0..10, current bands    |
| Time per wound (s)    | `InfusionExtension.ticksPerTend` (ticks ÷ 60) | 1            | 0.25..5, step 0.25      |
| Cooldown (s)          | ability `cooldownTime` (ticks ÷ 60)           | 0            | 0..300, step 5          |
| Range (cells)         | ability `distanceToTarget`                    | 2.9          | **1.9..12.9**, step 1   |

`warmdownTicks` (60) and `severityPerKit` (20) are def fields without settings (section 4).

- **The range grid moves from 1.5..12.5 to 1.9..12.9.** On the old grid 2.9 sits between steps.
  The Tunable clears any value within half a step of the default, so 2.5 would snap back to 2.9
  and could never be chosen. RimWorld's x.9 radii put 2.9 on the grid. 1.9 reaches exactly the
  cells 1.5 does, adjacent ones including diagonals, so the touch option survives. The old
  minimum's rationale, VEF pathing with `PathEndMode.Touch` at 1.5 or less, no longer applies:
  our own walk always paths to touch. A saved override below 1.9 reads as 1.9.
- **The medicine row** is its own small control in `Settings/`, not part of `Tunable.cs`, which
  must stay byte-identical across the family:
  - It shows the label, then a button with the medicine's icon and label that opens a float menu
    of the eligible medicines, with "(default)" while unset.
  - It is saved by defName; a name that no longer resolves to an eligible medicine reads as the
    default. "Restore default settings" resets it too.
  - `TryGetDefs` finds the suite by `compClass` and the extension by type.
- **The section header icon** is the configured medicine's `uiIcon`, read on each draw.
- **Summary line:** "The suite holds up to {0} kits of {1}, each treating {2} severity of injuries
  or one illness. An infusion warms up for {3} s, then tends a wound every {4} s, with {5} s of
  cooldown after." A second key replaces the last clause when the cooldown is 0.
- `TrimWornSuites` and the `Generation` cooldown cap are unchanged. A medicine change needs no
  reconciliation (3.1). A setting change mid-infusion is read live on the next tick.

### 3.10 The rename (tentative, separate commit)

Labels, descriptions and Keyed values change with the mechanics (section 5). Identifiers change
in one mechanical commit of their own, so the rename can be dropped or reverted without touching
behaviour. It is free only before the first Workshop publish. After that, a defName change breaks
saves, since the ability is scribed by def, and only labels can safely change.

| Now                                                          | Renamed                                |
| ------------------------------------------------------------ | -------------------------------------- |
| `MDWC_FieldDressing` (AbilityDef, JobDef)                    | `MDWC_MechaniteInfusion`               |
| `AbilityDefs/FieldDressing.xml`, `JobDefs/FieldDressing.xml` | `.../MechaniteInfusion.xml`            |
| `Ability_`, `Command_`, `JobDriver_FieldDressing`            | `..._MechaniteInfusion`                |
| `CompFieldDressingSuite`                                     | `CompInfusionSuite` (DefInjected handle `comps.CompInfusionSuite.*`) |
| `MDWC_SectionDressing`, `MDWC_DressingSummary`               | `MDWC_SectionInfusion`, `MDWC_InfusionSummary` |
| `MDWC_NoWoundsToDress`                                       | `MDWC_NoWoundsToTend`                  |
| `DressingDefName` and other `Dressing` names in the settings | `Infusion...`                          |

The Tunable keys change only where the meaning did: `Doses` becomes `Kits`, and `TendInterval` is
new. `CastTime`, `Cooldown` and `Range` keep theirs, so those overrides survive. Development saves
made with the old defName lose the ability on load; test on fresh entombings. `Field-Dressing.md`
keeps its name as the record of what it specified.

## 4. Readings to confirm, and open levers

Readings of the brief:

1. **"Targeting cursor def icon removed"** is read as: no medicine icon at the cursor over a valid
   patient (label only), the no-shoot cross kept over an invalid one, and the ability gizmo
   keeping the medicine icon as a placeholder.
2. **"No herbal option"** is read as a rule: industrial tech level and up (3.1). The alternative is
   `MedicalPotency` at or above industrial's.
3. **"Warmdown (not ability cooldown?)"** is read as: a 1 s phase of this job, with the pilot held
   still while the glow fades and the queue paused. The cooldown, default 0, starts after it.
4. **The stun covers the warmup as well as the beam.** That holds the patient still for the beam
   to connect. "More wounds, longer stun" still holds: the stun lasts `castTime + n × ticksPerTend`.
5. **"So it can be queued"** is met for any cooldown: a queued infusion waits out a running
   cooldown at the patient (3.4).

Open levers, each small:

1. **Settings for `warmdownTicks` and `severityPerKit`.** Both are def fields, one Tunable away.
   `severityPerKit` is the economy's one dial; 20 is vanilla's.
2. **The beam's length under fire.** It runs while anything is tendable and a kit is loaded, so a
   patient taking steady fire holds the medic until the player intervenes or the kits run out.
   The alternative is a cap: only what was tendable at connect, plus at most a few new wounds.
3. **First wound timing.** One interval after the beam connects (3.5). Tending the first wound at
   connect instead shortens every beam by one interval and puts a cross on the first frame.
4. **Medicine change semantics.** Loaded kits carry over (3.1). The alternative is emptying every
   worn suite on a change, with no refund.
5. **Beam origin.** The glow's offset is fixed for the job. A per-facing offset to a shoulder or
   chest is an art-pass choice.
6. **Queued over-ordering.** Queued orders reserve no medicine (3.3). `ValidateTarget` could
   refuse once queued costs exceed what is loaded, at the cost of summing the queue.

## 5. Localization surface

Drafts, no em dashes (player-facing):

- Ability `label` "mechanite infusion". `description`: "Seed the suite's medicine into a batch of
  carrier mechanites and stream them into a person or animal nearby. The carriers tend the
  patient's wounds one at a time, most urgent first, at the quality the pilot's medical skill and
  the medicine allow. With the medic helmet worn, they treat diseases and infections as well. A
  kit of medicine is drawn on as each wound is tended, about one gunshot or one illness per kit,
  and the patient is held still while the stream runs." `jobReportString` "Infusing
  mechanites."; the JobDef's `reportString` "infusing mechanites."
- Suite `chargeNoun` "kit" (was "dose"): vanilla prints it as "Kits" and "Reload cost per kit".
  The plural-worker concerns in `Field-Dressing.md` section 5 now apply to "kit".
- Armor `shortDescription` and first paragraph: "fitted with a field-medicine suite" becomes
  "fitted with a mechanite infusion suite". Helmet: "lets the field dressing treat disease"
  becomes "lets the mechanite infusion treat disease". The shared paragraphs are otherwise left to
  the descriptions item in `TODOs.md`.
- Keyed, changed:
  - `MDWC_SectionInfusion` "Mechanite infusion".
  - `MDWC_CastTime` "Warmup (seconds)"; `MDWC_CastTimeDesc` "How long the suite primes before the
    stream reaches the patient. The patient is held still from the start. Default: {0}."
  - `MDWC_CooldownDesc` "The time the pilot must wait after an infusion before starting another.
    Queued infusions wait it out. Default: {0}."
  - `MDWC_RangeDesc` "How close the pilot walks to the patient before starting the stream, which
    also needs a clear line of sight. Default: {0}."
  - `MDWC_InfusionSummary` "The suite holds up to {0} kits of {1}, each treating {2} severity of
    injuries or one illness. An infusion warms up for {3} s, then tends a wound every {4} s, with
    {5} s of cooldown after."
  - `MDWC_NoWoundsToTend` "{0} has no wounds to tend."
- Keyed, new:
  - `MDWC_Medicine` "Medicine"; `MDWC_MedicineDesc` "The medicine the armor's suite is loaded
    with. Herbal medicine is too crude for the carriers. Kits already loaded count as the new
    medicine. Default: {0}."
  - `MDWC_Kits` "Kits held"; `MDWC_KitsDesc` "The number of medicine kits the armor's suite holds
    when full. A suite already holding more than this loses the excess. Default: {0}."
  - `MDWC_TendInterval` "Time per wound (seconds)"; `MDWC_TendIntervalDesc` "How long the stream
    spends on each wound or illness. A patient with more to treat keeps the stream, and the hold,
    going longer. Default: {0}."
  - `MDWC_InfusionSummaryNoCooldown`: the summary ending "..., and the next can start at once."
  - `MDWC_WoundsTendedPartial` "Interrupted: {0} of {1} wounds tended".
  - `MDWC_WoundsTendedDry` "Out of {0}: {1} of {2} wounds tended".
  - `MDWC_BeamBroken` "Stream broken: {0} of {1} wounds tended".
  - `MDWC_TargetCost` "{0} wounds, {1} kits"; `MDWC_TargetCostShort` "{0} wounds, {1} kits ({2}
    loaded)".
  - `MDWC_KitsLoaded` "Kits loaded: {0} / {1}".
  - `MDWC_StatSeverityPerKit` "Severity per kit"; `MDWC_StatSeverityPerKitDesc` "The total
    severity of injuries one kit of medicine treats, as one vanilla treatment does. A disease,
    infection or fresh missing part takes a whole kit."
  - `MDWC_GaugeTip` "{0} sealed kits and {1} of an open kit. A kit treats up to {2} severity of
    injuries, or one illness. Colonists reload this suite to {3} kits."
- Keyed, removed: `MDWC_MedicineOption`, `MDWC_LoadedDoses`, `MDWC_LoadedDosesEntry`,
  `MDWC_LoadedDosesNone`, `MDWC_Doses`, `MDWC_DosesDesc`.
- Keyed, unchanged: `MDWC_NoMedicineLoaded`, `MDWC_NothingToTreat`, `MDWC_HostileTarget`,
  `MDWC_WoundsTended`, `MDWC_TargetQuality`, `MDWC_SettingsCategory`, `MDWC_SettingsUnavailable`,
  `MDWC_Cooldown`, `MDWC_Range`, `MDWC_DefaultSuffix`.
- DefInjected: the motes and the extension carry no translatable text. The comp handle changes
  with the rename (3.10). Regenerate `Scripts/expected-injections.json`. The translate skill's
  Keyed list, DefInjected handles and glossary change: "field dressing" and "dress" go, "kit"
  becomes the charge noun, "dose" leaves the surface.

## 6. Verification

Dev mode, VFEP chain plus this mod, fresh saves:

1. **Settings.** The medicine row lists industrial and glitterworld and no herbal; glitterworld
   is the default. Switching changes the gauge title, the gizmo icon, the cursor label and the
   stat sheet's reload line; the loaded kits carry over, and the next reload fetches the new
   medicine. The range slider steps 1.9, 2.9, 3.9 and on to 12.9, every step choosable. The
   defaults are warmup 1 s, 1 s per wound, cooldown 0, range 2.9 and 5 kits. Reset restores all
   six.
2. **Gauge.** It shows "5.0 / 5" with four kit ticks. Drag the target to 3: the suite is not
   reloaded past 3. Spend 0.6 of a kit: no reload yet. Spend the rest of it: one kit is fetched.
   For a non-player wearer the gauge is not draggable.
3. **A full infusion.** Dev-wound a colonist with rifle hits of 12 and 11 and a 4-severity
   scratch:
   - The pilot stops within 2.9 cells with a line of sight. The cursor read "3 wounds, 1.4 kits"
     before the click.
   - The glow appears in front of the pilot with `MechSerumUsed`, and the patient is held. After
     1 s the green beam leaves the glow, its noise flowing toward the patient, and the silhouette
     overlay shows.
   - A cross rises at 1, 2 and 3 s after connect, each drawing its wound's severity from the
     gauge, which ends 1.35 kits lower.
   - The flash and "3 wounds tended" appear, then the glow fades over 1 s while the beam and
     overlay fade. The patient moves again after the beam, not after the warmdown.
   - Medicine XP rises by about 81.
4. **Interruption.** Draft the pilot 1.5 s into that beam: one wound is tended and paid for, the
   others are untouched and unpaid, and "Interrupted: 1 of 3" shows. The glow fades quickly
   rather than over the warmdown, and the stun ends within a few ticks.
5. **Running dry.** Load 0.3 kits and infuse the same patient: the first wound is tended (the
   shortfall forgiven), then the beam ends "Out of glitterworld medicine: 1 of 3", and the
   warmdown follows.
6. **Broken beam.** Have a third pawn rescue a downed patient mid-beam: the beam breaks, "Stream
   broken" shows, and the warmdown follows. Kill a patient mid-beam: the job ends with no errors.
7. **Damage.** Shoot a drafted pilot and an undrafted pilot mid-beam and mid-warmdown with dev
   tools: neither phase is interrupted.
8. **Queue.** Shift-click the gizmo, then Shift-click three patients: the targeter stays open.
   Add a Shift-right-click waypoint between two of them. The pilot works the queue in order,
   with a warmdown between patients. With the suite empty, the next queued infusion fails at its
   check and nothing is spent. A plain click mid-queue replaces the queue. With the cooldown at
   20 s, the second infusion waits at its patient.
9. **Save mid-beam and reload.** The glow, beam and overlay come back, the next wound lands on
   schedule, and nothing is charged twice.
10. **Helmet.** With the helmet, the flu is tended and costs a whole kit; without it, the flu is
    not tended. Infuse a downed raider and a pet; the pilot still cannot target themself.
11. **Assets.** The four motes load without texture or shader errors, `Cloudy_C` included, and
    the beam takes its tint. A failure here triggers the fallbacks in 3.7.
12. **Gates.** `python3 Scripts/integration-smoke-test.py` and
    `python3 Scripts/check-translations.py` are clean after the sidecar regen, and a Release build
    passes with warnings as errors.

## 7. Implementation checklist

1. Mote defs (3.7); the ability def's `InfusionExtension`, sounds, `castTime` 60,
   `cooldownTime` 0 and `distanceToTarget` 2.9; the JobDef's `checkOverrideOnDamage` `Never`;
   `MDWC_DefOf` fields.
2. Suite (3.2): the props subclass with `severityPerKit`, `ammoDef` `MedicineUltratech`,
   `chargeNoun` "kit", `openUsed`, `Draw`, `targetKits`, the trim and the readouts. Delete
   `Patches/CompApparelReloadable_Suite.cs` and add `Patches/CompApparelReloadable_Reload.cs`.
3. Gauge (3.8) in a new `Source/1.6/Gizmos/` folder.
4. Ability: ordering and chained targeting (3.3), the infusion phases, charging, XP and settling
   (3.5), the cursor (3.8). Remove the pending-medicine snapshot. Driver: the binding, the report,
   the cooldown wait, the check, the beam and the warmdown toils (3.3, 3.4). Gizmo simplified
   (3.8). Rewrite every header comment to the new present: the infusion's call paths go in the
   ability's and driver's headers, and the queue binding in the driver's.
5. Settings (3.9): the medicine row, the kits and time-per-wound rows, the range grid, the summary
   keys.
6. Player-facing text (section 5).
7. Build, deploy, run section 6.
8. The rename (3.10), as its own commit.
9. Documentation:
   - `CLAUDE.md`: the overview's spec pointer and feature line, the project structure (Abilities,
     Comps and the new Gizmos folder), the settings bullet (gaining `ammoDef` and the
     extension's `ticksPerTend`), the reload-queries bullet (its patch is now the reload target),
     and the localization lines naming the dressing.
   - `README.md`, `CONTRIBUTING.md`, the translate skill, and
     `.steamworkshop/Description/English.txt` (the "Field dressing" section and its right-click
     instruction).
   - `Scripts/check-translations.py`'s comments.
   - `TODOs.md`: the XP and first-fill lines in the tuning item, and the adrenaline shot's "when a
     field dressing completes".
   - A status line on `Field-Dressing.md` naming this spec as superseding its sections 3.2 to
     3.5 and the settings table in 3.6.
   - The sidecar regen and the CHANGELOG `[Unreleased]` entry.
