# Russian glossary, Medic Warcasket

Seeded before any translation pass ran here; no pass has run yet. Only
mod-specific coinages and grounding decisions. The VFEP warcasket vocabulary
(warcasket, the parts, the label pattern, foundry, entomb, VFEP's set names
and research labels, the spacer-tier opener and the 10th-generation
paragraph) lives upstream in `l10n/languages/Russian.md` under "Grounded
common vocabulary, VFE Pirates warcasket domain", with the grounding source
(the community pack OneCodeUnit/VanillaRussianExpanded) and what is pending
native review; the settings and ability words the family settled
(spacer-tech, Imperial, plasteel/uranium/chemfuel, Off/default, cooldown,
range, Odyssey, the fuel-quantity phrases, the VEF `jobReportString` and the
`cooldownGerund`) sit in that file's main table. Nothing here restates them;
a VFEP term is changed there first.

## Set-specific rows

| English | Russian | Grounding |
|---|---|---|
| Workshop title | `<Set adj.>` броня мертвеца, sentence case | Contains `броня мертвеца`. Also the `MDWC_SettingsCategory` value (title coupling). |

## Charge nouns

The armor's medicine suite has the `chargeNoun` "kit" (its row is *pass*,
below the set-specific rows); render it with the language's natural word for a medicine kit. It is the
vanilla reload "charge noun", which vanilla prints as "Kits" and "Reload cost per kit". A charge noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and the Russian worker reads the `plural` table and the genitive slots go through `case` lookups, so every coined noun ships a `plural.txt` line and `case.txt` rows for both numbers under `1.6/Languages/Russian/WordInfo/`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## To ground in the first pass

- the trait label `warcasket` (Bio tab): VFEP's own trait label, so render it
  exactly as the community pack renders VFEP's, else the bare upstream
  warcasket noun; the description reuses the pilot and pain vocabulary
  settled for the armor.
- siegebreaker (the chassis, lore paragraph 3): the upstream set-name row.
- the 10th-generation paragraph: the pack's Siegebreaker / Brute / Guardian / Sarcophagus / Controller descriptions share it verbatim; take that rendering.
- medic (this set's name) and the Workshop title.
- pilot (the wearer, every description), where no upstream row covers it.
- Core medical vocabulary once the abilities exist: tend, medicine, surgery, bleeding, infection, pain, and whatever hediffs or stats the final text names.
- mechanite dressing (the ability): keep "mechanite" as vanilla renders "mechanites" in this language (match vanilla's rendering; ground it in the first pass), with "dressing" in the tending sense and the verb "dress" in its strings.
- This mod's coinages, once the descriptions are final.

## Pending native review

The VFEP terms' review is tracked upstream, under the section named above.
Add this mod's own coinages here as the pass makes them.
