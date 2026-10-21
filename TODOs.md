# TODOs

Scoping notes for the work that has not landed yet. The set's mechanics are in (doctor
passives, the field dressing, mod settings); every stat and cost is still Siegebreaker's.

## Design

- **Medical work for the wearer.** Own `MDWC_WarcasketTrait_Medic` in the Mechlord/Alpha Mechs
  idiom, with its `disabledWorkTags` derived at startup from VFEP's trait minus Caring (and
  ManualDumb swapped for Cleaning to keep VFEP's hauling exemption), so "Warcaskets can work!",
  "can haul!" and Hardworking Warcaskets apply to the Medic unchanged. Spec, verified facts,
  rejected options and the verification plan: `Docs/Specs/Medic-Work-Trait.md`. Review the spec,
  then implement its section 7 checklist.
- **Tune the set.** Every stat, cost and comp on the three defs is VFEP's Siegebreaker's
  (`Docs/Research/VFEP_WARCASKET_STATS.md` tabulates the 10th-generation sets; the sibling
  Shipcracker repo's def headers show the tuning rationale format). Decide what a medic
  suit gives up and what it gains, then price it against Siegebreaker, Guardian, Sarcophagus
  and Brute at market value, as Shipcracker's armor header does.
- **Field dressing follow-ups.** The passives and the dressing are implemented per
  `Docs/Specs/Field-Dressing.md` (its sections 2 to 5 remain the reference; where the code
  departs from the spec the file headers say so: click range unlimited with the walk gated by
  `distanceToTarget`, an own JobDef so a bed patient stays in bed, no `defaultMedicine` prop,
  own `MDWC_WoundsTended` text since Biotech owns `NumWoundsTended`).
  `Docs/Research/WARCASKET_ABILITIES.md` is the roster context.
  - In-game verification: run the spec's section 6 list (never run; the implementation was
    built and reviewed against decompiles only). Watch in particular: the stun on a drafted
    ally, a downed raider and a pet; the bed patient's quality offset; the automatic reload
    switching medicine with the selection; a save mid-walk.
  - Medicine XP: 60 per cast (one base-length tend job's worth), chosen without play testing;
    confirm or retune against what a medic actually earns per fight.
  - Passive offset sizes: `MedicalTendSpeed` +0.25 and `MedicalTendQuality` +0.10 are first
    guesses; retune with the set (folds into "Tune the set").
  - Healing beam: add a cast effect for the field dressing (VEF `castFleck` / `fleckOnTarget`,
    or an effecter) once the ability is signed off.
  - Auxiliary shot: consider a shoulder-granted stimulant / pain suppressant / anaesthetic
    shot that autocasts on the target when a field dressing completes.
  - Helmet: consider whether its Sight floor and tend quality overlap the Sarcophagus helmet
    or leave it under-attractive against the tier's alternatives.
  - Gizmo art: consider a custom ability texture with the medicine type as a partial overlay
    instead of the medicine's item icon.
  - Welding costs: balance pass on all three pieces' costLists once the abilities are
    signed off, the free first fill of five industrial medicine included (folds into "Tune the
    set").
- **Descriptions.** The three descriptions are placeholders written around the theme; rewrite
  them now that the abilities exist, keeping the shared second and third paragraphs identical
  across the three defs and each `shortDescription` equal to its first paragraph. The ability
  def's description and the Keyed strings are first drafts in the same register.
- **Art polish.** `About/Preview.png`, ability gizmos, hero art's arm injectors revision
- Acquisition and raid presence: all three pieces carry `WarcasketVeteran` and `WarcasketAll`
  like Siegebreaker, so veteran raiders can field the set. Decide whether a medic set should
  stay on the raid tables (AI never casts apparel abilities, so it is flavor and threat only).

## Translation (deferred to shortly before release)

- Only the English Keyed file exists under `1.6/Languages/`. The pass runs one language at a
  time via `/translate <Language>`, after the English is final; every glossary is seeded with the
  family's VFEP rows (mirrored from Toxin Warcasket's seeds) and lists what the pass must
  still ground. The l10n pin is at v1.5.0. The VFEP warcasket vocabulary now lives in three
  family repos; move it upstream into `rimworld-l10n` before a fourth mod copies it.
- `.steamworkshop/Description/English.txt` is a draft; rewrite it with the final feature set
  before the pass translates it.

## Infrastructure follow-ups

- **Cut a release candidate to exercise CI before the real release.** The release
  workflow fetches VEF and VFEP from the Workshop with SteamCMD (anonymous login) and injects
  them via `VEF_PATH` / `VFEP_PATH`, but it has never run in this repo. `/release major rc` tags
  `v1.0.0-rc.1`: a GitHub prerelease that needs no CHANGELOG section. Check the Workshop
  fetch step, the translation gate and the zip's contents.
- **Unit tests.** Five siblings carry a headless xUnit net472 suite at
  `Tests/1.6/<Mod>.Tests.csproj` (Krafs ref, no live game; XenogermTraderStock's CLAUDE.md
  has the mono/copy-target notes). There is now pure logic worth covering: `Tunable.Snap`
  with step bands (Shipcracker's `TunableSnapTests` is the template) and the suite's bucket
  accounting if it is ever split from the comp.
- **Tunable.cs has diverged from the family template.** This repo's copy adds step bands
  (the cast-time slider's 0.25 / 0.5 / 1 s steps); Shipcracker's is the template and Toxin's
  is older still. Backport the bands upstream so the three copies converge again.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it, add the
  Workshop link to the README's Installation section, fill the id into the README's
  commented-out Steam badges and uncomment them, and
  paste `.steamworkshop/Description/English.txt` into the page.
