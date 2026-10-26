# SPEC: Mechanite dressing (rework of the field dressing)

Status: LOCKED 2026-10-25, ready for implementation. Written against the installed RimWorld
1.7.4985 rev1269 (`Assembly-CSharp.dll` via `ilspycmd`, Core XML, the 1.7 GLSL shaders and
every `Assets.zip`), the 1.6 Krafs reference 1.6.4871 for the `DefOf` checks, the VEF checkout
at `../VanillaExpanded/VanillaExpandedFramework/` (58c884b9, 2026-10-21), and the sibling
`../ToxinWarcasket/` for the gauge and reload-target precedent. Every VEF fact in 2.1 was
checked against the Workshop `VEF.dll` players run (the 1.6 and 1.7 assemblies of
2023507013) and holds on both. 1.7 behaviour is taken to hold on 1.6, as Ludeon frames the
update; the author verifies by hand before release (`TODOs.md`).

This spec reworks the implemented field dressing (`Field-Dressing.md`). Once it lands it
supersedes that spec's sections 3.2 to 3.5 and the settings table in 3.6. The doctor passives
(3.1), the engine facts in its section 2 and the medic trait (`Medic-Work-Trait.md`) stand
unchanged. The name is settled: "mechanite dressing", a dressing because the carriers cover
and tend, they do not rebuild, which keeps the ability below the mech serums in the player's
mind as well as in its mechanics. Section 3.10 has the identifier rename.

## 0. The brief

| Request                                                                                          | Section       |
| ------------------------------------------------------------------------------------------------ | ------------- |
| Green tinted beam, resurrect-style overlay, `Mote_ResurrectFlash`                                | 3.7           |
| A soft green glow in front of the pilot from the warmup through the beam, fading over a warmdown | 3.5, 3.7      |
| Rename to "mechanite dressing"                                                                   | 1, 3.10       |
| Warmup (cast time) default 1 s                                                                   | 3.5, 3.9      |
| Variable beam: one wound per second by default, so more wounds mean a longer beam and stun       | 3.5           |
| A 1 s warmdown after the beam, separate from the cooldown                                        | 3.5           |
| Default range 2                                                                                  | 3.9           |
| Medicine chosen in mod settings only, default glitterworld, herbal included                    | 3.1           |
| The targeting cursor's medicine icon removed                                                     | 3.8, 4        |
| No medicine-based tint                                                                           | 3.7           |
| Charging by vanilla's tend economy, spent per wound tended (severity budget)                     | 2.4, 3.2, 3.6 |
| Medicine XP at vanilla parity, granted as each wound is tended                                   | 2.4, 3.5      |
| A tank gauge like Toxin's instead of the gizmo's corner count                                    | 3.8           |
| Default cooldown 0 s; queueing and waypoints work at any cooldown                                | 3.3, 3.9, 4   |
| Sounds reviewed in a later session                                                               | `TODOs.md`    |

The readings the review settled are in section 4.

## 1. Fiction

The suite breaks loaded medicine kits down and seeds them into carrier mechanites. A soft glow
gathers at the suit's emitter as it primes. The pilot then streams the carriers onto the patient
along a guided beam and steers them through the suit, wound by wound. Each wound gets a
dressing of carriers that deliver what the kit contained, then burn out.

This fiction explains each mechanic:

- **The kit's potency** is what the carriers can deliver, up to a ceiling of their own. Vanilla's glitterworld kit already
  contains "nanite diagnostic and healing assisters".
- **A kit covers a fixed amount of injury**, as one vanilla treatment does.
- **The pilot's Medicine skill** matters because the pilot directs the mechanites.
- **The helmet's diagnostic visor** provides additional diagnostic assistance for all conditions, including diseases and infections.
- **The beam** needs range and a line of sight.
- **Industrial and herbal medicine** are offered as balance levers only, the fiction remains ultratech;
  vanilla's tend quality formula already makes them the weaker seeds
- **The carriers dress, they do not heal.** They cannot rebuild tissue, so the ability stays
  below the mech serums and does not compete with them. The name says so: a dressing is what a
  medic lays on a wound, a serum is what rebuilds it. Biotech's Coagulate, a mechanite-flavoured
  wound tend named by its effect, is the vanilla precedent for an ability named this way.

Def descriptions are still a `TODOs.md` item; section 5 has first drafts in this voice.

## 2. Verified engine facts (new to this spec)

### 2.1 VEF's cast path

- `Ability.StartAbilityJob` (`VEF/Abilities/Misc/Ability.cs:441`) ends the current job and
  `StartJob`s the cast job with `InterruptForced`. It never queues, and it never clears the
  pawn's job queue either (`EndCurrentJob` does not), so queued jobs resume after a VEF cast.
  It writes `CompAbilities.currentlyCasting` and `currentlyCastingTargets` on the pilot's comp
  **at order time** (`Comps/CompAbilities.cs:23-24`, scribed at 108-112). `CreateCastJob(params
GlobalTargetInfo[])` (417) is virtual and is what calls it, after the `AbilityExtension_AbilityMod`
  `Valid` checks and `PreCast`, both no-ops without such extensions; `StartAbilityJob` is not
  virtual. `CreateCastJob(LocalTargetInfo)` (414) forwards to it and `OrderForceTarget` is
  virtual.
- `JobDriver_CastAbilityOnce` (`AI/JobDrivers/JobDriver_CastAbilityOnce.cs`) reads that comp state
  throughout:
  - `TryMakePreToilReservations` (overridable) dereferences `currentlyCasting.def.reserveTargets`
    and, when set, reserves from the ability's own `currentTargets`, not the comp's targets;
    `GetReport` (overridable) reads `currentlyCasting.def`.
  - `MakeNewToils` (overridable) sets `job.playerForced = true` and builds a
    `Toils_General.Wait(GetCastTimeForPawn(), A)` wait toil with `PreWarmupAction` as a pre-init
    action, passing it to the ability's virtual `WarmupToil(Toil)`. The warmup length is fixed
    here, at `SetupToils`, so it is read at job start, not live.
  - It then adds an instant cast toil (`atomicWithPrevious`) that sets
    `job.playerForced = !pawn.Drafted` and calls `currentlyCasting.Cast(currentlyCastingTargets)`.
    Nothing ends the job after it: toils a subclass yields after `base.MakeNewToils()` run next,
    with the job no longer `playerForced` for a drafted pilot.
  - A driver finish action calls `currentlyCasting.EndCastJob()` on **every** job end:
    completion, interruption or failure. `FailOnDespawnedOrNull(A)` covers every toil.

  A job started later against stale comp state therefore runs against whatever ability and
  targets were bound last. Vanilla also calls the driver's `TryMakePreToilReservations` at order
  time (2.2), before the ordered job starts and before the current one ends.

- `Ability.WarmupToil` (266): plays `def.warmupStartSound` at pre-init, after the driver's
  `PreWarmupAction`. On each tick it maintains `def.warmupSound` as a per-tick sustainer held in
  a private field, so it stops when the toil ends and a later toil cannot take it over.
  `def.warmupMote` is one static mote at the caster, so it cannot follow a beam.
  `PreWarmupAction()` is virtual too.
- `Ability.Cast` (535, virtual) sets `cooldown = TicksGame + GetCooldownForPawn()` as its first
  line, then runs goodwill, mod extensions, hediffs and cast effects, none of which this def
  uses. `cooldown` is a public int scribed by the base `ExposeData`. `EndCastJob()` is virtual
  with an empty body. `IsEnabledForPawn` (67) reports the cooldown first; nothing else in VEF
  assumes `Cast` set it. `Command_Ability`'s cooldown overlay (`Commands/Command_Ability.cs:57-60`)
  divides by `GetCooldownForPawn()` in float, only while the gizmo is disabled and
  `cooldown > TicksGame`. A zero cooldown cannot throw, and `ReconcileSettings` caps a running
  cooldown to the new length before the gizmo draws.
- Targeting: `DoTargeting` calls `Find.Targeter.BeginTargeting(this)` for a pawn target (386).
  `OrderForceTarget` (853) writes `currentTargets[currentTargetingIndex]` and re-enters
  `DoTargeting`; `CreateCastJob` resets the index to -1. `MultiSelect` (933) is a non-virtual
  get-only auto-property that nothing assigns, so it is always false. `Ability` implements
  `RimWorld.ITargetingSource` implicitly through its public members; a subclass that re-lists
  the interface in its base list may re-implement one member explicitly (`bool
ITargetingSource.MultiSelect => true`), the rest mapping to the inherited members, and the
  vanilla `Targeter`, which holds the interface, sees the subclass value. Without the re-listing
  the explicit member fails to compile (CS0540).
- `Ability.OnGUI` draws the cursor through `GenUI.DrawMouseAttachment(Texture2D)`, which draws
  the texture with no null check, then `DrawAttachmentExtraLabel`. The default `MouseAttachment`
  (899, protected virtual) returns `TexCommand.CannotShoot` only for an invalid
  `LocalTargetInfo`, else `def.icon`, or `TexCommand.Attack` for `BadTex`; it never consults
  `ValidateTarget`, whose `showMessages` defaults to true. `AbilityDef.icon` defaults to
  `BaseContent.BadTex` without an `iconPath` (`Defs/AbilityDef.cs:80`); the field is `[Unsaved]`
  and loaded from `iconPath` during def loading, so startup code may overwrite it.
- `AbilityModExtensions` (35) filters `def.modExtensions` to `AbilityExtension_AbilityMod`, so a
  plain `DefModExtension` on the ability def is ignored by VEF.
- `CompAbilitiesApparel.GivenAbilities` is a public `List<Ability>` scribed deep, each entry
  with its C# class and its def by name; `Ability.ExposeData` is virtual and `holder` is set
  back by the comp at load. `CompGetWornGizmosExtra` calls `ability.Init()` whenever the wearer
  differs from an unsaved field, so `Init()` runs again on the first gizmo draw after every
  load and must not reset scribed state.

### 2.2 Vanilla jobs, targeting and drawing

- `Pawn_JobTracker.TryTakeOrderedJob(job, tag, requestQueueing)` sets `job.playerForced` and
  returns at once when the order is the job already running (`JobIsSameAs`). The queue key
  (`KeyBindingDefOf.QueueOrder.IsDownEvent`, Shift by default) or `requestQueueing` appends the
  job to the queue. Otherwise, when the current job is player-interruptible and not
  `forceCompleteBeforeNextJob`, it **clears the queue**, puts the job at its head and ends the
  current job `InterruptForced`; when it is not, it clears the queue and appends instead. The
  queue key on an idle pawn interrupts. **Both paths call `job.TryMakePreToilReservations` at
  order time**, through the job's cached driver, before anything else happens; `StartJob` calls
  it again on the real driver when the job starts. Vanilla's `RimWorld.Ability.QueueCastingJob`
  orders this way.
- `RimWorld.Targeter`: on a left click it runs `targetingSource.ValidateTarget` first and bails
  on false, then `OrderForceTarget`, then `if (targetingSource.MultiSelect &&
  Event.current.shift) BeginTargeting(targetingSource, ...)` reopens the targeter with the same
  interface overload VEF uses (no callbacks of its own; drawing goes through the source's
  `OnGUI`), so Shift-clicks can chain. The reopen bypasses VEF's `DoTargeting`, so at the next
  click VEF's `OrderForceTarget` would index `currentTargets[-1]`. The chain test is the literal
  Shift key while the queue test is the keybinding: a rebound queue key splits the two.
- `Pawn_JobTracker.StartJob` order: `curJob = newJob`, `MakeDriver`,
  `TryMakePreToilReservations` (quiet when `fromQueue`; a false there ends the job
  `QueuedNoLongerValid`), `SetInitialPosture`, `Notify_Starting`, `SetupToils`,
  `ReadyForNextToil`. On load, `JobDriver.ExposeData` calls `SetupToils` at `PostLoadInit`: the
  toils are rebuilt and tick actions re-added, but the current toil's init actions and
  `Notify_Starting` do not re-run, and closure-local state in `MakeNewToils` is fresh.
- `Pawn_JobTracker.Notify_DamageTaken` only considers an override when `!curJob.playerForced` and
  the job def's `checkOverrideOnDamage` (`CheckJobOverrideOnDamageMode`, default `Always`) is
  not `Never`.
- `RimWorld.StunHandler.StunFor(ticks, instigator, addBattleLog, showMote, disableRotation)` sets
  `stunTicksLeft = max(stunTicksLeft, ticks)` and overwrites the mote and rotation flags every
  call. `StopStun()` zeroes it outright, which would also cut an overlapping stun from elsewhere.
  **`StunHandlerTick` zeroes the stun every tick on a downed pawn**, so a downed patient cannot
  be held by it. A stun neither wakes a sleeper nor interrupts a job; it only makes
  `stances.FullBodyBusy` true, which blocks toil transitions and pathing.
- `Graphic_Mote.DrawMote` draws `graphicColor * mote.instanceColor` with its alpha multiplied by
  `mote.Alpha`, so lowering `instanceColor.a` fades a maintained mote under code control.
  `Graphic_PawnBodySilhouette` ignores `instanceColor`. A maintained mote still dies at
  `fadeIn + solidTime + fadeOut` seconds of age, which is why vanilla's maintained defs carry a
  huge `solidTime`; one with `fadeOutUnmaintained` fades over `fadeOutTime` from its last
  `Maintain()`, one without is destroyed on the next tick.
- `MoteMaker.MakeInteractionOverlay(ThingDef, A, B, offsetA, offsetB)` returns a
  `MoteDualAttached`; with `scaleToConnectTargets` it stretches between the two anchors.
  `MoteMaker.MakeAttachedOverlay(Thing, ThingDef, offset, scale, solidTimeOverride)` returns a
  `Mote` that follows the thing (world-space offset). `Mote.Maintain()` keeps a `needsMaintenance`
  mote alive.
- `ThingDefOf.Mote_ResurrectFlash` is `[MayRequireBiotech]` although the def is Core XML;
  `Mote_ResurrectAbility` and `Mote_ActivatorCountdownGlow` have no `DefOf` field.
- `TendUtility.SortByTendPriority` sorts by `TendPriority` descending, then severity descending.
- Medicine tech levels (Core XML): herbal `Neolithic`, industrial `Industrial`, glitterworld
  `Ultra`. `ThingDef.IsMedicine` is "has a `MedicalPotency` stat base".
- Core sounds with `SoundDefOf` handles: `MechSerumUsed`, `Interact_Tend` (a sustainer whose
  `sustainStartSound` is `Tend_Start`, which has no handle of its own), `TechMedicineUsed`.

### 2.3 Gauge and reloadable

- `Verse.Gizmo_Slider` (vanilla's base for the refuel and launcher gauges) draws a title, a bar
  and a centred `BarLabel`. With `IsDraggable` it draws a draggable target that snaps to
  `Increments` steps (virtual, default 20). `GetBarThresholds()` (virtual, `IEnumerable<float>`)
  draws tick marks at the given fractions on either kind of bar. Its bar textures are made per
  instance on first draw, so the owner keeps one instance. **It reads `Target` once**, in that
  first draw, then owns the fraction and writes `Target` back every frame while draggable; a
  cached instance never sees a target or capacity changed elsewhere. Toxin's `Gizmo_ToxTank` is
  the worked subclass. Its `CompToxTank` returns it from `CompGetWornGizmosExtra` and holds a
  scribed `targetCharges` (-1 = full). Its reload patches
  (`Patches/CompApparelReloadable_Reload.cs`) prefix `NeedsReload` (`remaining < target`) and
  `MaxAmmoNeeded` (`perCharge * (target - remaining)`). `ReloadFrom` and `MinAmmoNeeded` are left
  alone, since both go through the patched `NeedsReload`.
- `CompApparelReloadable`: with `ammoCountPerCharge` set, `NeedsReload` is `remaining != max`,
  `ReloadFrom` adds `min(stack / perCharge, max - remaining)`, and `MaxAmmoNeeded` is
  `perCharge * (max - remaining)`; `MakeReloadJob` caps the carried count at `MaxAmmoNeeded`.
  `UsedOnce()` (virtual) decrements. `CompApparelVerbOwner_Charged`'s `CompInspectStringExtra`
  and `SpecialDisplayStats` are virtual and print the integer `LabelRemaining`. The props print
  "{CHARGENOUN_plural}" (max) **on the def's info card only**, and "Reload cost per {CHARGENOUN}"
  as "{count} {ammoDef.label}" (uncapitalised) on both, reading `ammoDef` when the card opens.

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
Without medicine a treatment covers the one top hediff, plus its `tendAllAtOnce` siblings. A
disease, an infection or a fresh missing part (`Hediff_MissingPart` is not a `Hediff_Injury`)
is never batched, so each takes a whole medicine. `Medicine.GetMedicineCountToFullyHeal`
simulates exactly this to decide how many medicines a doctor carries.

For scale, an unarmoured hit's severity is roughly its damage. Vanilla bullets: autopistol 10,
assault rifle 11, revolver and LMG 12, charge rifle 16, shotgun 18, sniper 25. Two rifle hits
(22) overflow one treatment, so a kit covers about one gunshot, or two or three scratches and
burns.

**Tend XP** is a lump, not a rate. `Toils_Tend.FinalizeTend` grants
`(animal ? 175 : 500) * medicine.MedicineTendXpGainFactor` once per treatment, where the
factor is `clamp(MedicalPotency * 0.7, 0.5, 1)`: 0.5 for no medicine or herbal, 0.7 for
industrial, 1 for glitterworld. The wait toil's `activeSkill` grants nothing per tick; it only
drives the passion thought. So one vanilla treatment on a humanlike is 250, 350 or 500 XP by
medicine. `DoTend` itself also rolls a 0.4% bond chance on an animal patient, calls
`Notify_SelfTended` on a self-tend and notifies the doctor's Ideology role.

### 2.5 Visual assets

- Core defines the resurrector serum's overlay `Mote_ResurrectAbility`: a `MoteAttached`
  maintained mote (`solidTime` 999999, no `fadeOutUnmaintained`), `Graphic_PawnBodySilhouette`,
  colour (0.45, 0.85, 0.52), shader `MoteMultiplyAddScroll` scrolling `Cloudy_B` and `Cloudy_C`
  with a `Speckles` detail layer. Core also defines its finishing flash `Mote_ResurrectFlash`:
  `MoteAttached`, `Graphic_MoteWithAgeSecs`, `RadiationDistortion_A/B`,
  `MoteMultiplyAddCircularGrayscale`, `_Intensity` 11, `growthRate` 2.5, drawSize 1.4. Both
  are in `Data/Core/Defs/Effects/Mote_Visual.xml`.
- Core's `Mote_ActivatorCountdownGlow` is a soft maintained glow: a `MoteThrown` with
  `solidTime` 999999999, `Things/Mote/BrightFlash` under `MoteGlow`, drawSize 1.3, `drawOffset`
  (0, 0, 1), already tinted green (135, 215, 40, 140).
- Biotech defines `Mote_GraserBeamBase`: `MoteDualAttached`, `needsMaintenance`,
  `fadeOutUnmaintained`, `rotateTowardsTarget`, `scaleToConnectTargets`, `MoteBeam` shader,
  `Things/Mote/GraserBeam` with `BeamSecondaryNoise_A/B` scrolling at -4 and 3, `_Intensity` 2,
  no colour. Odyssey's beam repeater reuses the texture.
- The root `Assets.zip`'s `Resources/Textures/Things/Mote/` (Core) holds `GraserBeam`,
  `BeamSecondaryNoise_A/B`, `BrightFlash`, `Cloudy_A`, `Cloudy_B`, `Speckles`, `HealingCross`,
  `Transparent` and `RadiationDistortion_A/B`. **`Cloudy_C` is in none of the six `Assets.zip`
  archives (Core and the five DLCs) and no loose file**, although the vanilla overlay names it.
  The 1.7 `MoteMultiplyAddScroll` shader defaults a missing `_MultiplyTexB` to black and
  multiplies the two cloud textures, so the cloud term goes dark and only the speckle layer
  shows. The shader types `MoteBeam`, `MoteGlow`, `MoteMultiplyAddScroll` and
  `MoteMultiplyAddCircularGrayscale` are in Core's `Misc/ShaderTypeDefs/ShaderTypes.xml`.
- The 1.7 `MoteMultiplyAddScroll` fragment shader (SPIR-V, read as in `CLAUDE.md`'s debugging
  notes) samples each layer at `coord * scale + scroll.xy * _AgeSecs`, where `coord` is the
  pawn-relative world (x, z) over the pawn's draw size from `_pawnCenterWorld` and
  `_pawnDrawSizeWorld`, and outputs `color * (texA * texB + detail)`. So a layer's pattern moves
  opposite to its scroll vector, in world space whatever the pawn faces, at `scroll / scale` draw
  heights per second: `(0, -s)` rises on screen. Its uniforms are `_texAScrollSpeed`,
  `_texBScrollSpeed` and Pascal-case `_DetailScrollSpeed`; the lowercase `_detailScrollSpeed`
  that `Mote_ResurrectAbility` writes binds nothing (`ShaderParameter.Apply` passes the XML name
  verbatim), so vanilla's speckles run at the 1.6 Unity shader's built-in (0.5, 0.5). The
  Archotech Android Hardware aura found the same direction rule and case bug in game on 1.6.
- The 1.7 `MoteBeam` fragment shader multiplies its output by `_Color`, and
  `Graphic_MoteWithAgeSecs` feeds it `graphicData.color * instanceColor` with the mote's alpha,
  so a tinted beam takes its tint on 1.7. The 1.6 Unity shader cannot be read here; section 6
  checks it in game.

## 3. Design

### 3.1 Medicine: one type, chosen in the mod settings

The suite holds **one** medicine type: the armor's `ammoDef`, written by a mod setting (3.9). The
def XML's `ammoDef` becomes `MedicineUltratech` (glitterworld), and that is the default. There is
no in-game switching.

- **Choices:** every `ThingDef.IsMedicine`, ordered by `MedicalPotency` descending: in vanilla
  glitterworld, industrial and herbal. A weak medicine needs no rule of its own, since the
  dressing's quality is vanilla's formula (herbal's 0.6 potency and 0.7 quality cap).
- **A changed setting keeps the loaded count.** Kits are a vanilla charge count with no type of
  their own, so the kits already loaded count as the new medicine, and the next reload fetches
  it. The setting's tooltip says so.
- **Stock drain:** colonists reload the suite to its gauge's target (3.8) whenever the medicine
  is reachable, by default ten glitterworld kits per medic. The gauge's target, or the
  industrial setting, conserves glitterworld stock. The free first fill on creation is ten
  glitterworld kits, the armor's costList MedicineUltratech 10.

### 3.2 The suite: kits and a severity budget

The vanilla charge is a **kit**, one medicine item (`ammoCountPerCharge` 1, `chargeNoun` "kit").
Each kit treats **20 severity**, vanilla's per-treatment cap (2.4). The amount is a new props
field `severityPerKit` (default 20) on `CompProperties_DressingSuite :
CompProperties_ApparelReloadable`.

- **State.** `remainingCharges` counts the kits loaded, **including the one in use**. A scribed
  float `openUsed` (0 ≤ `openUsed` < `severityPerKit`, 0 while no kit is loaded) is the severity
  already drawn from that kit. What is loaded, in kits, is
  `Available = remainingCharges - openUsed / severityPerKit`. Because the open kit still counts
  as a charge, vanilla's capacity and reload arithmetic never overfill and never waste a
  partly used kit.
- **Cost of tending one hediff:** a `Hediff_Injury` costs `min(severity, severityPerKit)`. Any
  other tendable hediff (disease, infection, fresh missing part) costs a whole kit. A
  `tendAllAtOnce` hediff costs a kit for the first of its def in a dressing and nothing for the
  rest. This is vanilla's accounting without its batching waste: a kit is used to the last point
  instead of leaving what no injury fits, so a dressing never costs more medicine than vanilla's
  treatments would. Worked example: rifle hits of 12 and 11, a 4-severity scratch and the flu cost
  vanilla 3 kits (12+4, then 11, then the flu). Here they cost 27/20 + 1 = 2.35.
- **`Draw(cost)`** draws from the open kit. When it reaches `severityPerKit`, it calls `UsedOnce()`
  and resets `openUsed`, and any remaining cost carries into the next kit. A tend is allowed
  whenever a kit is loaded, and a shortfall on the last kit is forgiven, as vanilla never refuses
  the top wound to its last medicine. A small epsilon closes a kit against float drift.
- **Reload to a target**, Toxin's way: a scribed `targetKits` (-1 = full, clamped to
  `MaxCharges`), set by dragging the gauge. A new `Patches/CompApparelReloadable_Reload.cs`
  mirrors Toxin's file, gated on `CompDressingSuite`:
  - `NeedsReload` returns `remainingCharges < TargetKits`;
  - `MaxAmmoNeeded` returns `TargetKits - remainingCharges`;
  - `AmmoDef` is right unpatched, now that it is the props' own field.

  An open kit counts toward the target, so the suite reloads only once a whole kit is used up.
  The old AmmoDef/ReloadFrom file, `CompApparelReloadable_Suite.cs`, is deleted.

- **Overfill trim** stays as now: on a lowered `maxCharges`, clamp `remainingCharges` from
  `TrimWornSuites`, `Notify_Equipped` and `PostLoadInit`, and zero `openUsed` when no kit is left.
- **Readouts:** `CompInspectStringExtra` is overridden to print "Kits loaded: 2.6 / 5".
  `SpecialDisplayStats` is overridden to show the same fractional count in place of vanilla's
  integer, plus a "Severity per kit: 20" entry. The vanilla props lines read "Kits: 5" on the
  def's info card only and "Reload cost per kit: 1 glitterworld medicine" on both cards (2.3).
- Everything per-type from the field dressing goes: the buckets, `Selected`/`Select`, `Reconcile`,
  `MoveLoaded`, `Types` and the per-type scribe.

### 3.3 Ordering and queueing

- **`CreateCastJob` override** (no base call): make the job (`def.jobDef`, target A = the
  patient) and hand it to `pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc)`. A plain click
  interrupts the current job and clears the queue, vanilla's order semantics and a change from
  VEF's path, which interrupted but left the queue standing (2.1); a click on the patient
  already being dressed is a no-op (2.2). A Shift-click appends, so Shift-right-click move
  orders interleave as waypoints. Reset `currentTargetingIndex = -1` and `currentTargets`, as
  the base does. The def has no `AbilityExtension_AbilityMod`s and is never cast from a
  caravan, so skipping `PreCast`/`StartAbilityJob` loses nothing.
- **`OrderForceTarget` override:** call `CreateCastJob(target)` directly, so a chained Shift-click
  from the reopened targeter never indexes `currentTargets[-1]` (2.2). The
  `CreateCastJob(LocalTargetInfo)` entry stays the call site, since VEF comments it as a
  Multiplayer hook point. Multiplayer is not a supported target here.
- **Chained targeting:** the subclass re-lists `RimWorld.ITargetingSource` in its base list and
  declares `bool ITargetingSource.MultiSelect => true`; the other interface members map to the
  inherited public ones (2.1), so with Shift held the targeter stays open after each patient.
  The targeter runs `ValidateTarget` on each click before ordering (2.2).
- **The job binds its own ability, at start only.** `TryMakePreToilReservations` runs at order
  time as well as at start (2.2), and at order time the pilot may be mid-dressing, or
  mid-cast of another VEF ability whose cast toil and finish action read the comp. So the
  override binds only when `pawn.CurJob == job`, which `StartJob` sets before calling it; at
  order time it returns true without touching the comp or calling the base, which would
  dereference a possibly null `currentlyCasting`. The binding finds the pilot's dressing
  instance, the worn apparel's `CompAbilitiesApparel.GivenAbilities` entry whose def is the
  dressing's (a new `MDWC_DefOf` field), writes it into `CompAbilities.currentlyCasting` and
  `[job.targetA]` into `currentlyCastingTargets`, then calls the base. It returns false when the
  armor or ability is gone, which ends a queued job quietly (`QueuedNoLongerValid`). On load the
  comp's own scribe restores both fields. `GetReport` is overridden to read the dressing def's
  `jobReportString` off the job's def, not the comp, since vanilla reports queued jobs through
  the cached driver too.
- The gizmo checks medicine and cooldown only at order time. Queued orders reserve no medicine;
  one that starts with the suite empty fails at its check (3.4) with "No {medicine} loaded", and
  the queue moves on. `reserveTargets` stays off, so a queued dressing does not keep a doctor
  from tending the same patient meanwhile.

### 3.4 The job (`JobDriver_MechaniteDressing`)

The JobDef gains `checkOverrideOnDamage` `Never`, so damage never interrupts any phase whatever
`playerForced` says (2.2). The beam toil also re-asserts `playerForced = true`, undoing VEF's
cast toil for a drafted pilot (2.1), so the melee-threat and hostility-response constant jobs,
which stand down for a player-forced job, leave the beam and warmdown alone.

Toils, in order:

1. **Walk** (the current `GotoPatient`): path to touch the patient and stop once within
   `distanceToTarget` with a line of sight. It reads the dressing def directly.
2. **Await cooldown:** completes at once unless the cooldown is running, then waits facing the
   patient until it ends. This is reachable only from a queue, since the gizmo is disabled during
   a cooldown; with the default 0 s it never waits.
3. **Check** (instant): the patient is valid with something to treat and the suite holds a kit.
   Otherwise end the job `Incompletable` with a message. Nothing has been spent.
4. **Warmup:** VEF's wait toil, `castTime` (default 60 ticks), with the dressing's actions added
   through `WarmupToil` (3.5). Its length is fixed when the job starts (2.1), so a cast-time
   change while a job is queued or walking applies from the next job.
5. **Connect:** VEF's cast toil calls `Cast`, which connects the beam (3.5).
6. **Beam** (new, variable length): see 3.5.
7. **Warmdown** (new, `warmdownTicks`, default 60): see 3.5.

The driver's finish action, VEF's `EndCastJob()`, settles whatever the job leaves open.
Interruption works as `Field-Dressing.md` 2.4 describes, apart from damage (above): a player
order, a draft change, a mental break, the pilot downed, or the patient despawned.

### 3.5 The dressing

The timing fields are a new `DefModExtension` on the ability def,
`MedicWarcasket.DressingExtension`: `ticksPerTend` (60) and `warmdownTicks` (60). VEF ignores it
(2.1). The dressing's state lives on the ability and is scribed in its `ExposeData`, as
`pendingMedicine` is today, since one dressing runs per pilot at a time. The state is: the
patient (reference), the medicine (by defName), the quality, the counts tended and found, ticks
into the current wound, and whether the beam connected. `pendingMedicine` and
`targetingMedicine` go. `Init()` runs again after every load (2.1), so it resets only the cached
suite reference, never this state.

- **Warmup** (toil 4):
  - At init, the glow appears in front of the pilot (3.7), with `MechSerumUsed` (XML
    `warmupStartSound`).
  - Each tick, the glow is maintained and the patient's stun refreshed (below). The XML
    `warmupSound` (`Interact_Tend`) loops, maintained by VEF.
  - The range and line-of-sight check runs; a patient carried off ends the job `Incompletable`,
    with nothing spent and no cooldown.
- **Connect** (`Cast`, no `base.Cast`): recheck that something is tendable. If nothing is, skip
  straight to the warmdown, as someone else finished the patient during the warmup. Otherwise:
  - compute the quality once with `CalculateBaseTendQuality(pilot, patient, potency, qualityMax)`,
    the medicine's potency clipped at the extension's `maxPotency` (1.3, no setting) so
    glitterworld saturates its quality cap at Medicine 8 rather than 6, so the bed
    offset and self-tend factor come with it;
  - spawn the beam and the overlay (3.7);
  - play `TechMedicineUsed` once when the medicine beats industrial potency, as vanilla's
    `DoTend` does.

  No medicine is spent yet. The cooldown is set when the job ends, not here.

- **Beam** (toil 6), one wound per `ticksPerTend`:
  - Each tick: maintain the beam, the overlay, the glow, the stun and the tend loop sustainer.
    The sustainer is the toil's own `Interact_Tend`, since VEF's lapses with the warmup toil
    (2.1); the restart at the seam is accepted until the sound review in `TODOs.md`. Check range
    and line of sight; on failure the beam breaks.
  - Each time `ticksPerTend` elapses: re-run `FillTendable` (injuries and fresh missing parts,
    plus every other tendable hediff with the helmet worn, in tend-priority order). Tend its first
    entry with `Tended(quality, maxQuality, 1)`; batch position 1 suppresses vanilla's
    per-hediff text, and the dressing throws its own in vanilla's words ("{wound} tended",
    "Quality {q}" with the quality that wound rolled), so each wound is announced as it lands.
    Then `Draw` its cost (3.2) and grant its XP (below).

    The **first wound lands one interval after the beam connects**, so a beam over `n` wounds
    runs `n × ticksPerTend`. `ticksPerTend` is the interval at a `MedicalTendSpeed` of 1.0: as
    vanilla's `JobDriver_TendPatient` divides its 600-tick base by the stat, the connect divides
    it by the pilot's `MedicalTendSpeed` floored at the extension's `minTendSpeed` (0.5, no
    setting), so skill and the shoulders speed the beam and an unskilled pilot runs at most twice
    the setting. Each wound is paid for as it is tended: an interrupted beam has
    spent exactly what it tended. A wound taken mid-beam joins the queue by priority and lengthens
    the beam. The beam runs as long as anything is tendable and a kit is loaded.

  - The beam ends when nothing is left to tend (**complete**), when the suite runs dry with work
    left (**dry**), or when range or sight is lost (**broken**). All three go on to the warmdown.
    Dry and broken show their outcome text on the patient at once; when a tend ended the beam,
    the outcome goes under that wound's text in the same mote, so the two never overlap. A
    complete beam shows no summary, since each wound announced itself.
  - The toil's progress bar is tended / (tended + still tendable).

- **Warmdown** (toil 7): the pilot holds still. The beam and overlay are no longer maintained and
  fade on their own (`fadeOutUnmaintained`). The stun is no longer refreshed and lapses. The glow
  is still maintained while its `instanceColor` alpha falls linearly to 0 over `warmdownTicks`
  (2.2), so the fade lasts exactly the warmdown at any setting. The warmdown is part of this job
  and separate from the ability's cooldown, which starts after it.
- **Settling** (`EndCastJob`, every job end):
  - if the beam connected, set `cooldown = TicksGame + GetCooldownForPawn()`. Because the
    cooldown starts here, the gizmo stays enabled through the beam and warmdown, and a plain
    click on another patient interrupts the running dressing, as any player order does;
  - if the job was interrupted during the beam, show "Interrupted: {n} of {m} wounds tended" when
    the patient is still spawned;
  - if at least one wound was tended, do once what `DoTend` does per treatment (2.4): increment
    the tend records, the guest counter and the quest signal, roll the 0.4% bond chance on an
    animal patient, and notify the pilot's Ideology role. `Notify_SelfTended` is unreachable,
    since the pilot cannot target themself;
  - clear the state.

  An interruption during the warmdown, or after a beam that ended on its own, only cuts the fade
  short. Guard against a dead or despawned pilot or patient.

- **XP, at vanilla parity.** Each tended hediff grants
  `(animal ? 175 : 500) × medicine.MedicineTendXpGainFactor × cost / severityPerKit`: the lump
  vanilla grants per treatment, by patient kind and medicine (2.4), scaled by the share of a kit
  the wound drew. A kit's worth of dressing therefore earns exactly one vanilla treatment's XP
  with that medicine (350 industrial, 500 glitterworld on a humanlike), an interrupted beam has
  earned what it tended, and a shortfall forgiven on the last kit still earns its cost's share.
  It scales with medicine spent, not wound count, so chip damage cannot farm it. The old flat 60
  matched nothing in vanilla and goes.
- **Stun, maintained** from the warmup's first tick through the beam's last, on a patient who is
  not downed: each tick refreshes `StunFor(StunRefreshTicks, pilot, addBattleLog: false,
  showMote: false)` with a window of a few ticks. The stun therefore lasts the warmup plus `n`
  intervals and lapses shortly after on any exit, without `StopStun` (2.2). A downed patient
  cannot be stunned and needs no holding (2.2), so the refresh skips them rather than overwrite
  another stun's flags every tick. A sleeping patient is not woken, and a bed patient keeps the
  bed's offset, as now.
- **Save mid-dressing:** the job resumes in its toil with the tick actions re-added (2.2), and
  the scribed state carries on. Motes are not scribed; the tick recreates any that are missing,
  as VEF's own `warmupMote` code does.

### 3.6 Charging, from the player's side

A kit is spent as it is used, wound by wound, never in advance. One kit covers about one gunshot,
two or three smaller wounds, or one illness, as in vanilla. A glitterworld kit treats no more
than an industrial one, only better. The cursor shows a patient's cost before the click (3.8),
and the gauge shows what is left. With no doctor-only tending, every wound the dressing tends
costs medicine.

### 3.7 Visuals

Three own mote defs, one per file, under `1.6/Defs/Effects/`. They are copies, not
`ParentName`s: `Mote_GraserBeamBase` is a Biotech def, and the resurrect overlay is not
reachable through a vanilla `DefOf` (2.2). They depend only on Core textures and shaders, and all
share one colour, the dressing's: by default vanilla's medical bed sheet,
`Building_Bed.SheetColorMedical` (0.388, 0.624, 0.886), a code constant whose value matches the
green it replaced (0.886 against 0.85, so the additive motes keep their brightness) while its
lower luminance (0.33 against 0.55) gives the gauge's white label more contrast. The tint is a
mod setting (3.9) written onto the three defs' `graphicData.color`, each keeping its own alpha,
with the old green (0.45, 0.85, 0.52) and a pink (vanilla's `ColorLibrary.Pink` at the blue's
value, (0.886, 0.448, 0.667)) as the alternatives. No colour follows the medicine. No finishing
flash: a completed beam ends with the last wound's text and the warmdown fade.

- **`MDWC_Mote_DressingGlow`** (`Mote_DressingGlow.xml`), the emitter:
  `Mote_ActivatorCountdownGlow`'s graphic (`BrightFlash`, `MoteGlow`) on a `MoteAttached` in
  place of its `MoteThrown`, keeping its huge `solidTime` (2.2), with `needsMaintenance`,
  `fadeOutUnmaintained`, a short `fadeOutTime` for an interrupted job, the colour at 0.65
  alpha, drawSize 1.5 and no `drawOffset`. It is attached to the pilot at warmup start with
  `MakeAttachedOverlay(pilot, def, offset)`, where the offset is about 0.5 cells toward the
  patient: "in front of the caster", fixed for the job since the pilot stands still. It is held
  through the beam and faded by code over the warmdown (3.5).
- **`MDWC_Mote_DressingBeam`** (`Mote_DressingBeam.xml`): `Mote_GraserBeamBase`'s fields
  verbatim plus `graphicData.color`. It is spawned at connect with `MakeInteractionOverlay(def,
  pilot, patient, glowOffset, zero)`, so the beam leaves from the glow, and maintained each beam
  tick. The tint is verified on 1.7 from the shader (2.5); two in-game checks remain: the tint on
  1.6, and that the noise flows from pilot to patient. If the 1.6 tint does not take, ship a
  tinted copy of the texture under `Textures/Things/Mote/MedicWarcasket/`. If the flow runs
  backwards, flip the two scroll speeds' signs.
- **`MDWC_Mote_DressingOverlay`** (`Mote_DressingOverlay.xml`): `Mote_ResurrectAbility`'s fields
  verbatim plus `fadeOutUnmaintained`, with `Cloudy_A` in the `_MultiplyTexB` slot: `Cloudy_C`
  ships in no archive and a missing slot blanks the cloud layer (2.5), and with every layer
  scrolling up the screen: `(0, -0.15, 0)` for both clouds, `(0, -0.5, 0)` for the speckles under
  the Pascal-case `_DetailScrollSpeed` (2.5), so the healing reads as rising whichever way the
  patient faces or lies. It is attached to the patient at connect and maintained each beam tick,
  and it fades only by lapsing, since its graphic ignores `instanceColor` (2.2).
- **Sound** is placeholder (3.5) until the sound review in `TODOs.md`.

New `MDWC_DefOf` fields: the three motes and the dressing's `AbilityDef` (3.3).

### 3.8 Gauge, gizmo and cursor

- **Gauge** (`Source/1.6/Gizmos/Gizmo_DressingSuite.cs : Gizmo_Slider`), returned from the
  suite's `CompGetWornGizmosExtra`, Toxin's pattern (2.3):
  - `Title`: the loaded medicine's `LabelCap`.
  - `ValuePercent`: `RemainingCharges / MaxCharges`, whole kits with the open one counted, since
    the reload target compares whole kits and a fractional fill would sit just under its own
    marker. `BarLabel`: "2.6 / 5", kits to one decimal, carries the fraction.
  - `GetBarThresholds()`: a tick at each kit boundary.
  - `IsDraggable` for the player's wearers. `Target` is `targetKits / MaxCharges`, with
    `Increments` = `MaxCharges`, so the handle snaps to whole kits. The setter maps the fraction
    back to whole kits idempotently, since the slider writes it every frame (2.3).
  - The comp caches one instance, keyed on `MaxCharges` and `MedicWarcasketSettings.Generation`,
    and builds a new one when either changes, since a `Gizmo_Slider` reads `Target` once (2.3)
    and would otherwise keep a stale target after a kits-held change or a clamp.
  - Colours are derived from the dressing's tint, read off the beam mote's def when the gauge is
    built: fill the tint, highlight the fill lerped 0.45 toward white, drag marker the fill at
    0.65 (for the default blue: (0.39, 0.62, 0.89), (0.66, 0.79, 0.94), (0.25, 0.41, 0.58)).
    Toxin's reason for a dark marker applies: vanilla's pale one is lost against a light fill.
  - Tooltip (Keyed): the sealed kits, the open kit's remainder, the severity a kit treats, and the
    reload target.
- **Ability gizmo** (`Command_MechaniteDressing`): the right-click menu goes, along with the
  disabled-button right-click workaround that existed only to reach it; a right-click now does
  nothing (VEF's auto-cast branch, with `CanAutoCast` false). `TopRightLabel` goes too, since the
  gauge replaces it, and so does the icon refresh: the gizmo shows `def.icon` as VEF's
  `Command_Ability` does unaided (below). The button is enabled whenever a kit is loaded and the
  cooldown is clear. If nothing is left to override, the class goes and the def's `gizmoClass`
  with it.
- **Icon and cursor follow VEF's idiom.** VEF and vanilla draw the ability's own icon at the
  cursor over a valid target and the no-shoot cross over an invalid one, and the gizmo shows the
  same icon (2.1). The medicine item icon at the cursor today is not a choice but the def's
  missing icon showing through a placeholder override, so the brief's "cursor icon removed" is
  met by giving the def an icon, which is the ability-art item in `TODOs.md`. Until that art
  lands, `ApplyToDefs` writes the configured medicine's `uiIcon` onto `def.icon` beside
  `ammoDef` (3.9), so the gizmo, the cursor and the settings header all show the medicine
  through VEF's default code with no override of their own, and the art replaces it by adding
  `iconPath`. The one override kept is `MouseAttachment`: `TexCommand.CannotShoot` when
  `ValidateTarget(target, showMessages: false)` fails, else the base. That is vanilla's own
  cursor rule, and `showMessages` must be passed, since VEF's default would spam the rejection
  message every frame. Returning null there is not an option: VEF draws the texture unchecked
  (2.1).
- **Cursor label:** `ExtraLabelMouseAttachment` reads "{medicine}, quality {q}" and, on a second
  line, "Wounds: {n}, kits: {k}", adding "({a} loaded)" when the cost is more than the suite
  holds. The cost is 3.2's rule summed over the current tendable set. `HidePawnTooltips` is
  on while targeting, as on Ideology's Convert: the hovered pawn's tooltip opens at the cursor
  over the label, and both flip with the screen edges, so no fixed offset separates them.

### 3.9 Settings

Defaults stay in the def XML, read by `LoadDefaultsFromDefs` and written by `ApplyToDefs`.

| Setting            | Def field                                     | Default      | Range                 |
| ------------------ | --------------------------------------------- | ------------ | --------------------- |
| Reload item        | suite `ammoDef`, and ability `icon` (3.8)     | glitterworld | eligible list (3.1)   |
| Effect color       | the three motes' `graphicData.color` (rgb)    | medical blue | blue, green, pink     |
| Kit capacity       | suite `maxCharges`                            | 10           | 1..20, step 1         |
| Casting time (s)   | ability `castTime` (ticks ÷ 60)               | 1            | 0..10, current bands  |
| Time per wound (s) | `DressingExtension.ticksPerTend` (ticks ÷ 60) | 1            | 0.25..5, step 0.25    |
| Cooldown (s)       | ability `cooldownTime` (ticks ÷ 60)           | 0            | 0..300, step 5        |
| Range              | ability `distanceToTarget`                    | 2            | 1.9, then 2..12 whole |

`warmdownTicks` (60) and `severityPerKit` (20) are def fields without settings (section 4).

- **The range grid is the touch stop, then whole cells: 1.9, 2, 3 and on to 12,** one band so
  the minimum sits off the grid. The walk and `InReach` compare `IntVec3.DistanceTo`, the
  straight-line cell distance (`GenMath.Sqrt` of the squared offsets), with `<=`, so a
  whole-number radius reaches the cells the game's own ring of that radius would, and the square
  root of a square is exact in float. 2 reaches two cells along a row or column and the diagonal
  neighbours (1.41), not the knight's cell (2.24) nor the corner two out (2.83); RimWorld's x.9
  radii would add those, 2.9 being the whole 5x5 block. The Tunable clears a value within half a
  step of the default, so the default must be a stop. 1.9 is touching, diagonals included; a
  minimum below 1.42 would refuse a diagonal neighbour, where a path to touch may already stand,
  and the walk would never pass its check. A saved override on an older grid (2.9, 3.9, ...) is
  kept as saved and applied as is until the slider is next touched, the Tunable's rule for every
  range move; 12.9 reads as the new end.
- **The medicine row** is its own small control in `Settings/`, not part of `Tunable.cs`, which
  must stay byte-identical across the family:
  - It shows the label, then a button with the medicine's icon and label that opens a float menu
    of the eligible medicines, with "(default)" while unset. No sibling has such a control;
    Toxin's `ToggleRow` is the row-layout precedent and its `Settings_Misc` the save, load,
    apply and reset hooks a non-slider setting needs.
  - It is saved by defName; a name that no longer resolves to an eligible medicine reads as the
    default. "Restore default settings" resets it too.
  - `TryGetDefs` finds the suite by `compClass` and the extension by type.
- **The effect color row** (`Settings/TintSetting.cs`), under the medicine's so the picked rows
  sit above the sliders, is a row of three radio buttons, each with a swatch, the default first
  and unmarked, saved by name; an unknown name reads as the default. "Default" means each mote's own
  XML colour, so a patch that recolours a mote moves the default. `ApplyToDefs` writes the rgb
  over each mote's `graphicData.color`, keeping that mote's alpha, then rebuilds the def's cached
  graphic: a mote reaches its graphic through `GraphicColoredFor`, which answers a colour the
  cache does not match with `Graphic_Single.GetColoredVersion`, a plain `Graphic_Single` without
  the def's `graphicClass` or `shaderParameters` (the beam's age-driven noise, the overlay's
  silhouette would be lost). `GraphicData.CopyFrom` onto itself is the one public way to clear
  the cache. Motes spawned after the write take the new colour; one in flight keeps its graphic.
- **The section header icon** is the configured medicine's `uiIcon`, the same texture
  `ApplyToDefs` wrote onto `def.icon`, read on each draw.
- **Summary line:** "The suite holds up to {0} kits of {1}, each treating {2} severity of injuries
  or one illness. A dressing takes {3} s to cast, then tends a wound every {4} s, with {5} s of
  cooldown after." A second key replaces the last clause when the cooldown is 0.
- `TrimWornSuites` and the `Generation` cooldown cap are unchanged. A medicine change needs no
  reconciliation (3.1). A setting change mid-dressing is read on the next tick, except the
  warmup length, which is fixed at job start (3.4).

### 3.10 The rename (own commit)

Labels, descriptions and Keyed values change with the mechanics (section 5). Identifiers change
in one mechanical commit of their own, for a readable diff. No save anywhere carries the old
names, so nothing needs a migration. For the record: both the defName and the C# class are
scribed with each granted ability (2.1), and a save with either missing fails to load the worn
armor, not just the ability, which is what makes a rename free only before the first Workshop
publish.

| Now                                                          | Renamed                                                                                                                                   |
| ------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `MDWC_FieldDressing` (AbilityDef, JobDef)                    | `MDWC_MechaniteDressing`                                                                                                                  |
| `AbilityDefs/FieldDressing.xml`, `JobDefs/FieldDressing.xml` | `.../MechaniteDressing.xml`                                                                                                               |
| `Ability_`, `Command_`, `JobDriver_FieldDressing`            | `..._MechaniteDressing`                                                                                                                   |
| `CompFieldDressingSuite`                                     | `CompDressingSuite` (DefInjected handle `comps.CompDressingSuite.*`), under `CompProperties_DressingSuite` as the armor entry's `Class` |

The Keyed keys that name the dressing (`MDWC_SectionDressing`, `MDWC_DressingSummary`,
`MDWC_NoWoundsToDress`) and the settings' `DressingDefName` keep their names, since the feature
is still a dressing. The Tunable keys change only where the meaning did: `Doses` becomes `Kits`,
and `TendInterval` is new. `CastTime`, `Cooldown` and `Range` keep theirs, so those overrides
survive. `Field-Dressing.md` keeps its name as the record of what it specified.

## 4. Readings settled by the review, and open levers

Readings of the brief:

1. **"Targeting cursor def icon removed"** is met the idiomatic way (3.8): the def gets an icon,
   the medicine's until the ability art lands, and VEF's default cursor and gizmo draw it. Hiding
   the cursor icon outright would have meant working around VEF's unchecked texture draw for a
   cosmetic difference.
2. **Herbal is an option** (3.1): any `IsMedicine` def qualifies, the row labelled "Reload
   item" as the shot's is.
3. **"Warmdown (not ability cooldown?)"** is a 1 s phase of this job, with the pilot held still
   while the glow fades and the queue paused. The cooldown, default 0, starts after it.
4. **The stun covers the warmup as well as the beam** on a conscious patient, so the beam can
   connect; a downed patient cannot be stunned and needs no holding (2.2). "More wounds, longer
   stun" holds: the stun lasts `castTime + n × ticksPerTend`.
5. **"So it can be queued"** is met for any cooldown: a queued dressing waits out a running
   cooldown at the patient (3.4).
6. **XP is vanilla parity**, not the old flat 60, which matched nothing in vanilla: the
   per-treatment lump, by patient kind and medicine, pro rata to the kit share each wound draws,
   granted as each wound is tended (3.5).
7. **The charge noun changes from "dose" to "kit"** in the def, the sidecar and every glossary
   (section 5).

Open levers, each small:

1. **Settings for `warmdownTicks` and `severityPerKit`.** Both are def fields, one Tunable away.
   `severityPerKit` is the economy's one dial; 20 is vanilla's.
2. **The beam's length under fire.** It runs while anything is tendable and a kit is loaded, so a
   patient taking steady fire holds the medic until the player intervenes or the kits run out.
   The alternative is a cap: only what was tendable at connect, plus at most a few new wounds.
3. **First wound timing.** One interval after the beam connects (3.5). Tending the first wound at
   connect instead shortens every beam by one interval and puts a wound's text on the first frame.
4. **Medicine change semantics.** Loaded kits carry over (3.1). The alternative is emptying every
   worn suite on a change, with no refund.
5. **Beam origin.** The glow's offset is fixed for the job. A per-facing offset to a shoulder or
   chest is an art-pass choice.
6. **Queued over-ordering.** Queued orders reserve no medicine (3.3). `ValidateTarget` could
   refuse once queued costs exceed what is loaded, at the cost of summing the queue.

## 5. Localization surface

Drafts, no em dashes (player-facing):

- Ability `label` "mechanite dressing". `description`: "Stream medicine-laden
  mechanites onto a nearby person or animal to tend their wounds, most urgent first. Tend quality
  depends on the pilot's medical skill and the medicine used. If the medic helmet is worn, the
  mechanites also treat diseases and infections.\n\nEach wound tended uses up medicine from the
  suite. The patient is held in place while the stream runs." `jobReportString` "Applying a
  mechanite dressing."; the JobDef's `reportString` "applying a mechanite dressing."
- **The charge noun changes: suite `chargeNoun` "kit", was "dose".** Vanilla prints it as "Kits"
  and "Reload cost per kit". The plural-worker concerns in `Field-Dressing.md` section 5 now
  apply to "kit", and every glossary's charge-noun line changes with it.
- Armor `shortDescription` and first paragraph: "fitted with a field-medicine suite" becomes
  "fitted with a mechanite dressing suite". Helmet: "lets the field dressing treat disease"
  becomes "lets the mechanite dressing treat disease". The shoulders' "field-medicine
  manipulators" stays: it names the shoulders' tooling, not the ability. The shared paragraphs
  are otherwise left to the descriptions item in `TODOs.md`.
- Keyed, changed:
  - `MDWC_SectionDressing` "Mechanite dressing".
  - `MDWC_CastTime` "Casting time"; `MDWC_CastTimeDesc` "How long the suite primes before the
    stream reaches the patient. The patient is held still from the start. Default: {0}."
  - `MDWC_CooldownDesc` "The time the pilot must wait after a dressing before starting another.
    Queued dressings wait it out. Default: {0}."
  - `MDWC_RangeDesc` "How close the pilot walks to the patient before starting the stream, which
    also needs a clear line of sight. Default: {0}."
  - `MDWC_DressingSummary` "The suite holds up to {0} kits of {1}, each treating {2} severity of
    injuries or one illness. A dressing takes {3} s to cast, then tends a wound every {4} s, with
    {5} s of cooldown after." Six arguments where there were three; the translate skill's note
    on its shape changes.
- Keyed, new:
  - `MDWC_Medicine` "Reload item"; `MDWC_MedicineDesc` "The medicine the armor's suite is loaded
    with, one per kit. Tend quality depends on its medical potency and tend quality maximum. Kits
    already loaded count as the new medicine. Default: {0}."
  - `MDWC_Kits` "Kit capacity"; `MDWC_KitsDesc` "The number of medicine kits the armor's suite holds
    when full. A suite already holding more than this loses the excess. Default: {0}."
  - `MDWC_TendInterval` "Time per wound"; `MDWC_TendIntervalDesc` "How long the stream
    spends on each wound or illness. A patient with more to treat keeps the stream, and the hold,
    going longer. Default: {0}."
  - `MDWC_DressingSummaryNoCooldown`: the summary ending "..., and the next can start at once."
  - `MDWC_WoundsTendedPartial` "Interrupted: {0} of {1} wounds tended".
  - `MDWC_WoundsTendedDry` "Out of {0}: {1} of {2} wounds tended".
  - `MDWC_BeamBroken` "Stream broken: {0} of {1} wounds tended".
  - `MDWC_TargetCost` "Wounds: {0}, kits: {1}"; `MDWC_TargetCostShort` "Wounds: {0}, kits: {1}
    ({2} loaded)". Label form, so a count of one needs no singular.
  - `MDWC_KitsLoaded` "Kits loaded: {0} / {1}".
  - `MDWC_StatSeverityPerKit` "Severity per kit"; `MDWC_StatSeverityPerKitDesc` "The total
    severity of injuries one kit of medicine treats, as one vanilla treatment does. A disease,
    infection or fresh missing part takes a whole kit."
  - The gauge's tooltip, laid out as vanilla's hemogen gauge under a "Medicine" header
    (`MDWC_Medicine`): `MDWC_GaugeOpenKit` "Open kit: {0} left"; `MDWC_GaugeReloadTarget`
    "Reload target: {0}"; `MDWC_GaugeNeverReload` "Never reload."; `MDWC_GaugeKitDesc` "Each kit
    treats up to {0} severity of injuries, or one disease, infection or fresh missing part."
- Keyed, removed: `MDWC_MedicineOption`, `MDWC_LoadedDoses`, `MDWC_LoadedDosesEntry`,
  `MDWC_LoadedDosesNone`, `MDWC_Doses`, `MDWC_DosesDesc`, `MDWC_WoundsTended`.
- Keyed, unchanged: `MDWC_NoWoundsToDress`, `MDWC_NoMedicineLoaded`, `MDWC_NothingToTreat`,
  `MDWC_HostileTarget`, `MDWC_TargetQuality`, `MDWC_SettingsCategory`,
  `MDWC_SettingsUnavailable`, `MDWC_Cooldown`, `MDWC_Range`, `MDWC_DefaultSuffix`. The file's
  header comment gains `Gizmos/`.
- DefInjected: the motes and the extension carry no translatable text. The handles change with
  the rename (3.10): `MDWC_MechaniteDressing.*` and `comps.CompDressingSuite.*`. Regenerate
  `Scripts/expected-injections.json`. The translate skill's Keyed list, DefInjected handles,
  summary-shape note and glossary change, and the nine per-language glossaries' "dose" line
  becomes the "kit" line: "field dressing" goes, "mechanite dressing" and "dress" stay, "kit"
  is the charge noun, "dose" leaves the surface.

## 6. Verification

Dev mode, VFEP chain plus this mod, fresh saves:

1. **Settings.** The reload item row lists glitterworld, industrial and herbal; glitterworld
   is the default. Switching changes the gauge title, the gizmo icon, the cursor icon and label,
   the section header icon and the stat sheet's reload line; the loaded kits carry over, and the
   next reload fetches the new medicine. The range slider steps 1.9, 2, 3 and on to 12, every
   step choosable. The defaults are warmup 1 s, 1 s per wound, cooldown 0, range 2 and 5
   kits. The effect color row shows blue, green and pink with their swatches; picking green
   and closing the window turns the gauge green at once and the next dressing's glow, beam and
   overlay with it, a dressing already running keeping its colour, and the beam keeps its
   flowing noise and the overlay its silhouette in every colour. Reset restores all seven.
2. **Gauge.** It shows "5.0 / 5" with four kit ticks. Drag the target to 3: the suite is not
   reloaded past 3. Spend 0.6 of a kit: no reload yet. Spend the rest of it: one kit is fetched.
   Lower the kit capacity to 4 in the settings: the gauge redraws at 4 with its target clamped. For a
   non-player wearer the gauge is not draggable.
3. **A full dressing.** Dev-wound a colonist with rifle hits of 12 and 11 and a 4-severity
   scratch:
   - The pilot stops within 2 cells with a line of sight: two along a row or column, or
     diagonally adjacent, never the knight's cell. The cursor showed the medicine icon
     and read "Wounds: 3, kits: 1.4" before the click, and the cross over the pilot and over a
     conscious raider, with no message spam.
   - The glow appears in front of the pilot with `MechSerumUsed`, and the patient is held. After
     1 s the blue beam leaves the glow, its noise flowing toward the patient, and the silhouette
     overlay shows with its cloud layer, clouds and speckles drifting up the screen whichever way
     the patient faces or lies.
   - A wound's text rises at 1, 2 and 3 s after connect, each drawing its wound's severity from the
     gauge, which ends 1.35 kits lower.
   - After the third wound's text the glow fades over 1 s while the beam and overlay fade. The
     patient moves again after the beam, not after the warmdown.
   - Medicine XP rises by 675 with glitterworld (500 × 27 / 20) and 472.5 with industrial.
4. **Interruption.** Draft the pilot 1.5 s into that beam: one wound is tended and paid for, the
   others are untouched and unpaid, "Interrupted: 1 of 3" shows and only that wound's XP was
   earned. The glow fades quickly rather than over the warmdown, and the stun ends within a few
   ticks.
5. **Running dry.** Load 0.3 kits and dress the same patient: the first wound is tended (the
   shortfall forgiven), then the beam ends "Out of glitterworld medicine: 1 of 3", and the
   warmdown follows.
6. **Broken beam.** Have a third pawn rescue a downed patient mid-beam: the beam breaks, "Stream
   broken" shows, and the warmdown follows. Kill a patient mid-beam: the job ends with no errors.
   A downed patient is dressed without a stun and without errors.
7. **Damage.** Shoot a drafted pilot and an undrafted pilot mid-beam and mid-warmdown with dev
   tools: neither phase is interrupted. Melee a drafted pilot mid-beam: the beam holds.
8. **Queue.** Shift-click the gizmo, then Shift-click three patients: the targeter stays open.
   Add a Shift-right-click waypoint between two of them. The pilot works the queue in order,
   with a warmdown between patients. With the suite empty, the next queued dressing fails at its
   check and nothing is spent. A plain click mid-queue replaces the queue; a plain click on the
   patient being dressed changes nothing. Shift-click a second patient during the first's
   warmup: the beam connects to the first patient. With the cooldown at 20 s, the second
   dressing waits at its patient.
9. **Save mid-beam and reload.** The glow, beam and overlay come back, the next wound lands on
   schedule, and nothing is charged twice.
10. **Helmet.** With the helmet, the flu is tended and costs a whole kit; without it, the flu is
    not tended. Dress a downed raider and a pet; the pilot still cannot target themself. The pet
    earns the animal lump (175 × factor per kit's worth).
11. **Assets.** The four motes load without texture or shader errors, and the beam takes its
    tint on 1.6 as it does on 1.7. A failure here triggers the fallbacks in 3.7.
12. **Gates.** `python3 Scripts/integration-smoke-test.py` and
    `python3 Scripts/check-translations.py` are clean after the sidecar regen, and a Release build
    passes with warnings as errors.

## 7. Implementation checklist

1. Mote defs (3.7); the ability def's `DressingExtension`, sounds, `castTime` 60,
   `cooldownTime` 0 and `distanceToTarget` 2; the JobDef's `checkOverrideOnDamage` `Never`;
   `MDWC_DefOf` fields.
2. Suite (3.2): `CompProperties_DressingSuite` with `severityPerKit` as the armor comp entry's
   `Class`, `ammoDef` `MedicineUltratech`, `chargeNoun` "kit", `openUsed`, `Draw`, `targetKits`,
   the trim and the readouts. Delete `Patches/CompApparelReloadable_Suite.cs` and add
   `Patches/CompApparelReloadable_Reload.cs`.
3. Gauge (3.8) in a new `Source/1.6/Gizmos/` folder, rebuilt on capacity or generation change.
4. Ability: ordering and chained targeting (3.3), the dressing phases, charging, XP and settling
   (3.5), the cursor rule and label (3.8). Remove the pending-medicine snapshot. Driver: the
   start-only binding, the report, the cooldown wait, the check, the beam and the warmdown toils
   (3.3, 3.4). Gizmo stripped to nothing, or deleted with its `gizmoClass` (3.8). Rewrite every
   header comment to the new present: the dressing's call paths go in the ability's and
   driver's headers, the queue binding and its order-time trap in the driver's, and the three
   piece defs' and both def files' headers lose the per-type suite and the old timings.
5. Settings (3.9): the medicine row writing `ammoDef` and `def.icon`, the kits and time-per-wound
   rows, the range grid, the summary keys.
6. Player-facing text (section 5).
7. Build, deploy, run section 6.
8. The rename (3.10), as its own commit.
9. Supporting documentation, as a side effect of the landed code, not as design: the Workshop
   description and its editorial contract, `README.md`, `CONTRIBUTING.md`, `CLAUDE.md`, the
   translate skill and its nine glossaries, `Scripts/check-translations.py`'s comments,
   `TODOs.md` and the old spec's status line all describe the field dressing, the doses or the
   right-click menu. Once the code is in, grep the repo for
   `FieldDressing|field.dressing|field-medicine|dose|Doses` outside `Field-Dressing.md` and
   `l10n/` and bring every hit to the shipped state; the review's sweep found 31 files. In
   particular:
   - `CLAUDE.md`: the overview's spec pointer and feature line, the project structure (Defs
     gaining `Effects/`, Abilities, Comps and the new Gizmos folder), the settings bullet
     (gaining `ammoDef`, `icon` and the extension's `ticksPerTend`), the reload-queries bullet
     (its patch is now the reload target), and the localization lines naming the dressing.
   - `.steamworkshop/Description/English.txt` (the "Field dressing" section, its right-click
     instruction and the "Grants field-dressing" bullet) and `.steamworkshop/CLAUDE.md`'s
     examples.
   - `TODOs.md`: close the rework item, the first-fill line in the tuning item (five
     glitterworld, not industrial), the adrenaline shot's "when a field dressing completes" and
     the 1.6 visuals re-check.
   - A status line on `Field-Dressing.md` naming this spec as superseding its sections 3.2 to
     3.5 and the settings table in 3.6, and correcting its "nothing implemented" status.
   - The sidecar regen and the CHANGELOG `[Unreleased]` entry.
