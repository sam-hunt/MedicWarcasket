# Scripts/: localization and release tooling

Loaded when work touches the tooling. Each shim's comments carry this repo's config and its
rationale (the VFEP dependency chain, why Odyssey is the only DLC pinned, why SOS2 and VGE1 are
on the smoke list); the engines live in the `l10n/` submodule. Nothing here ships.

- **`l10n/` is the `rimworld-l10n` toolkit as a git submodule pinned to a release tag**
  (`git submodule status`; if empty, `git submodule update --init`). Never edit it in place here:
  mod-independent learnings go upstream in the canonical `~/dev/rimworld-l10n` checkout,
  mod-specific ones in the `/translate` skill's glossary. The pin moves only at release, at the
  start of a translation pass, or when a new major lands (`l10n/tools/bump-consumer.sh`).
- **`check-translations.py`** validates every language deterministically and is a CI release
  gate. Its expected set is the checked-in `expected-injections.json`, a dump of every injection
  point the live game sees for this mod. The checker refuses stale expectations, so new content
  (a new def type adds a DefInjected folder) forces a regen.
- **`refresh-translation-expectations.py`** (game closed) regenerates that sidecar by driving the
  L10nProbe dev mod through the game's own walker (source at `l10n/probe/`, built and deployed
  only from the canonical checkout; this mod is ticked in its settings). Regenerating is always
  fine; the release skill does it every release.
- **`integration-smoke-test.py`** (game closed, pre-release) boots the deployed mod on a pinned
  list (the VFEP chain, Odyssey, Save Our Ship 2 with Vehicle Framework so `SOS2Patch.xml` runs,
  Vanilla Gravship Expanded so its compat root opens), then fails on any logged error attributed
  to this mod or a VFEP/VEF/SOS2/VGE seam. Sibling-mod "dump WILL fail" warnings at launch are
  expected: the probe is ticked for every family mod but each boot loads only its own list.
- **Translation passes are deferred until shortly before release**, one language at a time on
  explicit request, through the `/translate` skill, once the English text is final. No language
  exists yet; the public roster is in `CONTRIBUTING.md`.
