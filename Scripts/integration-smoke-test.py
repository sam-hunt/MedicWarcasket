#!/usr/bin/env python3
# Pre-release integration smoke test: boots the real game once with Medic
# Warcasket, its dependency chain and the optional mods it patches for, on a
# pinned minimal list where the baseline is a clean log, then classifies
# every logged error/warning by origin and fails on anything attributed to
# this mod or an integration seam. Thin shim over the shared engine in
# l10n/smoke/startup_smoke.py (see its header for mechanics and the
# BetterTradersGuild v1.1.0 CWTL incident this exists to catch).
#
# Run this before every release, with the game closed:
#   python3 Scripts/integration-smoke-test.py              # boot + scan
#   python3 Scripts/integration-smoke-test.py --no-launch  # rescan last log
#   python3 Scripts/integration-smoke-test.py --strict     # any error fails

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "smoke"))
import startup_smoke as engine  # noqa: E402

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

engine.PACKAGE_ID = "shunter.medicwarcasket"

# RATIONALE: the first five entries are the l10n CANONICAL_ACTIVE_MODS (the
# refresh shim explains them: VFE Pirates is the hard dep, VEF and Harmony
# are its deps, Vanilla Gravship Expanded requires Odyssey). Save Our Ship 2
# is one of two optional mods this repo integrates with:
# 1.6/Patches/SOS2Patch.xml fires on its display name (or Universum's) and
# never runs otherwise, and a failed PatchOperation is exactly the kind of
# error only a boot with the mod active can surface. SOS2 hard-requires
# Vehicle Framework, which is here only for that reason. Universum is not
# installed locally, and the patch is the same either way. Vanilla Gravship
# Expanded (chapter 1) is the other: LoadFolders.xml opens the
# 1.6/Mods/VanillaGravshipExpanded root on its package id, and that root's
# vacuum-resistance patch only ever runs with it active. It sits before our
# mod, as the mod a compat root complements should. Probe last (auto-quit).
# Plain ids on purpose; see the refresh shim's "_steam" note.
engine.SMOKE_ACTIVE_MODS = [
    "brrainz.harmony",
    "ludeon.rimworld",
    "ludeon.rimworld.odyssey",
    "oskarpotocki.vanillafactionsexpanded.core",
    "oskarpotocki.vfe.pirates",
    "vanillaexpanded.gravship",
    "smashphil.vehicleframework",
    "kentington.saveourship2",
    "shunter.medicwarcasket",
    "shunter.l10nprobe",
]

# Assembly/namespace name, the C# log prefix (also the About.xml display
# name, which the engine derives and appends itself), and the def prefix.
engine.OWN_PATTERNS = ["MedicWarcasket", "[Medic Warcasket]", "MDWC_"]

# The seams: VFEP's warcasket code (foundry, entombing, Apparel_Warcasket)
# walks our WarcasketDefs, VEF's ApparelExtension and CompShieldBubble run
# on them, SOS2 reads the EVA tag our patch adds, and VGE1's vacuum rules
# read the resistance our patch lowers. An error mentioning any of these
# gates the test even when the exception fires inside their code. Vehicle
# Framework is deliberately absent: it is only SOS2's dep and its own noise
# is third-party.
engine.INTEGRATION_PATTERNS = {
    "VFEP": ["VFEPirates"],
    "VEF": ["VEF."],
    "SOS2": ["SaveOurShip2", "SaveOurShip"],
    "VGE": ["VanillaGravshipExpanded"],
}

raise SystemExit(engine.main())
