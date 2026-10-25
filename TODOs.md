# TODOs

## Mechanite dressing

- review gizmo tooltip text

## Before the first release

- **Tune the set.** Decide what a medic suit gives up and gains, then price the three pieces
  against Siegebreaker, Guardian, Sarcophagus and Brute at market value (Shipcracker's armor
  header is the format, `VFEP_WARCASKET_STATS.md` the tables). Folds in:
  - the passives (`MedicalTendSpeed` +0.25, `MedicalTendQuality` +0.10 are first guesses)
  - ~~the dressing's medicine XP (60 per cast, chosen without play testing)~~
  - welding costs, the free first fill of five glitterworld medicine kits included
  - the helmet: does its Sight floor overlap the Sarcophagus helmet? could the floor be lower?
- **Adrenaline shot.** The Workshop page already names it on the shoulders, so it ships with
  the release that carries that text. Scope: a shoulder-granted stimulant, pain suppressant or
  anaesthetic shot, perhaps autocast on the target when a mechanite dressing completes.
- **Descriptions.** The three def descriptions are theme placeholders. Rewrite around the final
  features, shared paragraphs identical across the pieces, each `shortDescription` equal to
  its first paragraph. The ability description and the Keyed strings are first drafts.
- **In-game verification.** Never run; the features were built against decompiles only.
  - Mechanite dressing: the verification list in `Mechanite-Dressing.md` section 6.
  - Medic trait: `Medic-Work-Trait.md` section 6, fresh entombings only. Watch the work tab
    under each patch mod, the Bio tab's "Incapable of", the slave case.
  - On 1.6 as well as 1.7: the mechanite dressing was specced against the 1.7 install alone,
    on Ludeon's word that 1.6 behaves the same. Rerun its visuals and assets checks there.
- **Mod settings section title/icon tooltip ability descriptions**, similar to Toxin
- **Trait label should drop "medic" from "medic warcasket"** and tighten tooltip prose
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Later

- **Configurable work profile.** Settings toggles over the Caring work types instead of the
  fixed "Doctor open, Childcare blocked" (`Medic-Work-Trait.md` section 5); they would feed the
  mirror through `ApplyToDefs`.
- **Shock shoulders upstream.** VFEP's Shock shoulders grant VFEP's trait by themselves, so a
  Medic wearing them loses Doctor (same spec, section 5). Propose moving the lists off the
  shoulders upstream; no local patch.
- **Dressing sounds.** The spec's sounds are placeholders (`Mechanite-Dressing.md` 3.5):
  the serum's `MechSerumUsed` at the warmup, vanilla's `Interact_Tend` loop through the warmup
  and the beam, and `TechMedicineUsed` when the beam connects. Review whether a beam-like
  sustainer, a weapon's if nothing else fits, suits the stream better, and check the layering
  and volumes. A DLC sound needs a fallback for when the DLC is absent.
- **Art.** Ability gizmos; until the dressing's icon lands, the medicine's item icon stands in
  on the gizmo, the cursor and the settings header (`Mechanite-Dressing.md` 3.8).
- **Raid presence.** The pieces carry `WarcasketVeteran` and `WarcasketAll` like Siegebreaker;
  decide whether a medic set stays on the raid tables (AI never casts apparel abilities, can we make them?).
- **Unit tests.** Siblings carry a headless xUnit net472 suite (`Tests/1.6/<Mod>.Tests.csproj`);
  `Tunable.Snap` with step bands is worth covering (Shipcracker's `TunableSnapTests`).
- **VFEP vocabulary upstream.** It is seeded in three family glossaries; move it into
  `rimworld-l10n` before a fourth copies it.
