# TODOs

Scoping notes for the work that has not landed yet. The set is a scaffold: three untuned
Siegebreaker copies with borrowed art and no abilities.

## Design (next session)

- **Tune the set.** Every stat, cost and comp on the three defs is VFEP's Siegebreaker's
  (`Docs/Research/VFEP_WARCASKET_STATS.md` tabulates the 10th-generation sets; the sibling
  Shipcracker repo's def headers show the tuning rationale format). Decide what a medic
  suit gives up and what it gains, then price it against Siegebreaker, Guardian, Sarcophagus
  and Brute at market value, as Shipcracker's armor header does.
- **Abilities and gizmos.** Deliberately absent for now. `Docs/Research/WARCASKET_ABILITIES.md`
  maps every VFEP set's mechanics and the VEF levers (`CompAbilitiesApparel`,
  `ApparelExtension`, `pawnCapacityMinLevels`, `equippedStatFactors`), the Sarcophagus set
  is the closest VFEP precedent for a support role. A medic direction to weigh: tend speed
  and quality factors on the shoulders, a capacity floor or a diagnostic stat on the helmet,
  and an activated ability on the armor (a field-tend, a stim, or an area heal via a
  hediff) granted through VEF's `CompProperties_AbilitiesApparel`. Any new def type the
  abilities add changes the DefInjected surface: regenerate the sidecar and update the
  translate skill's def-type list.
- **Descriptions.** The three descriptions are placeholders written around the theme; rewrite
  them once the abilities exist, keeping the shared second and third paragraphs identical
  across the three defs and each `shortDescription` equal to its first paragraph.
- **Art.** `texPath` / `wornGraphicPath` borrow VFEP's Siegebreaker textures. Custom art goes
  under `Textures/Things/Pawn/Warcasketlike/WarcasketMedic/` (armor, shoulders, helmet, each
  with `_north`/`_south`/`_east` facings plus the item icon), then repoint the three defs.
  `About/Preview.png` and `About/ModIcon.png` are also missing.
- Acquisition and raid presence: all three pieces carry `WarcasketVeteran` and `WarcasketAll`
  like Siegebreaker, so veteran raiders can field the set. Decide whether a medic set should
  stay on the raid tables (AI never casts apparel abilities, so it is flavor and threat only).

## Translation (deferred to shortly before release)

- No `1.6/Languages/` tree exists and no glossary files exist. The pass runs one language at a
  time via `/translate <Language>`; seed each glossary from the sibling Shipcracker repo's per
  `.claude/skills/translate/glossary/README.md`. The VFEP warcasket vocabulary is now shared by
  more than one family mod; consider moving it upstream into `rimworld-l10n` before a third
  copy is made.
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
  has the mono/copy-target notes). Add one once there is pure logic worth covering.
- **First Workshop publish.** Upload writes `About/PublishedFileId.txt`; commit it, add the
  Workshop link to the README's Installation section, fill the id into the README's
  commented-out Steam badges and uncomment them, and
  paste `.steamworkshop/Description/English.txt` into the page.
