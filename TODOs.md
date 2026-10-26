# TODOs

- Review in-flight description edits
- Test mechanite dressing cooldown settings
- Mod setting for Adrenaline shot autocast beginning on/off? tied to whether the torso is equipped?
- what value for mechanite dressing cast range (cells) would allow 2 cells directly along x/z, but only 1 cell diagonally

## Before the first release

- **Tune the set.** Decide what a medic suit gives up and gains, then price the three pieces
  against Siegebreaker, Guardian, Sarcophagus and Brute at market value (Shipcracker's armor
  header is the format, `VFEP_WARCASKET_STATS.md` the tables). Folds in:
  - the passives (`MedicalTendSpeed` +0.25, `MedicalTendQuality` +0.10 are first guesses)
  - welding costs, the free first fills included: five glitterworld medicine kits in the
    armor, three industrial medicine kits in the shoulders
  - the helmet: does its Sight floor overlap the Sarcophagus helmet? could the floor be lower?
  - with adrenaline shot switched gojuice->medicine, should the high be nerfed a bit/should we
    add a final crash stage?
- **Descriptions.** The three def descriptions are theme placeholders. Rewrite around the final
  features, shared paragraphs identical across the pieces, each `shortDescription` equal to
  its first paragraph. The ability description and the Keyed strings are first drafts.
- CE Patch similar to Shipcracker/Toxin once they're tested/released
- **Smoke test on 1.6**
- **Full review of player-facing copy**
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Later

- **Shock shoulders upstream.** VFEP's Shock shoulders grant VFEP's trait by themselves, so a
  Medic wearing them loses Doctor (same spec, section 5). Propose moving the lists off the
  shoulders upstream; no local patch.
- **Dressing sounds.** The spec's sounds are placeholders (`Mechanite-Dressing.md` 3.5):
  the serum's `MechSerumUsed` at the warmup, vanilla's `Interact_Tend` loop through the warmup
  and the beam, and `TechMedicineUsed` when the beam connects. Review whether a beam-like
  sustainer, a weapon's if nothing else fits, suits the stream better, and check the layering
  and volumes. A DLC sound needs a fallback for when the DLC is absent. The shot plays
  vanilla's injection (`Ingest_Inject`) at the patient; hear it against the dressing's.
- **Art.** Ability gizmos switch from their reload items' icons once commissioned art lands
- **Raid presence.** The pieces carry `WarcasketVeteran` and `WarcasketAll` like Siegebreaker;
  decide whether a medic set stays on the raid tables (AI never casts apparel abilities, can we make them?).
- **Unit tests.** Siblings carry a headless xUnit net472 suite (`Tests/1.6/<Mod>.Tests.csproj`);
  `Tunable.Snap` with step bands is worth covering (Shipcracker's `TunableSnapTests`).
- **VFEP vocabulary upstream.** It is seeded in three family glossaries; move it into
  `rimworld-l10n` before a fourth copies it.
