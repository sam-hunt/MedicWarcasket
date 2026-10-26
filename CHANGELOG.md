# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- Entries are short one-liners ready to paste as Steam Workshop change notes: what changed for the player, no rationale, no def names, no sub-bullets. -->

<!-- Convention: unreleased changes accumulate under the [Unreleased] heading at the top,
     directly below this intro, so a merged contribution is recorded where the release
     flow finds it. Each release renames that heading to its version and date and adds a
     link reference at the bottom of the file. The release workflow lifts the tagged
     version's section into the GitHub release body verbatim and FAILS a stable release
     if the section is missing, so write the section before tagging. Release candidates
     (X.Y.Z-rc.N) get no section: their changes land in the stable version's section at
     promotion. The /release skill walks through all of this. -->

## [Unreleased]

### Added

- The shoulders grant an adrenaline shot, two loaded from a medicine kit each: one gives a person, animal or the pilot go-juice's surge for three hours, fading in three levels while their rest falls faster
- A shot into a heart attack stops it outright instead of stimulating, on anyone, raiders included
- With the armor worn, every patient gets a shot as their dressing ends; right-click the shot to turn that off
- Mod settings for the shots held and their reload item, how long a shot lasts, its rest cost and its cooldown
- Mod settings now choose which caring work the pilot may take on, doctoring and childcare included, with VFE Pirates' warcasket rules as the default for everything else

### Changed

- The field dressing is now the mechanite dressing: it warms up, tends one wound at a time with a beam, then winds down, and the patient is held still until it ends
- The dressing's beam, glow, overlay and gauge are medical blue, with green and pink as mod settings options
- Each medicine kit is charged by the severity of the wounds tended, and the pilot earns medicine experience for every wound, as with a vanilla treatment
- The medicine is chosen in the mod settings (glitterworld by default, no herbal), replacing the right-click medicine menu, and the armor shows a tank gauge whose handle sets how full colonists keep the suite
- Dressings can be queued, and Shift-click chains several patients in a row
- Each wound shows its own tend quality as the beam tends it, instead of one summary at the end
- New defaults: 1 second warmup, 1 second per wound, no cooldown, range 2 cells, a ten-kit capacity
- Internal names changed from field dressing to mechanite dressing, which breaks saves made with unreleased builds only
