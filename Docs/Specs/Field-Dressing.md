# SPEC: Doctor passives and the field dressing

Status: DESIGN, nothing implemented. Written 2026-10-21 against the installed RimWorld
1.7.4982 rev1280 (`Assembly-CSharp.dll` via `ilspycmd`; the 1.6 Krafs reference 1.6.4871 for
the build), Core and Biotech XML from the same install, VEF and VFE Pirates at the checkouts
under `../VanillaExpanded/` (upstream `main`). Tracked by the "Abilities and gizmos" entry in
`TODOs.md`. The roster context is `Docs/Research/WARCASKET_ABILITIES.md`; the sibling precedents
are Shipcracker's breach jump (one C# ability reading a vanilla reloadable off the piece that
grants it) and Toxin's tank (a `CompApparelReloadable` subclass with Harmony prefixes on the
reload queries that vanilla left non-virtual).

Depends on nothing in `Medic-Work-Trait.md`, but the two meet in play: the dressing's quality
reads the pilot's Medicine skill, and the trait is what lets that pilot practise it.

## 1. Goal

Two things, in this order:

1. **Doctor passives, XML only.** The shoulders floor Manipulation and speed up tending; the
   helmet floors Sight and raises tend quality. Both feed vanilla's tend stats and, through
   them, the ability below.
2. **Field dressing, the set's one active.** The armor carries a medicine suite: a reloadable
   holding up to five doses **of each** medicine type, with one type selected at a time. The
   ability tends every tendable wound on a targeted humanlike or animal in one cast, at the
   quality vanilla would give that pilot with that medicine, and spends one dose of the type
   that was selected when the target was clicked. With the helmet worn it also tends diseases
   and infections. Reloading takes only the selected type; nothing loaded can be taken back out.

Player-only in practice: raiders in the set carry a full suite but the AI never casts apparel
abilities.

## 2. Verified engine facts

### 2.1 Tend quality and tend XP

`TendUtility.CalculateBaseTendQuality(doctor, patient, medicinePotency, medicineQualityMax)`:

- starts from the doctor's `MedicalTendQuality` (0.75 with no doctor), multiplies by the
  medicine's potency, adds the patient's bed `MedicalTendQualityOffset` if they are in one,
  multiplies by 0.7 when doctor and patient are the same pawn, clamps to the medicine's
  quality max. Nothing else. **Room cleanliness is not a tend-quality input** in vanilla; it
  drives infection chance (`HediffComp_Infecter`) and surgery success only, so there is nothing
  for the dressing to ignore.
- `MedicalTendQuality` (`Stats_Pawns_WorkMedical.xml`): base 1, Medicine skill 0.20 + 0.10 per
  level, capacity factors Sight weight 0.7 max 1.4 and Manipulation weight 1 max 1.4. Apparel
  `equippedStatOffsets` reach it because it is a pawn stat.
- `MedicalTendSpeed`: base 1, skill 0.4 + 0.06 per level, Sight 0.8 max 1.3, Manipulation 1,
  and `WorkSpeedGlobal` as a factor (so Hardworking Warcaskets' malus reaches it).
- Medicine (`Items_Resource_Stuff.xml`): herbal potency 0.60 / max 0.70 / value 10; industrial
  1.00 / 1.00 / 18; glitterworld 1.60 / 1.30 / 50. `ThingDef.IsMedicine` is "has a
  `MedicalPotency` stat base", so modded medicines qualify automatically.
- `HediffComp_TendDuration.CompTended(quality, maxQuality, batchPosition)` adds a random
  -0.25..+0.25 per hediff, clamps to the max, and throws the "Tended / Quality" text only for
  `batchPosition` 0.
- Tend XP is not granted by `DoTend`. The tend job's wait toil sets `activeSkill` Medicine and
  `JobDriver.DriverTick` learns `SkillTuning.XpPerTickDefault` (0.1) per tick of it, so one
  treatment at base speed (600 ticks) is worth 60 XP, less for a faster doctor.

### 2.2 What one treatment tends

Vanilla's tend job (`JobDriver_TendPatient`) spends one medicine per treatment and
`GetOptimalHediffsToTendWithSingleTreatment` picks the batch: the top-priority tendable hediff,
plus every other hediff of the same def when its `tendAllAtOnce` is set, or, for an injury
tended with medicine, further injuries while their summed severity stays at or under 20. A pawn
with many wounds takes several treatments and several medicines; the job loops.

Biotech's Coagulate (`CompAbilityEffect_Coagulate`) ignores that rule: every `Hediff_Injury` and
`Hediff_MissingPart` that `TendableNow()` is `Tended` at once with one hemogen cost, and the
target must have a tendable wound (`AbilityUtility.ValidateHasTendableWound`). Diseases and
infections are not touched. Its def: `Verb_CastAbilityTouch`, range -1, warmup 1 s,
`stunTargetWhileCasting`, animals and humans targetable, not self, not mechs, hostile false.

`stunTargetWhileCasting` is implemented in `Verb_CastAbility.TryStartCastOn`: when the target
is a pawn other than the caster, `target.stances.stunner.StunFor(warmupTicks, caster,
addBattleLog: false, showMote: false)` and `RestUtility.WakeUp` if asleep. It is a vanilla
`AbilityDef` field; VEF's def has no equivalent, so the ability mirrors it in code.

### 2.3 The vanilla reloadable

`CompApparelReloadable : CompApparelVerbOwner_Charged`. The parts that matter:

- `remainingCharges` is `protected`; `MaxCharges` is `Props.maxCharges`; `LabelRemaining` is
  `"{remaining} / {max}"`; `GizmoExtraLabel` returns it; `PostPostMake` fills to max on
  creation; `CanBeUsed` fails on `remainingCharges <= 0`.
- `AmmoDef => Props.ammoDef` is **non-virtual**, as are `NeedsReload`, `ReloadFrom`,
  `MinAmmoNeeded`, `MaxAmmoNeeded`. With `ammoCountToRefill` 0: `NeedsReload` is
  `remaining != max` for both the automatic and the forced case; `ReloadFrom(ammo)` adds
  `min(ammo.stackCount / ammoCountPerCharge, max - remaining)` charges and destroys that much
  ammo; `MaxAmmoNeeded` is `ammoCountPerCharge * (max - remaining)`. Every one of them reads
  `remainingCharges` and `MaxCharges` and nothing else.
- `UsedOnce()` is virtual and decrements. `PostExposeData` is virtual and scribes
  `remainingCharges`.
- `JobGiver_Reload` and the float-menu provider go through `IReloadableComp` and
  `reloadable.AmmoDef`; the float menu offers a stack only when `clickedThing.def == AmmoDef`.
  `JobDriver_Reload` resolves the comp with `Gear.TryGetComp<CompApparelReloadable>()`, so the
  suite must **subclass** the vanilla comp, not reimplement the interface.
- The dev gizmo "reload to full" writes `remainingCharges = MaxCharges` directly, bypassing
  `ReloadFrom`.

### 2.4 VEF ability plumbing

- `AbilityDef.targetMode` Pawn builds a `TargetingParameters` for pawns; `targetingParametersList`
  can carry an explicit one instead (`canTargetAnimals`, `canTargetHumans`, `canTargetMechs`,
  `canTargetSelf`). `mustBeSameFaction` is a def flag, default false.
- `AbilityDef.jobDef` chooses the cast job. VEF ships `VFEA_GotoTargetAndUseAbility`
  (`JobDriver_GotoTargetAndCastAbilityOnce`, 1.6 tree): a goto toil that paths to the target
  (`PathEndMode.Touch` when `distanceToTarget` is 1.5 or less), orders a pawn target to stand
  and face the caster (`VFEA_StandAndFaceTarget`), then the base driver's wait toil of
  `GetCastTimeForPawn()` ticks (`Toils_General.Wait`, a progress bar, no `Stance_Warmup`) and
  an instant cast toil. The driver sets `job.playerForced = true` for the whole job.
- **Interruption.** `Pawn_JobTracker.Notify_DamageTaken` only considers overriding a job when
  `!curJob.playerForced`, and `Pawn_StanceTracker.Notify_DamageTaken` is empty on this build,
  so being shot during the walk or the cast does not break the job. What does: a player order
  (the job def leaves `playerInterruptible` at its default true), the pilot being downed or
  drafted into something else, a mental break, and the target despawning
  (`FailOnDespawnedOrNull(TargetIndex.A)`; a target going down stays valid). A VEF
  `VFEA_UseAbilityUninterruptible` def exists only for the cast-in-place driver; an
  uninterruptible walk-and-cast would be an own JobDef cloning the goto one with
  `playerInterruptible false`.
- `Ability.WarmupToil(Toil)` and `PreWarmupAction()` are virtual hooks the wait toil calls;
  the stun goes there.
- `Ability.DoAction` → `DoTargeting` → `Find.Targeter.BeginTargeting(this)`; the ability is the
  `ITargetingSource`, so `OnGUI(target)` draws `MouseAttachment(target)` (protected virtual,
  default: the def icon) and `ExtraLabelMouseAttachment(target)` (virtual, default null).
- `CreateCastJob` → `StartAbilityJob`: ends the current job and starts the cast job; the
  driver calls `Cast(targets)`. `Cast` sets the cooldown, runs extensions, hediffs and effects,
  then `PostCast`. `ValidateTarget(target, showMessages)` and `IsEnabledForPawn(out reason)`
  are virtual; the base `IsEnabledForPawn` reports the cooldown first.
- `Ability.ExposeData` is virtual and the instance is scribed `LookMode.Deep` by
  `CompAbilitiesApparel`, so ability state survives a save.
- `GetGizmo()` instantiates `def.gizmoClass` with `(pawn, ability)`; vanilla `Command` exposes
  `TopRightLabel`, `RightClickFloatMenuOptions`, `IconDrawColor` and `DrawIcon` as virtuals, and
  `Command_Ability` sets `icon = def.icon` in its constructor and groups gizmos by def.
- `GenUI.DrawMouseAttachment(Texture2D)` and `ThingDef.uiIcon` exist for the cursor icon.

### 2.5 Apparel levers

- `VEF.Apparels.ApparelExtension.Merge` combines `pawnCapacityMinLevels` and
  `equippedStatFactors` and ORs `isUnifiedApparel`, `hideHead` and `showBodyInBedAlways`, so a
  priority-1 entry on the shoulders or helmet keeps the base's rendering flags. The fields it
  does not copy are `priority` itself and `moveSpeedFactorByTerrainTag`.
- `pawnCapacityMinLevels` floors the named capacity while the piece is worn
  (`Apparel_Patches.cs`); the Sarcophagus set uses it per body region.
- A piece checking for a sibling piece by def in code is the Controller helmet's precedent
  (`Verb_DroneDeployment` looks for the helmet in `WornApparel`).

## 3. Design

### 3.1 Passives

Shoulders (`WarcasketShoulders_Medic.xml`), new priority-1 `ApparelExtension`:
`pawnCapacityMinLevels` Manipulation 1.0; `equippedStatOffsets` `MedicalTendSpeed` +0.25.
Drop the dead `EnergyShieldRechargeRate +1` (discrepancy 1 in the research doc) at the same
time.

Helmet (`WarcasketHelmet_Medic.xml`), new priority-1 `ApparelExtension`:
`pawnCapacityMinLevels` Sight 1.0; `equippedStatOffsets` `MedicalTendQuality` +0.10. The helmet
is also what lets the dressing treat diseases (3.3); the description should say so.

Sizing, to be revisited in the tuning pass: +0.10 quality is one Medicine level and equals a
hospital bed's offset; +0.25 speed is about four levels. The floors matter only to an impaired
pilot (a floor never lowers a capacity) and feed both stats through their capacity factors;
Sight 1.0 also steadies an impaired pilot's aim, which is in character for a visor. The
descriptions already promise "field-medicine manipulators" and "a diagnostic visor".

### 3.2 The suite (armor comp)

`CompFieldDressingSuite : CompApparelReloadable`, props
`CompProperties_FieldDressingSuite : CompProperties_ApparelReloadable` adding
`defaultMedicine` (ThingDef, `MedicineIndustrial`). Def values: `maxCharges` 5 (**per
type**), `ammoCountPerCharge` 1, `ammoDef` `MedicineIndustrial` (the def-level value the
selection starts from), `chargeNoun` "dose", `baseReloadTicks` 60, `soundReload`
`Standard_Reload`, `displayGizmoWhileUndrafted` true.

**The selected type's count is the vanilla charge count.** State: `selected` (ThingDef) and
`loaded` (`Dictionary<ThingDef,int>`), both scribed in `PostExposeData`. The vanilla
`remainingCharges` is kept equal to `loaded[selected]` at all times, so every non-virtual
vanilla path (`NeedsReload`, `ReloadFrom`'s clamp, `MinAmmoNeeded`, `MaxAmmoNeeded`,
`CanBeUsed`, `LabelRemaining`, the stat sheet) is correct for the selected type **unpatched**:
the suite wants a reload exactly when the selected type is below five, and a reload fills
the selected type to five whatever the other types hold. The discoverability trap of a shared
pool (herbal full, glitterworld selected, "0 / 5" shown and no reload offered) cannot occur.

- `Select(type)`: `loaded[selected] = remainingCharges; selected = type;
remainingCharges = loaded[type]`. Switching from three glitterworld to industrial shows the
  industrial count; switching back shows three again. Allowed at any time, including mid-walk
  (the pending cast keeps its own type, 3.3).
- `Reconcile()`, run before every read: `loaded[selected] = remainingCharges`. This folds in
  anything that wrote `remainingCharges` behind the comp's back: the dev "reload to full"
  gizmo, another mod's reload path, a capacity lowered by settings (clamp every bucket, as
  Shipcracker's `CompBreachTank` trims its tank).
- `TryConsume(type)`: decrements `loaded[type]`, and `remainingCharges` too when `type` is the
  selection. `remainingCharges` is protected, so no reflection.
- No unload. A loaded dose leaves the suite only by being spent.
- `Types`: every `IsMedicine` def, ordered by `MedicalPotency` descending, so a modded
  medicine lists itself. Counts default to zero.
- First fill: `PostPostMake` leaves vanilla's five charges on the default type and the other
  buckets empty; this is the free fill VFEP's reloadables get and is to be priced into the
  armor's cost in the tuning pass (`TODOs.md`).
- `CompInspectStringExtra` lists the loaded doses by type.

Harmony, `Source/1.6/Patches/CompApparelReloadable_Suite.cs`, gated on
`__instance is CompFieldDressingSuite` so every other reloadable passes through:

- prefix on the `AmmoDef` getter: returns `selected`. The automatic reload, the float-menu
  reload (offered only on stacks of the selected type), `DisabledReason`, the stat sheet's
  "reload" line and the carry-space check all read `AmmoDef`, so this one patch makes them
  follow the selection.
- postfix on `ReloadFrom(Thing ammo)`, defensive only: if `ammo.def != selected` (a foreign
  reload path), move the charges the call added out of the selected bucket into `ammo.def`'s
  and restore `remainingCharges = loaded[selected]`. Vanilla's own paths always load the
  `AmmoDef`, so in practice it never fires.

### 3.3 The ability (`AbilityDefs/FieldDressing.xml`, `Ability_FieldDressing`)

`VEF.Abilities.AbilityDef` `MDWC_FieldDressing`, granted by a `CompProperties_AbilitiesApparel`
on the armor (same def as the suite, as the breach jump sits with its tank, because the ability
reads the suite off `holder`). `abilityClass` `MedicWarcasket.Ability_FieldDressing`,
`gizmoClass` `MedicWarcasket.Command_FieldDressing`, `targetMode` Pawn with an explicit
`targetingParametersList` entry: humans and animals, not mechs, **not self**, not buildings.
`jobDef` `VFEA_GotoTargetAndUseAbility`. `range` 1.9 (adjacent, diagonals included; the job
walks the pilot there). `castTime` 60. `cooldownTime` 1200. `showUndrafted` true (a medic
dresses wounds in the hospital too). `requireLineOfSight` true. Faction is not checked, so a
downed raider can be dressed before capture. Interruptible as 2.4 describes: a new order
cancels the walk or the cast, and nothing is spent until the cast toil runs.

- **Enabled when** (`IsEnabledForPawn`, after the base cooldown check): the suite holds at least
  one dose of `selected`; otherwise the reason names the type ("No {type} loaded").
- **Hediff set** (`TendableSet(target)`): every `Hediff_Injury` and `Hediff_MissingPart` that
  `TendableNow()`; **with `MDWC_WarcasketHelmet_Medic` worn by the pilot**, every hediff that
  `TendableNow()`, which adds diseases and infections. The helmet check is a def comparison on
  `pilot.apparel.WornApparel` at cast time (the Controller idiom, 2.5), via a `[DefOf]`.
- **Valid target** (`ValidateTarget`): a spawned, living pawn whose `TendableSet` is non-empty;
  otherwise "{pawn} has no wounds to dress" (or "nothing to treat" when the helmet is worn).
  Downed is fine.
- **Medicine is fixed at the click.** `DoAction` snapshots `suite.Selected` into
  `pendingMedicine` (scribed) before targeting starts; the targeter, the walk and the cast all
  use it, so changing the gizmo selection mid-walk changes the next cast, not this one. If the
  pending type's count is zero at cast time (only reachable through dev tools), the cast
  aborts with the "No {type} loaded" message and no cooldown.
- **Stun while casting**, mirroring vanilla (2.2): in `PreWarmupAction`, when the target is a
  pawn, `StunFor(castTime, pilot, addBattleLog: false, showMote: false)` and wake it if
  asleep. VEF's goto driver has already given it the stand-and-face job; the stun holds a
  conscious target still for the second the dressing takes.
- **Cast:** `TryConsume(pendingMedicine)`, then `quality =
TendUtility.CalculateBaseTendQuality(pilot, target, potency, qualityMax)` with the dose's
  stat bases, then over `TendableSet(target)` in tend-priority order
  (`TendUtility.SortByTendPriority`): `Tended(quality, qualityMax, i)`. Count tended, throw
  vanilla's `NumWoundsTended` text, play `TechMedicineUsed` when the dose's potency beats
  industrial (as `DoTend` does), increment `TimesTendedTo` / `TimesTendedOther` records. The
  bed offset is whatever vanilla gives, since the vanilla formula is called unchanged.
- **Medicine XP:** `600 * SkillTuning.XpPerTickDefault` (60 XP) per cast, the XP of one
  base-length treatment, granted through `pilot.skills.Learn(SkillDefOf.Medicine, xp)`. It is
  a flat amount: the dressing spends a dose and uses the skill, as a treatment does, and a
  per-wound scale would reward chip damage.
- **Effects:** none beyond the text and sound until the beam effect lands (`TODOs.md`); VEF's
  `castFleck` / `fleckOnTarget` fields are the hook when it does.

### 3.4 Gizmo (`Command_FieldDressing : Command_Ability`)

- Icon: the selected medicine's `uiIcon`, set in the constructor and refreshed in
  `GizmoOnGUIInt` so a selection change shows on the next frame; `IconDrawColor` white.
- `TopRightLabel`: `"{loaded of selected} / {max}"`, read live from the suite; with the
  selected count mirrored into `remainingCharges` this is vanilla's own `LabelRemaining`.
- `RightClickFloatMenuOptions`: one option per `suite.Types` entry, built with the vanilla
  `FloatMenuOption(label, action, shownItemForIcon)` constructor so each line carries the
  medicine's item icon; label `"{medicine label} ({loaded})"`; the current selection is listed
  but disabled, so the menu shows the state. Selecting a type calls `Select`, and the next
  automatic reload tops that type up to five.
- Multi-select: VEF groups the gizmo by def, so with several medics selected the first pawn's
  suite is shown and changed, as vanilla's reloadables behave.
- Disabled text: the suite's "No {type} loaded" or the base cooldown line.

### 3.5 Targeting

`MouseAttachment` override returns the pending medicine's `uiIcon` for a valid target and
vanilla's `CannotShoot` otherwise; `ExtraLabelMouseAttachment` returns the medicine's label
and, when the target is valid, the base quality the formula would give ("Industrial medicine,
quality 86%"), computed from the pending def and the hovered pawn. The highlight ring is VEF's
default.

### 3.6 Mod settings

`Source/1.6/Settings/`: `MedicWarcasketMod : Mod` (window and `WriteSettings` → `ApplyToDefs`
only; `PatchAll` stays in `ModInit`), `MedicWarcasketSettings : ModSettings`, `Tunable.cs`.
Copy `Tunable.cs` from Shipcracker, not Toxin: on 2026-10-11 Shipcracker's gained `DisplayScale`,
`LabelArg`, `DescArgs` and a pure `Snap`, and the two have diverged; Shipcracker's is the
superset. (Backporting it to Toxin is a family chore, not this spec's.) `ApplyToDefs` runs from
`ModInit` after `PatchAll` and whenever the window closes; defaults are read from the defs so
the def headers stay the record.

| Setting                 | Def field                           | Default | Range             |
| ----------------------- | ----------------------------------- | ------- | ----------------- |
| Doses per medicine type | comp `maxCharges`                   | 5       | 1..20, step 1     |
| Cooldown (seconds)      | ability `cooldownTime` (ticks ÷ 60) | 20      | 0..300, step 5    |
| Range (cells)           | ability `range`                     | 1.9     | 1.5..12.9, step 1 |

Settings strings are Keyed, which this mod does not have yet; see section 5.

## 4. Decided and open levers

Decided (2026-10-21): capacity is per type; the target is stunned for the cast; self-target is
off; Medicine XP is granted; diseases and infections join the hediff set only with the helmet
worn; cast time 60 ticks; free first fill of five industrial, to be priced in the tuning pass;
no unloading, with switching at any time; "dose" as the charge noun unless a target language
cannot render it (section 5); the gizmo shows undrafted.

Still open, each a one-line change if wanted:

1. **Cast time as a setting.** Fixed at 60 ticks for now.
2. **XP amount.** 60 per cast, the XP of one base-length treatment.
3. **Stat offset sizes** in 3.1 are first guesses for the tuning pass.
4. **An uninterruptible cast.** Not recommended: the dose is only spent at the cast toil, so an
   interrupted dressing costs nothing, and a medic that cannot be re-ordered mid-walk is worse
   than one that can.

## 5. Localization surface

- A `VEF.Abilities.AbilityDef` adds the folder `DefInjected/VEF.Abilities.AbilityDef/` (the
  full type name, as Shipcracker's sidecar records it; it is its own def database, so no
  alias) with `label`, `description`, `jobReportString` and, because the walker lists it,
  `iconPath`. The armor's new comp adds `comps.CompFieldDressingSuite.chargeNoun` (the walker
  keys a comp both by index and by comp class; Shipcracker's `comps.CompBreachTank.chargeNoun`
  is the precedent).
- **Charge noun "dose".** Vanilla prints it only through plural and lookup slots, so each
  language's rendering must survive the plural worker: a single word in French (its worker
  never reads the plural table), a `plural.txt` row where the language needs one (German
  "Dosis / Dosen", Russian with `case.txt`), nothing for Chinese, Korean or Japanese. Each
  roster language has a one-word equivalent, so "dose" stands; the translation pass records
  the forms in the glossary's "Charge nouns" section, and if a language cannot be made to
  render it the fallback is "charge", which VFEP's own sets already localize.
- The float menu labels, the disabled reasons, the mouse label and the settings window need
  Keyed strings: `1.6/Languages/English/Keyed/MedicWarcasket.xml`. That is this mod's first
  Keyed file. The translate skill (`.claude/skills/translate/SKILL.md`) currently states "No
  Keyed strings and no English Languages tree at all" and must be rewritten for it; Toxin's
  skill is the worked example with one Keyed file.
- Regenerate `Scripts/expected-injections.json` after the defs land; update the skill's def-type
  list.

## 6. Verification

Dev mode, VFEP chain plus this mod, game closed between runs:

1. Entomb a colonist with Medicine 8 and no impairments. Stat sheet shows tend speed and quality
   with the shoulder and helmet offsets; dev-damage an eye and a hand and confirm the floors hold
   Sight and Manipulation at 100% while the pieces are worn and drop them when removed.
2. New suite: inspect string lists five industrial; gizmo shows the industrial icon and "5 / 5".
3. Right-click the gizmo: three vanilla medicines listed with icons and counts, industrial
   disabled as current; select glitterworld: icon and label change to "0 / 5" and the
   automatic reload fetches glitterworld up to five; select herbal: "0 / 5" again and a herbal
   reload follows; select glitterworld again: "5 / 5" restored; float menu on an industrial
   stack offers no reload while glitterworld is selected.
4. Dev-wound a colonist with six cuts and a bruise and give them the flu; cast on them from
   across the room without the helmet: the pilot walks adjacent, the target is held for the
   1-second cast, every injury tended at one quality near the formula's, "7 wounds tended"
   text, the flu untouched, one glitterworld dose gone, cooldown shown, Medicine XP up by 60.
   Put the helmet on and cast again: the flu is tended too. Repeat on a downed raider and on a
   pet; confirm the pilot cannot be targeted.
5. Select glitterworld, click a target, switch to industrial during the walk: the cast spends
   glitterworld. Save mid-walk and reload: the pending type survives. Draft the pilot mid-walk:
   the job ends and nothing is spent. Shoot the pilot mid-cast with dev tools: the cast
   completes.
6. Dev "reload to full" with glitterworld selected: inspect string attributes the new doses to
   glitterworld; lower the doses setting to 3: every type trims to 3.
7. Hardworking Warcaskets active: tend speed in the stat sheet shows its malus and the cast
   time does not change (it is the ability's, not the tend job's).
8. `python3 Scripts/integration-smoke-test.py` and `python3 Scripts/check-translations.py`
   clean after the sidecar regen; a Release build with warnings as errors.

## 7. Implementation checklist

1. Passives: shoulders and helmet `ApparelExtension` entries and offsets (3.1), headers
   updated with the why; remove the dead recharge offset.
2. `Source/1.6/Comps/CompFieldDressingSuite.cs` and `CompProperties_FieldDressingSuite.cs`
   (3.2); `Source/1.6/Patches/CompApparelReloadable_Suite.cs`; `Source/1.6/MDWC_DefOf.cs`.
3. `1.6/Defs/AbilityDefs/FieldDressing.xml`; `Source/1.6/Abilities/Ability_FieldDressing.cs`
   and `Command_FieldDressing.cs` (3.3 to 3.5); armor def gains the two comps and the header
   paragraph that explains why both sit on the armor; helmet description mentions the disease
   treatment.
4. `Source/1.6/Settings/` (3.6), `ModInit` calls `ApplyToDefs` after `PatchAll`.
5. Keyed file; translate skill rewrite; sidecar regen (5).
6. Build, deploy, run section 6.
7. CHANGELOG Unreleased entry; Workshop description; close the `TODOs.md` entry and leave the
   ledger lines that follow it.
