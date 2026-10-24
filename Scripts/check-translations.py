#!/usr/bin/env python3
# Medic Warcasket's config shim over the shared translation checker
# (l10n/checker/check_translations.py, the rimworld-l10n submodule). The
# engine holds all logic; this file holds only this repo's config and the
# rationale behind it. Usage is unchanged:
#   python3 Scripts/check-translations.py [--strict] [--root PATH]
# If l10n/ is empty, run: git submodule update --init

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "checker"))
import check_translations as engine  # noqa: E402  (import after sys.path edit)

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

# No [TranslationCanChangeCount]-style matching-token fields in this repo:
# the def text fields and the Keyed strings carry no such tokens.
engine.PARITY_EXEMPT_FIELDS = set()

# RATIONALE: no DLC is a hard dependency (About.xml's modDependencies are
# Harmony and VFE Pirates). The defs carry MayRequire on two stat LEAVES
# (VacuumResistance for Odyssey, SlaveSuppressionOffset for Ideology); a
# gated leaf drops a number, not a def, so neither DLC changes the key set
# the probe dumps. Odyssey is required anyway: Vanilla Gravship Expanded
# requires it, and if an Odyssey-only def or DefInjected ever lands, a
# sidecar generated without Odyssey would silently lose its keys. Ideology
# stays out: nothing here will ever gate translatable content on it.
engine.REQUIRED_DLCS = {"Odyssey"}

# The three warcasket parts are VFEPirates.WarcasketDefs, a ThingDef
# subclass with no database of its own (it adds shortDescription and three
# role flags). The game rolls it into DefDatabase<ThingDef>, so the probe's
# walker dumps these defs under ThingDef and DefInjected translations
# legally target 1.6/Languages/<Language>/DefInjected/ThingDef/. The checker
# needs the XML element tag mapped to that folder to match Defs/ against the
# sidecar.
# The mechanite dressing's VEF.Abilities.AbilityDef needs no alias: it is its
# own def database, and its element tag is already its folder name.
engine.DEF_TYPE_ALIASES = {
    "VFEPirates.WarcasketDef": "ThingDef",
}

# The mod settings window and the mechanite dressing's runtime strings are a
# real Keyed surface, so a missing Languages/ tree is a hard config error,
# not a legal state.
engine.ALLOW_NO_KEYED_SURFACE = False

# The localized Steam Workshop title lives in this Keyed key (the
# settings-window header); the checker enforces the title-coupling rule
# against each .steamworkshop/Description/<Language>.txt title line.
engine.WORKSHOP_TITLE_KEY = "MDWC_SettingsCategory"

raise SystemExit(engine.main())
