# SPEC: Intellectual work toggles for the medic pilot

> Implemented, pending an in-game test. The code, its def headers and its tests are the tested current state and supersede this spec wherever the two differ.

Status: IMPLEMENTED on `main` with decisions 5.2 to 5.4 taken as recommended; 5.1 (keep it or
roll it back) waits on the in-game checks in section 6, ledgered in `TODOs.md`. Facts verified against
the installed RimWorld 1.7.5000 rev1275 (`Assembly-CSharp.dll` via `ilspycmd`, Core and Anomaly
XML), the frozen 1.6 assemblies at `~/.local/share/RimWorld-1.6/Managed` (every call path in
section 2 reads the same on both), VFE Pirates at be4040e (v4.03) and this repo at 9bf5a90
(v1.0.0). Builds on `Docs/Specs/Medic-Work-Trait.md` (the mirror and the Caring toggles); read
its sections 2 and 4.3 for the mechanism this extends.

## 1. Origin and goal

A Workshop comment (Simplified Chinese) asked:

> 能不能让医疗战棺可以进行智识工作，进入医疗战棺的小人与射击专家的工作类型相比就少了个智识

"Could the medic warcasket do Intellectual work? Compared with a shooting specialist, a pawn in
the medic warcasket is only missing Intellectual." 射击专家 is Ideology's shooting specialist role
(`IdeoRole_ShootingSpecialist`), whose `roleDisabledWorkTags` are Crafting, Cooking, PlantWork,
Mining and Constructing, so it keeps research open.

The Intellectual block is not ours. VFEP's `VFEP_WarcasketTrait` blocks the Intellectual tag for
every pilot, and the medic trait mirrors it. The goal, if pursued: let the player open the
Intellectual work types for medic pilots from the existing "Enabled work types" settings
section, **off by default**, so an unchanged install behaves exactly as v1.0.0.

Workaround available today, worth putting in a reply to the commenter: "Warcaskets can work!"
(2730151836) and VFE Pirates - Hardworking Warcaskets (3535194807) empty VFEP's tag list, and
the mirror carries that onto the medic trait, research included (`Medic-Work-Trait.md` 4.3
table).

## 2. Verified facts

### 2.1 What the Intellectual tag covers

| Work type  | Source  | `workTags`                       | `relevantSkills` |
| ---------- | ------- | -------------------------------- | ---------------- |
| Research   | Core    | Intellectual, Commoner, AllWork  | Intellectual     |
| DarkStudy  | Anomaly | Intellectual, Commoner, AllWork  | Intellectual     |

No other vanilla or DLC work type carries the tag or uses the skill. Doctor is Caring only, so
the two groups do not overlap in vanilla; a modded type could carry both.

### 2.2 Code that reads the tag rather than the work types

A full decompile of `Assembly-CSharp.dll` finds three non-def reads of `WorkTags.Intellectual`:

- `HackUtility.IsCapableOfHacking`: requires the Intellectual skill not `TotallyDisabled`
  **and** `!pawn.WorkTagIsDisabled(WorkTags.Intellectual)` (plus manipulation and the hacking
  speed stat).
- `CompUseEffect_InstallImplantMechlink`: shows a confirmation warning when the tag is
  disabled. Cosmetic.
- `PawnBioAndNameGenerator`: backstory selection at generation. Not reached by a trait granted on
  equip.

Every other Intellectual gate (reading, studying, analysing, the research job, xenogerm
creation, pilot consoles, scanners, Anomaly study and suppression) goes through the skill
(`SkillDefOf.Intellectual`), not the tag. Third-party mods may read the tag directly; none is
surveyed here.

### 2.3 The skill follows the work types

`SkillDef.IsDisabled(tags, disabledWorkTypes)` returns true when the tags block the skill's
`disablingWorkTags`, **or** when every work type listing the skill in `relevantSkills` is
disabled (the Intellectual skill does not set `neverDisabledBasedOnWorkTypes`). So with the tag
dropped but Research and DarkStudy blocked by type, the skill is still totally disabled, and
`IsCapableOfHacking` still fails on its skill check. Opening Research enables the skill (levels
and passions are kept while disabled, so a veteran researcher gets theirs back).

### 2.4 Refresh

`Pawn.Notify_DisabledWorkTypesChanged`, which `WorkProfile.Apply` already calls on every pawn
carrying the trait, clears the cached disabled work types and calls
`skills.Notify_SkillDisablesChanged`, so the skill greys or un-greys on the next frame.
`CombinedDisabledWorkTags` is uncached, so a tag change needs nothing extra.

## 3. Design

### 3.1 Mirror (`Source/1.6/WorkProfile.cs`, `MirrorFromVfep`)

Generalise the Caring handling to a fixed list of toggle tags, Caring and Intellectual. For each
tag: note whether VFEP's tags carry it, clear it from ours, and add every work type carrying it
that the remaining tags do not block to `toggleable` (one row per type, work tab order, a type
carrying both tags added once). Default rule per type:

- Doctor: open.
- Any other: open only when VFEP's tags carry none of the toggle tags the type has and VFEP's
  `disabledWorkTypes` does not list it.

Under an unpatched VFEP that gives Research and DarkStudy blocked by type, so the default profile
equals v1.0.0's except for where Intellectual is blocked (section 3.2). The scope stays a fixed
list on purpose, for the reason the Caring scope has: a generic "VFEP blocks it, we do not" diff
would re-block the Hauling the ManualDumb swap opens.

### 3.2 Writing the tag (`WorkProfile.Write`)

Today `Write` only rebuilds `disabledWorkTypes`. Two options (decision 5.2):

- **A. Keep the tag while nothing needs it (recommended).** Mirror stores the base tags with
  Intellectual still in them when VFEP had it; `Write` clears Intellectual only when at least
  one open toggleable type carries it. The default def is byte-identical to v1.0.0's, so any
  mod reading the tag sees no change unless the player opens research. Costs `Write` a tag
  computation and the mirror a second piece of state.
- **B. Always drop the tag, block by type.** Simpler, same shape as Caring. Vanilla behaviour
  is identical (2.3 keeps hacking blocked), but the mechlink warning disappears and a
  third-party mod reading the tag would see Intellectual open while research is blocked.

Under A, a pilot with research open can also hack, as any colonist with research can. That is
the vanilla coupling, not a leak.

### 3.3 Settings (`Source/1.6/Settings/WorkSetting.cs`, `Settings_Work.cs`)

No structural change. Overrides are saved by work type defName, so the new rows appear on their
own and need no save migration; an existing save's settings read as "all defaults". Comments
that say the toggles are the Caring work types need rewording.

### 3.4 Player-facing text

Per `l10n/languages/English.md` and the `player-text-vanilla-register` memory:

- `MDWC_WorkSummaryNone` ("The pilot does no caring work. ...") is wrong once a research row
  exists. Needs new wording (decision 5.4).
- `MDWC_SectionWork`, `MDWC_SectionWorkDesc`, `MDWC_WorkDesc`, `MDWC_WorkDefault` and
  `MDWC_WorkSummary` still read correctly.
- The trait description ("not capable of most work, but can still do doctor work") stays true:
  research is opt-in.
- Nine languages carry the Keyed file, so any English change needs a `/translate` pass before
  the release that ships it.

### 3.5 Unaffected

- Raider medics and the man in black: AI pawns do no colony work; the stranger kind's
  `requiredWorkTags` (Violent, Caring) do not involve Intellectual.
- Shellcasket: removal still gives `VFEP_Shellcasket`, which blocks Intellectual on its own.
- VFEP's Shock shoulders in a mixed set: VFEP's trait still blocks Intellectual on that pilot,
  as it already re-blocks Doctor (`Medic-Work-Trait.md` section 5).

## 4. Touchpoints

- `Source/1.6/WorkProfile.cs`: mirror, `Write`, header.
- `Source/1.6/Settings/WorkSetting.cs`, `Settings_Work.cs`: headers only.
- `1.6/Defs/TraitDefs/WarcasketTrait_Medic.xml`: header contract (tags line, work types line).
  Under 3.2 A the XML values stay as they are.
- `1.6/Languages/English/Keyed/`: `MDWC_WorkSummaryNone`, then `/translate` per language.
- `Docs/Specs/Medic-Work-Trait.md`: a pointer from section 5's "configurable profile" bullet.
- `.steamworkshop/`: a line in the settings description if it lists the toggles.
- `Tests/1.6/`: no test covers `WorkProfile` yet. Add one that the default profile equals the
  v1.0.0 def (tags and types), one that opening Research clears the tag (3.2 A) and puts the
  Intellectual skill back, and one that a type carrying both tags is listed once.
- `CHANGELOG.md`: minor version (new setting).

## 5. Open decisions

1. **Keep it** (open until the in-game test). VFEP blocks research for every pilot, and the medic's identity is
   doctoring. Opt-in and off by default keeps parity unless the player asks; the existing patch
   mods already cover players who want every warcasket working. Precedent for opt-in: Childcare.
2. **Tag handling**: 3.2 A (recommended) or B. Taken: A.
3. **Scope**: every Intellectual-tagged type as its own row (Research, DarkStudy, modded ones;
   the recommended reading of the fixed tag list) or Research only. Taken: every type.
4. **Summary wording** for `MDWC_WorkSummaryNone` and whether the summary should group rows
   (caring, intellectual) or keep listing work type names as it does now. Taken: names as now,
   and "The pilot can do none of the work below." for the empty case.
5. **Further tags.** The fixed tag list makes other VFEP blocks cheap to add later (Cleaning is
   already a noted design call in `Medic-Work-Trait.md` section 5; Cleaning would need care
   because of the ManualDumb swap). Out of scope unless asked for.

## 6. Verification (if built)

- `dotnet test Tests/1.6/MedicWarcasket.Tests.csproj` with the new `WorkProfile` tests.
- In game, VFEP only: defaults show Research and Dark study unticked, work tab and skill as in
  v1.0.0; ticking Research mid-game opens the column for an already entombed pilot, the
  Intellectual skill un-greys with its old level, a hackable is hackable; unticking reverses all
  three.
- In game with "Warcaskets can work!": both rows default to ticked, nothing blocked.
