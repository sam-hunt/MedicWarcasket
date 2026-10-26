# Combat Extended Compatibility

What Combat Extended (CE 16.7.4.0, `CETeam.CombatExtended`) does to this set and to the
medical subdomain its abilities run through, verified against CE's XML and a decompile of
`CombatExtended.dll` (the `CombatExtended.HarmonyCE` namespace) with the 1.7 game assembly
beside it. The patch headers under `1.6/Mods/CombatExtended/Patches/` hold the values; this
file holds the survey that decided what needed patching and what did not.

## What CE does to an unpatched warcasket set

CE's VFE Pirates patch (`ModPatches/Vanilla Factions Expanded - Pirates/`) is per-defName, so
from CE an add-on set inherits only the base-class patches: `Flammability` 0.25 on
`VFEP_WarcasketPartBase` and a `PartialArmorExt` on the helmet base (Eye x0.85) and the
shoulder base (Hand x0.80). The apparel autopatcher (`ApparelAutoPatcher`, off by default) only
touches apparel that already has `Bulk` and `WornBulk`, which no warcasket does. Everything
else keeps its vanilla number, read in CE's units:

| Mechanic | CE reading of the unpatched def | Effect |
| --- | --- | --- |
| `ArmorRating_Sharp` | millimetres of RHA (`ArmorUtilityCE.TryPenetrateArmor`) | 2 mm is CE's flak jacket. Every rifle round penetrates. |
| `ArmorRating_Blunt` | megapascals | 1 MPa; CE's flak jacket is 1.5. |
| Mass | counted in full against `CarryWeight` (default 40 kg) by `CompInventory` | 78 kg worn: move speed at the 0.5 floor. |
| `Bulk` / `WornBulk` | missing stats default to 1 / 0, silently | No error, no encumbrance from bulk. |
| Smoke | `Smoke.ApplyHediffs` scales `SmokeInhalation` by `SmokeSensitivity` (base 1, min 0) over Breathing | The helmet grants nothing: `immuneToToxGasExposure` and `ToxicResistance` are never consulted. |

CE's spacer warcasket numbers (its `Apparel_Warcaskets_Spacer.xml`): every armor Bulk 275,
WornBulk 35, Sharp 36, CarryWeight +250, CarryBulk +225, Suppressability -100; every shoulder
piece 15 / 5 and Sharp 36; every helmet 20 / 10, Sharp 32, NightVisionEfficiency_Apparel 0.75,
SmokeSensitivity -1 and a conditional `immuneToToxGasExposure`. Blunt 90 / 72 on the blunt-1
sets (Siegebreaker, Controller, Sarcophagus), 110 / 86 on the blunt-1.5 sets (Guardian, Brute).
Shields x1.5 (1.5 / 0.01 to 2.25 / 0.015). Costs plasteel x1.5 rounded to CE's own figures
(Siegebreaker's 145 to 225) and two more spacer components on the armor and helmet; uranium
untouched.

## CE's medical layer

CE patches no tend entry point or medical stat. Grepped absent from the decompile:
`TendUtility` (`DoTend`, `CalculateBaseTendQuality`,
`GetOptimalHediffsToTendWithSingleTreatment`), `Medicine`, `MedicineDef`, the
`MedicalTend*` / `MedicalSurgery*` / `MedicalPotency` / `MedicalQualityMax` stat defs,
`Recipe_Surgery`, `JobDriver_TendPatient`, `WorkGiver_Tend`, `Toils_Tend`,
`Pawn_HealthTracker.AddHediff` / `PostApplyDamage` / `ShouldBeDowned`, `HealthUtility`,
`HediffUtility`, `HediffComp_Immunizable`, `ImmunityHandler`. No CE def or patch puts a
medical stat on apparel, and CE has no stimulant hediff of its own. What it does change:

- **`HediffComp_TendDuration.CompTended`** (transpiler): the random tend-quality spread
  narrows from +/-0.25 to +/-0.15. The mechanite dressing goes through `Hediff.Tended`, so it
  takes the same roll, and `Ability_MechaniteDressing` reads the rolled `tendQuality` back off
  the comp after the call.
- **`Hediff.TendPriority`** (postfix): raised to at least `BleedRate x 1.5` on injuries that
  carry CE's Stabilize comp. The dressing sorts through `TendUtility.SortByTendPriority`, so
  its most-urgent-first order is CE's bleed-weighted one unasked.
- **`HediffComp_InfecterCE`** replaces vanilla's infecter on every injury def: infection
  chance is divided by `(tendQuality + 0.75)^3` once tended, so tend quality drives infection
  directly, through the same `tendQuality` the dressing sets.
- **`BleedRate`** (postfix on every override): multiplied by the Stabilize comp's modifier;
  `Hediff_MissingPart` always x0.5. Vanilla returns 0 for a tended injury before the postfix
  runs, so a dressed wound stops bleeding as it does without CE.
- **Stabilize**: CE's own float-menu action (`JobDriver_Stabilize`, one injury per
  `60 / MedicalTendSpeed` ticks, `bleedModifier = 1 - 2 x MedicalTendQuality x
  MedicalPotency`). It reads the medic's stats, so the shoulders' MedicalTendSpeed and the
  helmet's MedicalTendQuality offsets strengthen it with no patch. It needs the Doctor work
  type, which the medic trait leaves open.
- **`Pawn_HealthTracker.CheckForStateChange`** (transpiler): non-player pawns never die
  outright when downed. The pain-shock and downed thresholds, `InjuryHealingFactor` and
  `ImmunityGainSpeed` are untouched; so is `HeartAttack`, which the shot removes.
- **Suppression** (`CompSuppressable`): incoming suppression scales by `Suppressability`.
  CE's go-juice patch puts -0.5 on `GoJuiceHigh`'s stage (`Patches/Core/Drugs/Drugs.xml`);
  `AdrenalineSuppression.xml` gives the shot's levels the same, tapered.
- **Bleed and pain retunes** in XML (body-part bleed multipliers, blood-loss stages,
  per-injury infection chances) change what the patient has, not how the dressing tends it.

What CE leaves alone that the set relies on, verified: vanilla tox gas (`Harmony_ToxicUtility`
and `Harmony_GasUtility` are gas-mask AI hooks only), `CompApparelReloadable` and the reload
job (CE adds a "reload worn armor" gizmo beside them), VEF's `ApparelExtension`
(`pawnCapacityMinLevels` still applies through VEF's own postfix) and VEF's `CompShieldBubble`
(absorbs CE projectiles unpatched; only the numbers move).
