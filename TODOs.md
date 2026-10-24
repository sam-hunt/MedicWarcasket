# TODOs

Work that has not landed. The mechanics are in (doctor passives, field dressing, settings,
the medic work trait); every stat and cost is still Siegebreaker's. Specs live in
`Docs/Specs/`, the VFEP stat tables and ability roster in `Docs/Research/`.

## Before the first release

- **Tune the set.** Decide what a medic suit gives up and gains, then price the three pieces
  against Siegebreaker, Guardian, Sarcophagus and Brute at market value (Shipcracker's armor
  header is the format, `VFEP_WARCASKET_STATS.md` the tables). Folds in:
  - the passives (`MedicalTendSpeed` +0.25, `MedicalTendQuality` +0.10 are first guesses)
  - the dressing's medicine XP (60 per cast, chosen without play testing)
  - welding costs, the free first fill of five industrial medicine included
  - the helmet: does its Sight floor and tend quality overlap the Sarcophagus helmet?
- **Adrenaline shot.** The Workshop page already names it on the shoulders, so it ships with
  the release that carries that text. Scope: a shoulder-granted stimulant, pain suppressant or
  anaesthetic shot, perhaps autocast on the target when a field dressing completes.
- **Descriptions.** The three def descriptions are theme placeholders. Rewrite around the final
  features, shared paragraphs identical across the pieces, each `shortDescription` equal to
  its first paragraph. The ability description and the Keyed strings are first drafts.
- **In-game verification.** Never run; both features were built against decompiles only.
  - Field dressing: `Field-Dressing.md` section 6. Watch the stun on a drafted ally, a downed
    raider and a pet; the bed patient's quality offset; the reload switching with the
    selection; a save mid-walk.
  - Medic trait: `Medic-Work-Trait.md` section 6, fresh entombings only. Watch the work tab
    under each patch mod, the Bio tab's "Incapable of", the slave case.
- **Translations.** English Keyed only so far. One language at a time via
  `/translate <Language>` once the English is final (defs, Keyed, Workshop page); the
  glossaries are seeded. The l10n pin (v1.5.0) moves at release.
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Later

- **Configurable work profile.** Settings toggles over the Caring work types instead of the
  fixed "Doctor open, Childcare blocked" (`Medic-Work-Trait.md` section 5); they would feed the
  mirror through `ApplyToDefs`.
- **Shock shoulders upstream.** VFEP's Shock shoulders grant VFEP's trait by themselves, so a
  Medic wearing them loses Doctor (same spec, section 5). Propose moving the lists off the
  shoulders upstream; no local patch.
- **Dressing polish.** A cast effect (VEF `castFleck` / `fleckOnTarget` or an effecter); a gizmo
  texture with the medicine type overlaid instead of the medicine's item icon.
- **Art.** Ability gizmos; the hero art's arm injectors.
- **Raid presence.** The pieces carry `WarcasketVeteran` and `WarcasketAll` like Siegebreaker;
  decide whether a medic set stays on the raid tables (AI never casts apparel abilities).
- **Unit tests.** Siblings carry a headless xUnit net472 suite (`Tests/1.6/<Mod>.Tests.csproj`);
  `Tunable.Snap` with step bands is worth covering (Shipcracker's `TunableSnapTests`).
- **VFEP vocabulary upstream.** It is seeded in three family glossaries; move it into
  `rimworld-l10n` before a fourth copies it.
