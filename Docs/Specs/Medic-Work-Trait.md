# SPEC: Medical work for the entombed medic

Status: DESIGN, nothing implemented. Written 2026-10-20 against RimWorld 1.7.4982 rev1280 (the
1.6 Krafs reference 1.6.4871 for the build), VFE Pirates checkout at upstream `main` v4.01
(commit e12223c; its 1.6 trait XML is byte-identical to the installed Workshop copy), VEF
checkout at upstream `main`, and the installed Workshop copies of every third-party mod named
below (Hardworking Warcaskets fetched with SteamCMD on the writing date). Tracked by the
"Medical work for the wearer" entry in `TODOs.md`.

Not in scope: any change to VFE Pirates. An upstream proposal to move warcasket work blocking
onto hediffs was vetoed by the VE design lead, so this spec assumes the trait mechanism stays
exactly as shipped for the life of this mod.

## 1. Goal

A pawn entombed in the Medic set can do medical work: tending, surgery, feeding and rescuing
patients, and childcare where Biotech is active. Social work is already open to every warcasket
wearer (section 2.2). Everything else the VFEP warcasket trait blocks stays blocked, and the
blocking must follow what the player's other warcasket mods do to VFEP's profile: "Warcaskets can
work!", "Warcaskets can haul!" and "VFE Pirates - Hardworking Warcaskets" must affect a Medic
wearer the way they affect a Siegebreaker wearer, without any of those mods knowing we exist.

## 2. How work blocking works today (verified)

### 2.1 The trait and how a pawn gets it

`VFEP_WarcasketTrait` (`1.6/Defs/TraitDefs/Traits_Singular.xml`): `commonality 0`,
`disabledWorkTags` ManualSkilled, ManualDumb, Animals, Artistic, Caring, Intellectual;
`VEF.Pawns.TraitExtension` with `painFactor 0` and `apparelExclusiveTrait true`. Its description
is the only place the pain immunity and the work block are explained to the player, and it says
the wearer "is not capable of doing any work".

Every VFEP torso, and every third-party torso that copies VFEP's block (ours included,
`1.6/Defs/ThingDefs_Misc/Warcasket_Medic.xml`), grants it through
`VEF.Apparels.ApparelExtension.traitsOnEquip` and grants `VFEP_Shellcasket` through
`traitsOnUnequip`. The abstract `VFEP_WarcasketArmorBase` carries neither list, so a child def
decides its own traits. VEF (`Source/VEF/Apparels/Harmony/Apparel_Patches.cs`) applies them from
postfixes on `Pawn_ApparelTracker.Notify_ApparelAdded` / `Notify_ApparelRemoved` and replays the
equip side at `PostLoadInit`. A trait flagged `apparelExclusiveTrait` can only be gained while
some worn apparel lists it in `traitsOnEquip` (`TraitSet.GainTrait` prefix); the notify fires
after the item is in `WornApparel`, so the self-grant passes. `HediffSet.CalculatePain` is
multiplied by every trait's `painFactor`.

### 2.2 Which work the tags cover

Core and the four DLCs, 23 work types, read from the installed `Data/*/Defs/WorkTypeDefs`:

| Tag the trait blocks | Work types carrying it |
|---|---|
| Caring | Doctor; Childcare (Biotech, also tagged Social) |
| ManualDumb | Hauling, Cleaning (exactly these two) |
| ManualSkilled | Cooking, Construction, Growing, Mining, PlantCutting, Smithing, Tailoring, Crafting |
| Animals | Handling; Fishing (Odyssey) |
| Artistic | Art |
| Intellectual | Research; DarkStudy (Anomaly) |

Social is **not** blocked: Warden (Social, AllWork) is open to every warcasket pawn today.
Firefighter, Patient, PatientBedRest and BasicWorker carry none of the blocked tags. So the
medic delta is exactly "drop Caring"; it unlocks Doctor and Childcare and nothing else.

### 2.3 Hauling is a Harmony exemption keyed on VFEP's def

`VFEPirates.Trait_AllowsWorkType_Patch` postfixes `Trait.AllowsWorkType` and returns true for
Hauling and Hunting when `__instance.def == VFEP_DefOf.VFEP_WarcasketTrait`. The Hunting half is
a no-op (Hunting carries no blocked tag). A trait with a different def does not get the
exemption, so a copied profile silently loses hauling. Replacing ManualDumb by Cleaning in the
copy reproduces the exact behaviour, because ManualDumb covers only Hauling and Cleaning.

`VFEPirates.Pawn_GetDisabledWorkTypes_Patch` re-adds trait disables for slaves wearing any
`Apparel_Warcasket`; it iterates every trait on the pawn, so it covers a foreign trait as well.

### 2.4 Where the game reads the block

- Work tab and `Pawn_WorkSettings`: `Pawn.GetDisabledWorkTypes` (cached, cleared by
  `Notify_DisabledWorkTypesChanged`) via `Trait.GetDisabledWorkTypes`, which reads
  `def.disabledWorkTypes` plus every WorkTypeDef whose tags intersect `def.disabledWorkTags`.
  Both fields are read live from the def on every refill; nothing is cached on the TraitDef.
- Bio tab "Incapable of" and skill greying: `Pawn.CombinedDisabledWorkTags`, which ORs the
  traits' `disabledWorkTags` directly.
- VEF's `ApparelExtension.workDisables` reaches only the first path. Apparel-level disables never
  show in the bio or grey a skill.

### 2.5 Ecosystem survey

| Mod (Workshop id) | What it does | Consequence for us |
|---|---|---|
| WarCasket Expanded (2733135485) | Copies VFEP's trait block on most torsos; Centurion grants no equip trait | Precedent for "copy block"; nothing to do |
| Mechlord Warcasket (3087911691) | Own `MechanitorWarcasketTrait` (own tag set, `painFactor 0`, no `apparelExclusiveTrait`), `VFEP_Shellcasket` on unequip | The idiom we adopt |
| Alpha Mechs (2973169158) | Own `VFEP_WarcasketTrait_Mech`, injected under a Biotech `PatchOperationFindMod`; `VFEP_Shellcasket` on unequip | Same idiom |
| Warcaskets can work! (2730151836) | `PatchOperationRemove` of `/Defs/TraitDef[defName="VFEP_WarcasketTrait"]/disabledWorkTags`; a conditional copy for Alpha Mechs' trait | Patches VFEP's def by xpath; will never target ours |
| Warcaskets can haul! (2730543694) | Removes the `li`s and re-adds ManualSkilled, Animals, Artistic, Caring, Intellectual; conditional copy for Alpha Mechs | Same |
| VFE Pirates - Hardworking Warcaskets (3535194807, `dmp.vfepirates.warcasketwork`, 1.6) | **Unconditional** `PatchOperationReplace` of the trait's `disabledWorkTags` with an empty node, plus a replaced description; three VEF `PatchOperationToggableSequence`s (two default-on) adding `equippedStatOffsets` to the three abstract bases: WorkSpeedGlobal -0.25 / -0.15 / -0.10 on armor / shoulders / helmet, carry capacity +10 / +5, and an off-by-default labour profile. `loadAfter` VFEP only. | Trait patch is not toggleable: with this mod active VFEP's profile is empty. The stat offsets land on the abstract bases we inherit, so Medic pieces take the same work-speed malus as VFEP's own; nothing to do |

The two patch mods and Hardworking all shaped the Alpha Mechs trait by adding a conditional
patch on *their* side. Nobody will do that for a trait nobody has heard of, and reimplementing
their patches behind `ModsConfig.IsActive` checks would track three mods' behaviour forever and
double-apply when they update. The design below needs none of that.

Weapon gating is unaffected by the trait choice: VEF's `EquipmentUtility_CanEquip_Patch.CanEquip`
accepts any worn torso-group apparel with trade tag `Warcasket`, which our armor inherits from
`VFEP_WarcasketPartBase`. Mechlord and Alpha Mechs rely on the same path.

## 3. Options rejected

- **VEF `workDisables` on our extension.** `ApparelExtension.Merge` ORs `workDisables` and unions
  the trait lists; a child can add but never subtract what a granted trait blocks, and apparel
  disables are invisible in the bio and skills (2.4). Cannot express "VFEP's profile minus Caring".
- **Keep VFEP's trait, re-allow Doctor from a Harmony postfix** on `Pawn.GetDisabledWorkTypes`
  or `Trait.AllowsWorkType` keyed on the worn torso. The work tab would allow Doctor while the bio
  still lists Caring under "Incapable of" and greys Medicine, because `CombinedDisabledWorkTags`
  reads the trait's tags directly. Patching that too means patching `CharacterCardUtility`'s
  private cause builders for one def. Not worth the surface.
- **Own trait with a hardcoded tag list.** Correct in isolation but deaf to the patch mods: a
  "can work!" user gets a Medic who can tend and nothing else while every other warcasket is
  fully able. This is the Mechlord/Alpha Mechs state today, and the thing the mirror fixes.
- **Own trait filled by an XML copy at load.** Vanilla has no copy operation, and even with one
  the result would depend on load order: neither patch mod declares `loadAfter` for us, so a
  load-time copy taken before they run would miss them. The copy has to happen after all patches,
  which means startup code.

## 4. Design

Own trait, Mechlord shape, with the work-tag profile derived at startup from VFEP's trait.

### 4.1 TraitDef (`1.6/Defs/TraitDefs/WarcasketTrait_Medic.xml`, one def per file)

```xml
<TraitDef>
  <defName>MDWC_WarcasketTrait_Medic</defName>
  <commonality>0</commonality>
  <!-- Default profile: VFEP's with ManualDumb swapped for Cleaning (VFEP's hauling exemption is
       keyed on its own def) and Caring removed. Overwritten at startup from VFEP_WarcasketTrait
       as patched by the player's mod list; this list is what the mirror produces when VFEP's
       trait is unpatched, kept so the def reads correctly on its own. -->
  <disabledWorkTags>
    <li>ManualSkilled</li>
    <li>Cleaning</li>
    <li>Animals</li>
    <li>Artistic</li>
    <li>Intellectual</li>
  </disabledWorkTags>
  <degreeDatas>
    <li>
      <label>medic warcasket</label>
      <description>...</description>   <!-- see 4.4 -->
    </li>
  </degreeDatas>
  <modExtensions>
    <li Class="VEF.Pawns.TraitExtension">
      <painFactor>0</painFactor>
      <apparelExclusiveTrait>true</apparelExclusiveTrait>
    </li>
  </modExtensions>
</TraitDef>
```

`apparelExclusiveTrait true` (VFEP has it; Mechlord and Alpha Mechs do not) stops trait-adding
mods and the trait-rolling code paths from ever handing the trait to a pawn outside the suit.
`painFactor` is static: no known mod patches it on VFEP's trait, and it is not a work rule.

### 4.2 Armor def

`Warcasket_Medic.xml`: `traitsOnEquip` becomes `MDWC_WarcasketTrait_Medic`; `traitsOnUnequip`
keeps `VFEP_Shellcasket` (the shared "cut out of a warcasket" state, as every add-on does). The
`priority 1` note in its header stays valid. Shoulders and helmet are unchanged. Header gains the
why: own trait because the profile must differ from VFEP's and a child extension can only add.

### 4.3 Startup mirror

`MedicStartup.Run()` (name to taste; `Source/1.6/`), idempotent, runs against the current
DefDatabase:

1. `vfep = DefDatabase<TraitDef>.GetNamedSilentFail("VFEP_WarcasketTrait")`,
   `ours = DefDatabase<TraitDef>.GetNamedSilentFail("MDWC_WarcasketTrait_Medic")`. If either is
   null, `Log.Error("[Medic Warcasket] ...")` and return (VFEP is a hard dependency, so this is a
   broken install, not a supported state).
2. `tags = vfep.disabledWorkTags`; if `tags` has ManualDumb, clear it and set Cleaning (encodes
   VFEP's hauling exemption, 2.3); clear Caring (the medic delta, 2.2).
3. `ours.disabledWorkTags = tags`; `ours.disabledWorkTypes = new List<WorkTypeDef>(vfep.disabledWorkTypes)`
   with Doctor and Childcare removed if present (copy by value, never alias VFEP's list; the list
   form is unused by every mod surveyed but costs one line).
4. One `Log.Message` with the resulting tag set, so a bug report's log shows the profile the
   player actually got.

Why nothing more is needed: `Trait.AllowsWorkType` and `Trait.GetDisabledWorkTypes` read the def
fields live (2.4), `Pawn.GetDisabledWorkTypes` is cached per pawn and no pawn exists when startup
code runs, and `CombinedDisabledWorkTags` is uncached.

**Where it runs.** Per CLAUDE.md, def-derived startup state goes in a postfix on
`StaticConstructorOnStartupUtility.CallAll` so an in-process play-data reload re-derives it
(`../UniqueMeleeWeapons/Source/1.6/Patches/StaticConstructorOnStartupUtility_CallAll_Patch.cs`
has the verified ordering). One wrinkle specific to this mod: the reference implementation arms
its patch from a `Mod` constructor, while ours arms from `ModInit`'s static ctor, which `CallAll`
itself is executing when `PatchAll` runs. A postfix applied to a method already on the stack does
not fire for that invocation, so on the first load `ModInit` must call `Run()` directly after
`PatchAll()`; the postfix covers every reload. `Run()` is idempotent so a double run is harmless
and the static ctor never runs twice anyway. `PatchAll` stays in `ModInit` (patch-timing rule).

Ordering guarantee: every XML patch, including VEF toggleable ones, applies during def loading,
before `CallAll`. So the mirror sees VFEP's trait exactly as the full mod list leaves it, in any
load order. That is the whole compatibility story:

| Active alongside us | VFEP's trait after patches | Medic trait after the mirror | Medic can |
|---|---|---|---|
| nothing | ManualSkilled, ManualDumb, Animals, Artistic, Caring, Intellectual | ManualSkilled, Cleaning, Animals, Artistic, Intellectual | doctor, childcare, warden, haul, fight fires |
| Warcaskets can haul! | ManualSkilled, Animals, Artistic, Caring, Intellectual | ManualSkilled, Animals, Artistic, Intellectual | the above plus clean |
| Warcaskets can work! | none | none | everything |
| Hardworking Warcaskets | none (empty node) | none | everything, at its work-speed malus |

### 4.4 Player-facing text

Label `medic warcasket` (shows in the Bio traits list). Description in VFEP's register with
`{PAWN_nameDef}` grammar, stating the pain immunity, that the suit is built around treating the
wounded so the pilot keeps their medical and social faculties, and that other work is beyond the
suit. It must stay true under every row of the table above, so it should describe the suit's
intent rather than enumerate blocked work types. No em dashes.

### 4.5 Localization surface

A TraitDef adds a second DefInjected folder, `DefInjected/TraitDef/`, with
`MDWC_WarcasketTrait_Medic.degreeDatas.0.label` and `.description`. After the def lands:
regenerate `Scripts/expected-injections.json` (the checker refuses stale expectations), add the
folder to the def-type list in `.claude/skills/translate/SKILL.md`, and seed the glossary with
VFEP's own trait label as the term to mirror ("warcasket" trait in each language). No
`DEF_TYPE_ALIASES` change: `TraitDef` is a real database.

### 4.6 Documentation touchpoints

- Trait def header: the mirror contract (what is copied, the two edits, where it runs).
- Armor def header: why an own trait.
- CLAUDE.md Conventions: one bullet that the trait's `disabledWorkTags` is overwritten at startup
  from VFEP's and must not be patched or edited expecting it to stick, pointing at the def header
  and `MedicStartup`. Cross-file coupling, so it belongs there.
- `.steamworkshop/Description/English.txt`: the set's selling point. Say the wearer can do medical
  work and that the "can work!" / "can haul!" / Hardworking mods apply to it like any warcasket.
- `CHANGELOG.md` Unreleased entry.

## 5. Residuals and knobs

- **Cleaning** stays blocked for parity with VFEP's wearers. If a medic should be able to clean
  the hospital, step 2 of the mirror simply does not set Cleaning. One line; a design call.
- **Mods that match VFEP's trait by name** do not know ours. MedPod blocks pod use for
  `VFEP_WarcasketTrait` wearers, so a Medic wearer could be treated in a MedPod; Vanilla
  Aspirations Expanded's "get entombed" aspiration will not complete in a Medic set; Altered
  Carbon, VRE Android and Alpha Random list the VFEP traits in their allow/ignore lists. Mechlord
  and Alpha Mechs have the identical gap. Each can be closed later with a small patch where it
  matters; none blocks this work.
- **Hardworking's work-speed malus** reaches us through the abstract bases. A Medic under its
  default toggles tends at a heavy global work-speed penalty. That is the player's choice through
  that mod and consistent with how it treats VFEP's sets; the Workshop description should mention
  it so it is not reported as our bug.
- **Shellcasket** is unchanged: removal surgery still gives `VFEP_Shellcasket` (Social and
  Intellectual blocked, market value hit) and re-entombing in any warcasket cures it.
- **Raid presence** (`TODOs.md`): veteran raiders wearing the set get the trait on generation
  through `Wear`; AI never does colony work, so no behavioural change.

## 6. Verification

Dev-mode harness, game closed between runs, VFEP chain plus this mod:

1. Entomb a colonist in the Medic set via the foundry (or dev-wear the three pieces). Work tab:
   Doctor assignable, Warden assignable, Hauling and Firefighter assignable, Construction,
   Research, Cooking and Handling disabled. Bio tab: Traits shows "medic warcasket"; "Incapable
   of" lists no Caring entry; Medicine skill not greyed. Health tab: pain stays 0 after a dev hit.
2. Tend a wounded pawn and perform a surgery; feed a patient.
3. Remove the set with the surgery: Shellcasket trait appears; re-entomb in a Siegebreaker:
   Shellcasket gone, VFEP trait present, Doctor disabled again.
4. Repeat step 1 with "Warcaskets can haul!" active, then with "Warcaskets can work!", then with
   Hardworking Warcaskets: work tab matches the table in 4.3; the startup log line shows the
   derived tag set; no red patch errors from any of the three.
5. `python3 Scripts/integration-smoke-test.py` clean; `python3 Scripts/check-translations.py`
   clean after the sidecar regen.
6. Slave wearing the set: Doctor allowed, Construction blocked (exercises VFEP's slave patch
   with a foreign trait).

## 7. Implementation checklist

1. `1.6/Defs/TraitDefs/WarcasketTrait_Medic.xml` (4.1) with header.
2. `Warcasket_Medic.xml`: swap `traitsOnEquip`; header note (4.2).
3. `Source/1.6/MedicStartup.cs` with `Run()`;
   `Source/1.6/Patches/StaticConstructorOnStartupUtility_CallAll_Patch.cs`; `ModInit` calls
   `Run()` after `PatchAll()` with the first-load rationale in a comment (4.3).
4. Build (warnings are errors), deploy, run section 6.
5. Regenerate the sidecar, update the translate skill def-type list and glossary (4.5).
6. CLAUDE.md bullet, Workshop description, CHANGELOG (4.6).
7. Close the `TODOs.md` entry.
