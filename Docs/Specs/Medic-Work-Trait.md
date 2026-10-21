# SPEC: Medical work for the entombed medic

Status: DESIGN, nothing implemented. Verified against RimWorld 1.7.4984 rev1242, VFE Pirates
checkout at upstream `main` v4.02 (commit d851807; its 1.7 trait XML is byte-identical to the
installed Workshop copy), VEF checkout at upstream `main` (commit 172479c5), and the installed
Workshop copies of every third-party mod named below (Hardworking Warcaskets fetched with
SteamCMD). Tracked by the "Medical work for the wearer" entry in `TODOs.md`.

**Precondition:** the repo's Phase C migration (`../RIMWORLD_1.7_MIGRATION.md` section 7) has
landed: the mod is 1.7-only, content lives under `1.7/`, source under `Source/1.7/` (net10).
Every path below assumes that layout, and every VFEP fact is about VFEP's `1.7/` tree, which is
what the game loads on 1.7. VFEP's `1.6/` trait differs (pain immunity on the VEF extension, no
rest-fall factor) and is out of scope.

Not in scope: any change to VFE Pirates. An upstream proposal to move warcasket work blocking
onto hediffs was vetoed by the VE design lead, so this spec assumes the trait mechanism stays
exactly as shipped for the life of this mod.

## 1. Goal

A pawn entombed in the Medic set can do medical work: tending, surgery, feeding and rescuing
patients. Social work is already open to every warcasket wearer (section 2.2). Everything else
the VFEP warcasket trait blocks stays blocked, Childcare included, and the blocking must follow
what the player's other warcasket mods do to VFEP's profile: "Warcaskets can work!",
"Warcaskets can haul!" and "VFE Pirates - Hardworking Warcaskets" must affect a Medic wearer
the way they affect a Siegebreaker wearer, without any of those mods knowing we exist. The
trait's non-work effects (pain immunity, the rest-fall factor) follow VFEP's trait the same way.

## 2. How work blocking works today (verified)

### 2.1 The trait and how a pawn gets it

`VFEP_WarcasketTrait` (`1.7/Defs/TraitDefs/Traits_Singular.xml`): `commonality 0`;
`disabledWorkTags` ManualSkilled, ManualDumb, Animals, Artistic, Caring, Intellectual; one
degree carrying the label, the description, `painFactor 0` and `statFactors`
`RestFallRateFactor 1.3`; and a `VEF.Pawns.TraitExtension` declaring only
`apparelExclusiveTrait true`. Its description is the only place the pain immunity and the work
block are explained to the player, and it says the wearer "is not capable of doing any work".

Pain immunity arrives twice. Vanilla applies the degree's `painFactor`. VEF's
`HediffSet.CalculatePain` postfix multiplies pain by every trait extension's `painFactor`
unless it is exactly 1, and `TraitExtension.painFactor` is a float with no initializer, so an
extension that leaves the field out multiplies pain by 0 as well. Any `TraitExtension` we
declare inherits the same trap: leaving `painFactor` out means zero pain, not "unchanged".

Every VFEP torso, and every third-party torso that copies VFEP's block (ours included,
`1.7/Defs/ThingDefs_Misc/Warcasket_Medic.xml`), grants the trait through
`VEF.Apparels.ApparelExtension.traitsOnEquip` and grants `VFEP_Shellcasket` through
`traitsOnUnequip`. The abstract `VFEP_WarcasketArmorBase` carries neither list, so a child def
decides its own traits. VEF (`Source/VEF/Apparels/Harmony/Apparel_Patches.cs`) applies them from
postfixes on `Pawn_ApparelTracker.Notify_ApparelAdded` / `Notify_ApparelRemoved` and replays the
equip side at `PostLoadInit`; the unequip side removes exactly the traits the removed apparel
lists. A trait flagged `apparelExclusiveTrait` can only be gained while some worn apparel lists
it in `traitsOnEquip` (`TraitSet.GainTrait` prefix); the notify fires after the item is in
`WornApparel`, so the self-grant passes.

### 2.2 Which work the tags cover

Core and the four DLCs, 23 work types, read from the installed `Data/*/Defs/WorkTypeDefs`:

| Tag the trait blocks | Work types carrying it                                                             |
| -------------------- | ---------------------------------------------------------------------------------- |
| Caring               | Doctor (Caring, Commoner, AllWork); Childcare (Biotech; Social, Caring)            |
| ManualDumb           | Hauling, Cleaning (exactly these two)                                              |
| ManualSkilled        | Cooking, Construction, Growing, Mining, PlantCutting, Smithing, Tailoring, Crafting |
| Animals              | Handling; Fishing (Odyssey)                                                        |
| Artistic             | Art                                                                                |
| Intellectual         | Research; DarkStudy (Anomaly)                                                      |

Social is **not** blocked: Warden (Social, AllWork) is open to every warcasket pawn today.
Firefighter, Patient, PatientBedRest and BasicWorker carry none of the blocked tags. So dropping
Caring unlocks Doctor and Childcare and nothing else. No installed Workshop mod adds a work type
tagged Caring or ManualDumb.

Childcare is not medical work (its work givers are BringBabyToSafety, BreastfeedBaby,
CarryToBreastfeed, PlayWithBaby, BottleFeedBaby and ChildcarerTeach), so the Medic keeps it
blocked by work type (4.3).

### 2.3 Hauling is a Harmony exemption keyed on VFEP's def

`VFEPirates.Trait_AllowsWorkType_Patch` postfixes the private `Trait.AllowsWorkType` (patched by
name) and returns true for Hauling and Hunting when `__instance.def ==
VFEP_DefOf.VFEP_WarcasketTrait`. The Hunting half is a no-op (Hunting carries no blocked tag).
A trait with a different def does not get the exemption, so a copied profile silently loses
hauling.

Replacing ManualDumb by Cleaning in the copy keeps the work tab identical, because ManualDumb
covers only Hauling and Cleaning. It is not identical everywhere ManualDumb is read as a tag
(2.4): the Medic pilot does opportunistic hauling, which VFEP's pilots never do, the Bio tab
lists Cleaning rather than Dumb labor under "Incapable of", and a modded work type tagged
ManualDumb would open for the Medic. These are accepted: a utility suit that picks up an item
on its way past fits the role, and the alternative costs a Harmony patch (section 3).

`VFEPirates.Pawn_GetDisabledWorkTypes_Patch` re-adds trait disables for slaves. It keys on
`IsWearingWarcasket()`, not on the trait def, and then iterates every trait on the pawn, so it
covers a foreign warcasket trait as well.

### 2.4 Where the game reads the block

- Work tab and `Pawn_WorkSettings`: `Pawn.GetDisabledWorkTypes` (cached, cleared by
  `Notify_DisabledWorkTypesChanged`) via `Trait.GetDisabledWorkTypes`, which reads
  `def.disabledWorkTypes` plus every WorkTypeDef the private `AllowsWorkType` rejects
  (`def.disabledWorkTags & workDef.workTags`). Both fields are read live from the def on every
  refill; nothing is cached on the TraitDef.
- Bio tab "Incapable of" and skill greying: `Pawn.CombinedDisabledWorkTags`, which ORs
  `Pawn_StoryTracker.DisabledWorkTagsBackstoryAndTraits` (the traits' `disabledWorkTags`, read
  directly, uncached) with kind, title, role and hediff tags. `disabledWorkTypes` never reaches
  it, so a work type blocked that way shows only in the work tab.
- Tag checks in job code: `Pawn.WorkTagIsDisabled` reads the same combined tags. The one that
  matters here is `Pawn_JobTracker.TryOpportunisticJob`, which skips opportunistic hauling when
  `ManualDumb | Hauling | AllWork` is disabled.
- VEF's `ApparelExtension.workDisables` reaches only the first path. Apparel-level disables never
  show in the bio or grey a skill.

### 2.5 Ecosystem survey

| Mod (Workshop id)                                                                       | What it does                                                                                                                                                                                                                                                                                                                                                                                | Consequence for us                                                                                                                                                                         |
| --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| WarCasket Expanded (2733135485)                                                         | Copies VFEP's trait block on most torsos; Centurion grants no equip trait                                                                                                                                                                                                                                                                                                                   | Precedent for "copy block"; nothing to do                                                                                                                                                  |
| Mechlord Warcasket (3087911691)                                                         | Own `MechanitorWarcasketTrait` (own tag set, `painFactor 0`, no `apparelExclusiveTrait`), `VFEP_Shellcasket` on unequip                                                                                                                                                                                                                                                                     | The idiom we adopt                                                                                                                                                                         |
| Alpha Mechs (2973169158)                                                                | Own `VFEP_WarcasketTrait_Mech`, injected under a Biotech `PatchOperationFindMod`; `VFEP_Shellcasket` on unequip                                                                                                                                                                                                                                                                             | Same idiom                                                                                                                                                                                 |
| Warcaskets can work! (2730151836)                                                       | `PatchOperationRemove` of `/Defs/TraitDef[defName="VFEP_WarcasketTrait"]/disabledWorkTags`; a conditional copy for Alpha Mechs' trait                                                                                                                                                                                                                                                       | Patches VFEP's def by xpath; will never target ours                                                                                                                                        |
| Warcaskets can haul! (2730543694)                                                       | Removes the `li`s and re-adds ManualSkilled, Animals, Artistic, Caring, Intellectual; conditional copy for Alpha Mechs                                                                                                                                                                                                                                                                      | Same                                                                                                                                                                                       |
| VFE Pirates - Hardworking Warcaskets (3535194807, `dmp.vfepirates.warcasketwork`, 1.6) | **Unconditional** `PatchOperationReplace` of the trait's `disabledWorkTags` with an empty node, plus a replaced description; three VEF `PatchOperationToggableSequence`s (two default-on) adding `equippedStatOffsets` to the three abstract bases: WorkSpeedGlobal -0.25 / -0.15 / -0.10 on armor / shoulders / helmet, carry capacity +10 / +5, and an off-by-default labour profile. `loadAfter` VFEP only. | Trait patch is not toggleable: with this mod active VFEP's profile is empty. The stat offsets land on the abstract bases we inherit, so Medic pieces take the same work-speed malus as VFEP's own; nothing to do |

The two patch mods and Hardworking all shaped the Alpha Mechs trait by adding a conditional
patch on *their* side. Nobody will do that for a trait nobody has heard of, and reimplementing
their patches behind `ModsConfig.IsActive` checks would track three mods' behaviour forever and
double-apply when they update. The design below needs none of that.

Weapon gating is unaffected by the trait choice. VFEP's warcasket weapons carry a
`VEF.Weapons.HeavyWeapon` extension whose `supportedTraits` lists only `VFEP_WarcasketTrait`,
which looks like a gap for any own trait, but VEF's `EquipmentUtility_CanEquip_Patch.CanEquip`
falls through to the worn apparel and accepts any torso-group piece with trade tag `Warcasket`,
which our armor inherits from `VFEP_WarcasketPartBase`. Mechlord and Alpha Mechs rely on the
same path.

## 3. Options rejected

- **VEF `workDisables` on our extension.** `ApparelExtension.Merge` ORs `workDisables` and unions
  the trait lists; a child can add but never subtract what a granted trait blocks, and apparel
  disables are invisible in the bio and skills (2.4). Cannot express "VFEP's profile minus
  Doctor".
- **Keep VFEP's trait, re-allow Doctor from a Harmony postfix** on `Pawn.GetDisabledWorkTypes`
  or `Trait.AllowsWorkType` keyed on the worn torso. The work tab would allow Doctor while the bio
  still lists Caring under "Incapable of" and greys Medicine, because `CombinedDisabledWorkTags`
  reads the trait's tags directly. Patching that too means patching `CharacterCardUtility`'s
  private cause builders for one def. Not worth the surface.
- **Own trait with a hardcoded profile.** Correct in isolation but deaf to the patch mods: a
  "can work!" user gets a Medic who can tend and nothing else while every other warcasket is
  fully able. This is the Mechlord/Alpha Mechs state today, and the thing the mirror fixes. The
  same holds for the non-work effects: VFEP's rest-fall factor arrived with its 1.7 tree, and a
  hardcoded copy would miss the next such change.
- **Own trait filled by an XML copy at load.** Vanilla has no copy operation, and even with one
  the result would depend on load order: neither patch mod declares `loadAfter` for us, so a
  load-time copy taken before they run would miss them. The copy has to happen after all patches,
  which means startup code.
- **Exact hauling parity through our own copy of VFEP's `AllowsWorkType` postfix**, keeping
  ManualDumb in the profile. It would remove the deviations listed in 2.3, all of which are
  acceptable or welcome for this suit, at the cost of a Harmony patch that would also have to
  follow VFEP's exemption list by hand.

## 4. Design

Own trait, Mechlord shape, with the work profile and the degree's effects derived at startup
from VFEP's trait.

### 4.1 TraitDef (`1.7/Defs/TraitDefs/WarcasketTrait_Medic.xml`, one def per file)

```xml
<TraitDef>
  <defName>MDWC_WarcasketTrait_Medic</defName>
  <commonality>0</commonality>
  <!-- Defaults equal what the startup mirror produces from an unpatched VFEP_WarcasketTrait,
       so the def reads correctly on its own; the mirror overwrites them (contract in the
       header). -->
  <disabledWorkTags>
    <li>ManualSkilled</li>
    <li>Cleaning</li>
    <li>Animals</li>
    <li>Artistic</li>
    <li>Intellectual</li>
  </disabledWorkTags>
  <disabledWorkTypes>
    <li MayRequire="Ludeon.RimWorld.Biotech">Childcare</li>
  </disabledWorkTypes>
  <degreeDatas>
    <li>
      <label>medic warcasket</label>
      <description>...</description>   <!-- see 4.4 -->
      <painFactor>0</painFactor>
      <statFactors>
        <RestFallRateFactor>1.3</RestFallRateFactor>
      </statFactors>
    </li>
  </degreeDatas>
  <modExtensions>
    <li Class="VEF.Pawns.TraitExtension">
      <apparelExclusiveTrait>true</apparelExclusiveTrait>
    </li>
  </modExtensions>
</TraitDef>
```

`apparelExclusiveTrait true` (VFEP has it; Mechlord and Alpha Mechs do not) stops trait-adding
mods and the trait-rolling code paths from ever handing the trait to a pawn outside the suit.
The extension mirrors VFEP's literally, `painFactor` omitted and therefore 0 (2.1); the header
says so, so nobody "fixes" it by adding `painFactor 1`.

### 4.2 Armor def

`Warcasket_Medic.xml`: `traitsOnEquip` becomes `MDWC_WarcasketTrait_Medic`; `traitsOnUnequip`
keeps `VFEP_Shellcasket` (the shared "cut out of a warcasket" state, as every add-on does). The
`priority 1` note in its header stays valid. Shoulders and helmet are unchanged. Header gains the
why: own trait because the profile must differ from VFEP's and a child extension can only add.

### 4.3 Startup mirror

A new idempotent `WorkProfile.Mirror()` (name to taste; `Source/1.7/`), called first from
the existing `Startup.Run`, against the current DefDatabase:

1. `vfep = DefDatabase<TraitDef>.GetNamedSilentFail("VFEP_WarcasketTrait")`,
   `ours = DefDatabase<TraitDef>.GetNamedSilentFail("MDWC_WarcasketTrait_Medic")`. If either is
   null, `Log.Error("[Medic Warcasket] ...")` and return (VFEP is a hard dependency, so this is a
   broken install, not a supported state).
2. **Tags.** `tags = vfep.disabledWorkTags`; if `tags` has ManualDumb, clear it and set Cleaning
   (keeps VFEP's hauling exemption, 2.3); clear Caring. `ours.disabledWorkTags = tags`.
3. **Work types.** `ours.disabledWorkTypes = new List<WorkTypeDef>(vfep.disabledWorkTypes)`
   (copy by value, never alias VFEP's list). If VFEP's tags carried Caring, add every
   WorkTypeDef other than Doctor whose `workTags` has Caring and that `tags` does not already
   block (`(tags & w.workTags) == 0`). Then remove Doctor. This re-blocks exactly what the
   Caring drop opened, other than Doctor: Childcare where Biotech is active, and any modded
   Caring work type. When a patch mod has already removed Caring from VFEP's tags, nothing is
   re-blocked, so the Medic does childcare exactly when every other warcasket does. The check
   is scoped to Caring on purpose: a generic "VFEP blocks it, `tags` does not" diff would also
   catch Hauling, which step 2's swap opens deliberately.
4. **Degree effects.** Copy every public instance field of `vfep.degreeDatas[0]` onto
   `ours.degreeDatas[0]` except the text (`label`, `labelMale`, `labelFemale`,
   `untranslatedLabel`, `description`), by reflection so a field VFEP starts using later is
   carried too; `List<T>` values are copied into new lists. Today that carries `painFactor 0`
   and `RestFallRateFactor 1.3`. The copy runs after `ResolveReferences`, so VFEP's values are
   already resolved.
5. **Extension.** Copy every public field of VFEP's `VEF.Pawns.TraitExtension` onto ours
   (`painFactor`, `apparelExclusiveTrait` today), for the same reason.
6. One `Log.Message` with the resulting tags and work types, so a bug report's log shows the
   profile the player actually got.

Why nothing more is needed: `Trait.AllowsWorkType`, `Trait.GetDisabledWorkTypes`, the degree's
stat and pain fields and VEF's extension are all read live from the def (2.1, 2.4),
`Pawn.GetDisabledWorkTypes` is cached per pawn and no pawn exists when startup code runs, and
`CombinedDisabledWorkTags` is uncached.

**Where it runs.** `Startup.Run` already carries the def-derived startup contract: `ModInit`
calls it once for the first load and the `StaticConstructorOnStartupUtility.CallAll` postfix
calls it on every play-data reload (their headers carry the reasoning, including why the first
load needs the direct call). The mirror needs no plumbing of its own; `Startup`'s header gains
it in its list of what runs.

Ordering guarantee: every XML patch, including VEF toggleable ones, applies during def loading,
before `CallAll`. So the mirror sees VFEP's trait exactly as the full mod list leaves it, in any
load order. That is the whole compatibility story:

| Active alongside us    | VFEP's trait tags after patches                                     | Medic trait after the mirror                                           | Medic can                                                          |
| ---------------------- | ------------------------------------------------------------------- | ---------------------------------------------------------------------- | ------------------------------------------------------------------ |
| nothing                | ManualSkilled, ManualDumb, Animals, Artistic, Caring, Intellectual | tags ManualSkilled, Cleaning, Animals, Artistic, Intellectual; Childcare | doctor, warden, haul (opportunistically too), fight fires         |
| Warcaskets can haul!   | ManualSkilled, Animals, Artistic, Caring, Intellectual             | tags ManualSkilled, Animals, Artistic, Intellectual; Childcare          | the above plus clean                                               |
| Warcaskets can work!   | none                                                                | none                                                                   | everything, childcare included                                     |
| Hardworking Warcaskets | none (empty node)                                                   | none                                                                   | everything, childcare included, at its work-speed malus           |

### 4.4 Player-facing text

Label `medic warcasket` (shows in the Bio traits list). Description in VFEP's register with
`{PAWN_nameDef}` grammar, stating the pain immunity, that the suit is built around treating the
wounded so the pilot keeps their medical and social faculties, and that other work is beyond the
suit. It must stay true under every row of the table above, so it should describe the suit's
intent rather than enumerate blocked work types, and it must not promise care for children.
No em dashes.

### 4.5 Localization surface

A TraitDef adds a fourth DefInjected folder beside `ThingDef`, `VEF.Abilities.AbilityDef` and
`JobDef`: `1.7/Languages/<Language>/DefInjected/TraitDef/`, with
`MDWC_WarcasketTrait_Medic.degreeDatas.0.label` and `.description`. After the def lands:
regenerate `Scripts/expected-injections.json` (the checker refuses stale expectations), add the
folder to the def-type list in `.claude/skills/translate/SKILL.md` and to CLAUDE.md's
Localization Toolchain sentence that names the def types, and seed the glossary with VFEP's own
trait label as the term to mirror ("warcasket" trait in each language). No `DEF_TYPE_ALIASES`
change: `TraitDef` is a real database. The mirror copies no text, so nothing it writes is
translatable.

### 4.6 Documentation touchpoints

- Trait def header: the mirror contract (what is copied, the tag swap, the Caring re-block, the
  degree and extension copies, where it runs) and the `painFactor` trap (4.1).
- Armor def header: why an own trait.
- CLAUDE.md Conventions: one bullet that the trait's work profile, degree effects and extension
  are overwritten at startup from VFEP's and must not be patched or edited expecting it to stick,
  pointing at the def header and the mirror. Cross-file coupling, so it belongs there.
- `.steamworkshop/Description/English.txt`: the set's selling point. Say the wearer can do medical
  work and that the "can work!" / "can haul!" / Hardworking mods apply to it like any warcasket.
- `CHANGELOG.md`: nothing now (the file has no Unreleased section); the release section says it.

## 5. Residuals and knobs

- **Opportunistic hauling** is a deliberate difference from VFEP's pilots (2.3). The Workshop
  description need not mention it.
- **Cleaning** stays blocked for parity with VFEP's wearers. If a medic should be able to clean
  the hospital, step 2 of the mirror simply does not set Cleaning. One line; a design call.
- **A configurable profile.** Doctor open and Childcare re-blocked is a fixed choice. A settings
  section with toggles over the Caring work types (medical, childcare, any other caring work) is
  ledgered in `TODOs.md`; it would feed step 3's "other than Doctor" set and run through
  `ApplyToDefs` like every other setting, since the mirror writes def fields.
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
  Intellectual blocked, pain, rest-fall factor, market value hit) and re-entombing in any
  warcasket cures it.
- **Raid presence** (`TODOs.md`): veteran raiders wearing the set get the trait on generation
  through `Wear`; AI never does colony work, so no behavioural change.

## 6. Verification

Dev-mode harness, game closed between runs, VFEP chain plus Biotech plus this mod:

1. Entomb a colonist in the Medic set via the foundry (or dev-wear the three pieces). Work tab:
   Doctor assignable, Warden assignable, Hauling and Firefighter assignable, Childcare,
   Cleaning, Construction, Research, Cooking and Handling disabled. Bio tab: Traits shows
   "medic warcasket"; "Incapable of" lists no Caring entry and does list Cleaning; Medicine
   skill not greyed. Health tab: pain stays 0 after a dev hit. Info card stats: rest fall rate
   factor at 130%, as on a Siegebreaker pilot.
2. Tend a wounded pawn and perform a surgery; feed a patient.
3. Remove the set with the surgery: Shellcasket trait appears; re-entomb in a Siegebreaker:
   Shellcasket gone, VFEP trait present, Doctor disabled again.
4. Repeat step 1 with "Warcaskets can haul!" active, then with "Warcaskets can work!", then with
   Hardworking Warcaskets: work tab matches the table in 4.3 (Childcare open under the last two);
   the startup log line shows the derived profile; no red patch errors from any of the three.
5. `python3 Scripts/integration-smoke-test.py` clean; `python3 Scripts/check-translations.py`
   clean after the sidecar regen.
6. Slave wearing the set: Doctor allowed, Construction blocked (exercises VFEP's slave patch
   with a foreign trait).

## 7. Implementation checklist

1. `1.7/Defs/TraitDefs/WarcasketTrait_Medic.xml` (4.1) with header.
2. `Warcasket_Medic.xml`: swap `traitsOnEquip`; header note (4.2).
3. `Source/1.7/WorkProfile.cs` with `Mirror()` (4.3); `Startup.Run` calls it first and its
   header lists it.
4. Build (warnings are errors), deploy, run section 6.
5. Regenerate the sidecar, update the translate skill def-type list and glossary, and CLAUDE.md's
   def-type sentence (4.5).
6. CLAUDE.md bullet, Workshop description (4.6).
7. Close the `TODOs.md` entry; the configurable-profile follow-up stays open.
