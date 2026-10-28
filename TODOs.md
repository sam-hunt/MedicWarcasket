# TODOs

## Before v1.0.0

- **Dressing sounds.** The spec's sounds are placeholders (`Mechanite-Dressing.md` 3.5):
  the serum's `MechSerumUsed` at the warmup, vanilla's `Interact_Tend` loop through the warmup
  and the beam, and `TechMedicineUsed` when the beam connects. Review whether a beam-like
  sustainer, a weapon's if nothing else fits, suits the stream better, and check the layering
  and volumes. A DLC sound needs a fallback for when the DLC is absent. The shot plays
  vanilla's injection (`Ingest_Inject`) at the patient; hear it against the dressing's.
- **Unit tests.** `Tests/1.6/` holds the compat xpath test only; `Tunable.Stops` and `Nearest`
  are worth covering (Shipcracker's `TunableSnapTests` covers its banded `Snap`).
- **Smoke test on 1.6**
- **Full review of player-facing copy**
- **Workshop images** Shipcracker and Toxin as precedent
- **First Workshop publish.** Commit `About/PublishedFileId.txt`, add the Workshop link and the
  Steam badges to the README, paste the description.

## Later

- **Raider medics.** The set is on the raid tables as a Siegebreaker with the baseline shield;
  AI pawns never cast apparel abilities. Toxin's raider auto-vent is the family precedent for
  giving raiders an ability, so a raider medic dressing their downed is possible later.
- **Shock shoulders upstream.** VFEP's Shock shoulders grant VFEP's trait by themselves, so a
  Medic wearing them loses Doctor (same spec, section 5). Propose moving the lists off the
  shoulders upstream; no local patch.
- **Stop slider in the family.** `Tunable.cs` has diverged from Shipcracker's and Toxin's copies:
  banded rows slide over their stops here and snap within a band there. Port it or let the
  copies part.
