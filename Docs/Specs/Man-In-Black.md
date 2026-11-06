# SPEC: Man in black in the set (an opt-in override of vanilla's last-chance stranger)

Status: IMPLEMENTED 2026-11-06; section 4 records the decisions, section 6 what landed. Written
against the installed RimWorld 1.7 (`Assembly-CSharp.dll` via `ilspycmd`, Core XML), the frozen
1.6 assemblies at `~/.local/share/RimWorld-1.6/Managed` (every call path below was read on
both and is the same), the VEF checkout's `Source/VEF/Storyteller/Harmony/` and the VFEP
checkout's `1.7/Defs/` and `HarmonyPatches/`.

## 0. The brief

| Request                                                                               | Section |
| ------------------------------------------------------------------------------------- | ------- |
| A mod setting, off by default, that puts the man in black in the set, still black     | 3.1     |
| He carries the warcasket minigun instead of the revolver                              | 3.2     |
| A child setting, on by default, that waits for the warcasket research                 | 3.3     |
| The child is inactive while the parent is off                                         | 3.4     |
| Other mods' additions to vanilla's stranger (CE's ammo) reach ours                    | 3.5     |
| Vanilla's event is a last chance that stops scaling past the early game               | 1       |

## 1. Why

Vanilla's `StrangerInBlackJoin` fires when the colony is down to nobody who can act: a lone
joiner walks in, a `StrangerInBlack` in a duster and cowboy hat with a revolver and five
industrial medicine, and the letter asks whether he can set things right. Against a tribal
raid he can. Against mechanoids, or a late raid in marine armor, a revolver and a duster
change nothing, and the event becomes a free corpse. A colony that has researched warcaskets
is a colony whose threats have outgrown the stranger; a medic set with a minigun is the same
last chance at the scale those threats now have, and a medic can also patch up the colonists
he came to save.

## 2. What vanilla does

- `IncidentWorker_WandererJoin.GeneratePawn(Map)` builds a `PawnGenerationRequest` from
  `def.pawnKind`, the player faction, `pawnFixedGender` (Male) and
  `pawnMustBeCapableOfViolence`, then calls `PawnGenerator.GeneratePawn(request)`. 1.6 and 1.7
  are identical.
- `StrangerInBlack` (Core `PawnKinds_Special.xml`) is a plain Human kind: `apparelRequired`
  the duster set, `weaponTags` Revolver, `apparelColor` (0.2, 0.2, 0.2) with
  `ignoreIdeoApparelColors`, `requiredWorkTags` Violent and Caring, Shooting 8 to 16.
- `PawnApparelGenerator.GenerateStartingApparelFor` adds every `apparelRequired` piece
  before spending `apparelMoney`, whatever the money; `PostProcessApparel` then paints
  `kindDef.apparelColor` onto each piece through `SetColor` (its `CompColorable`).
- `PawnWeaponGenerator.TryGenerateWeaponFor` picks by `weaponTags` among weapons priced at or
  under `weaponMoney`, weighted by commonality.
- VEF transpiles the `def.pawnKind` read in `GeneratePawn` to prefer the player faction's
  `FactionDefExtension.strangerInBlackReplacement` when one is set.
- VFEP's `PawnGenerator_GeneratePawn_Patch` postfix strips every non-warcasket piece off a
  generated pawn wearing a warcasket, fills any missing piece at random and locks the apparel.
- Other mods on the event, from a scan of a 382-mod Workshop library: Combat Extended adds a
  `LoadoutPropertiesExtension` (5 to 8 magazines) and a backpack to vanilla's kind; Altered
  Carbon adds a `StackSpawnModExtension`; VFE Tribals' Wild Men faction sets VEF's
  `strangerInBlackReplacement`; Stranger In Black Techlevel replaces the `workerClass` with a
  subclass whose `GeneratePawn` picks a kind per tech level, never reads `def.pawnKind` and
  still honours `pawnFixedGender`. No mod patches `pawnFixedGender` or `letterLabel`.

## 3. Design

### 3.1 The kind

`MDWC_StrangerInBlack` (`1.6/Defs/PawnKindDefs/StrangerInBlack_Medic.xml`) is vanilla's
kind copied field for field (a concrete def cannot be a parent) with `apparelRequired` the
three pieces, no `apparelTags` or `apparelMoney`, and vanilla's own `apparelColor`, so vanilla
paints the set black through the pieces' `CompColorable` exactly as it paints the duster. The
labels stay vanilla's: the letter names the arrival by kind label.

### 3.2 The minigun

`weaponTags` is a tag of ours, `MDWC_StrangerInBlack`, that `1.6/Patches/StrangerInBlackMinigun.xml`
adds to `VFEP_WarcasketGun_Minigun` alone; its own tags are shared with the other heavy guns,
and the generator's weighted pick among them would not be a minigun. `weaponMoney` 5000
clears its price with headroom. The patch is ungated (VFEP is a hard dependency), so a renamed
target errors at every boot; `CompatPatchTests` therefore expects no main-tree hit for an
xpath naming no `MDWC_` def.

### 3.3 The gate

The kind carries a `StrangerInBlackExtension` (`Source/1.6/StrangerInBlack.cs`): `research`
`VFEP_Warcaskets`, `requiresResearch` true, `fallback` vanilla's `StrangerInBlack`. A prefix
on `PawnGenerator.GeneratePawn(PawnGenerationRequest)` (`Patches/PawnGenerator_GeneratePawn.cs`)
rewrites a request for a kind whose extension requires an unfinished project to the fallback,
so the vanilla stranger walks in until the colony can build warcaskets. `StrangerInBlack.Kind`
is the pure rule and has tests; with no game running the gate stays closed.

### 3.4 The settings

Two `ToggleSetting`s in `Settings/Settings_Stranger.cs`, each a def write: the parent writes
`StrangerInBlackJoin.pawnKind` (ours while on, the kind the XML left there while off), the
child writes the extension's `requiresResearch`. Defaults are the def fields as loaded, so the
parent defaults off unless a patch already points the incident at our kind. The child row is
drawn inactive while the parent is off (`ToggleSetting.DoRow`'s `disabled`), keeping its
saved value.

### 3.5 Mirrored extensions

`StrangerInBlack.MirrorFallbackExtensions`, in `Startup.Run`, appends the fallback's mod
extensions to every gated kind's own, skipping the gate's type and any type the kind already
carries. That carries CE's loadout and Altered Carbon's stack chance, and anything a later mod
attaches the same way, without a compat root per mod. Fields are not mirrored: patches on the
fallback's apparel and weapon describe the duster stranger.

## 4. Decisions

- **A kind, not a redress.** Generating our own kind through vanilla's generator, rather than
  stripping and re-dressing the vanilla stranger in a postfix, keeps every generation hook on
  the normal path: vanilla's colour pass, VFEP's warcasket postfix (the lock), and anything
  else keyed on a generated pawn's kind. What a hook reads off the kind itself (CE's ammo
  comes from a mod extension on vanilla's kind) needs the mirror in 3.5.
- **The parent is a def write.** Writing `pawnKind` is the repo's settings rule applied
  literally: the incident def says what it generates, and any code reading the def sees the
  setting. A mod that patches the incident's kind to its own keeps that while the box is off.
- **The gate is on the kind, at the generator.** Research is game state, not a def field, so
  it needs code somewhere; putting it on the kind as "generate my fallback until this project
  is done" keeps the vanilla worker and VEF's transpiler untouched and makes the rule reusable
  by any kind. VEF's own faction replacement still wins over ours by design.
- **Medicine skill added.** Vanilla gives the stranger Shooting 8 to 16 so he can fight; a
  medic set he cannot use would be a worse stranger than vanilla's, so Medicine gets the same
  range. The only field that is neither vanilla's nor gear.
- **`combatPower` 300**, VFEP_Major's, the raid-table kind closest in gear; a joiner's kind is
  never costed, so it only matters to a mod that lists the kind elsewhere.
- **`GameEndedWanderersJoin` is left alone.** It shares the vanilla kind but is the
  post-defeat wanderers event, not the man in black.

## 5. Verification

- `dotnet test`: `StrangerInBlackTests` (the gate's pure rule; the XML's tag, set and fallback
  agree with the patch and the defs; the extension mirror's merge) and `CompatPatchTests`
  (the patch's xpath rule).
- In game (the author's check): dev-mode fire `StrangerInBlackJoin` with the parent box on,
  before and after researching warcaskets; with it off; the settings page with the parent off
  shows the child greyed. With Combat Extended, check the minigun arrives with magazines.

## 6. What landed

`Source/1.6/StrangerInBlack.cs`, `Source/1.6/Patches/PawnGenerator_GeneratePawn.cs`,
`Source/1.6/Settings/Settings_Stranger.cs` and the `disabled` row in `ToggleSetting.cs`;
`1.6/Defs/PawnKindDefs/StrangerInBlack_Medic.xml`, `1.6/Patches/StrangerInBlackMinigun.xml`,
the `MDWC_Stranger*` Keyed strings; `Tests/1.6/StrangerInBlackTests.cs`; one line in
`About.xml`. Section 3.5 added the extension mirror, in `StrangerInBlack.cs` and called from
`Startup.cs`.
