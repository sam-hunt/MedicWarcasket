# Spanish (Castellano) glossary, Medic Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); no translation pass has run here yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/Spanish.md`. Folder `Spanish`, grounded against
the Castilian tars only (never `SpanishLatin`, whose VFEP lineage uses
"sarcotraje").

VFEP grounding: the community Castilian-leaning VFE Pirates translation by
Cito2310 (GitHub `Cito2310/translate-project-rimworld`). Its label pattern is
`ataúd de guerra <set>` for the torso, `hombreras <set> de ataúd de guerra`
and `casco <set> de ataúd de guerra` for the other parts; this mod follows
it, and takes its Siegebreaker rendering and its 10th-generation paragraph.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Spanish | Grounding |
|---|---|---|
| warcasket | ataúd de guerra (pl. ataúdes de guerra) | Community VFEP (`VFEP_Warcasket_Warcasket.label`). Also the Workshop title anchor. |
| `<set>` warcasket / shoulders / helmet (label pattern) | ataúd de guerra `<set>` / hombreras `<set>` de ataúd de guerra / casco `<set>` de ataúd de guerra | Community VFEP label pattern (`ataúd de guerra bruto`, `hombreras bruto de ataúd de guerra`, `casco bruto de ataúd de guerra`). |
| warcasket shell / pauldrons (shoulders) / helmet | armazón / hombreras / casco | Community VFEP (`Un armazón de ataúd de guerra...`, `VFEP.Shoulderpads`, `VFEP.Helmet`). |
| warcasket foundry | forja de ataúd de guerra | Community VFEP `VFEP_WarcasketFoundry.label`. |
| entomb / welded into | sepultar / sepultado en | Community VFEP `VFEPirates.EntombingWarning` ("Sepultar colonos en un ataúd de guerra"). |
| specialized spacer-tech | especializado de tecnología espacial | Community VFEP 10th-gen descriptions. |
| VFEP set names in prose | Siegebreaker, Bruto, Guardián; "series" for set | Community VFEP (`La serie Bruto`); it leaves Siegebreaker in English and is inconsistent on `guardian`/`guardián`, so the accented form is used. |
| spacer (tech level) | era espacial | Core `TechLevel_Spacer`. |
| Imperial | imperial (lowercase adjective) | Royalty (`batallón imperial`). |
| sealed | hermético | Odyssey (`paredes y puertas herméticas`). |
| respirator | respirador (integrado / propio) | No vanilla Castilian item; plain word. |
| wearer | portador | Core (`Apparel_ArmorMarineHelmetPrestige.description`). |
| plasteel / uranium / chemfuel | plastiacero / uranio / biocombustible | Core labels. |
| jobReportString (`Using ability: {0}`) | usando habilidad: {0}. | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | en enfriamiento | Shipcracker's rendering of the same vanilla comp field. |
| settings: tank / cells / cooldown | depósito / casillas / enfriamiento | `depósito` is plain Castilian for a fuel tank (no vanilla item). Core Keyed `{0} casillas`, `CooldownTime` = `Enfriamiento`. |
| settings: Off / default / Default: on | Desactivado / (por defecto) / Por defecto: activado | Core `Off` = `Apagado` is a power state; `Desactivado` mirrors `Enabled` = `Activado`. Core `default` = `por defecto`. |
| settings: Odyssey | Odyssey | Core `ExpansionDef` keeps DLC names in English. |
| Workshop title | Ataúd de guerra `<set>`, sentence case | Contains the VFEP term. Also the `MDWC_SettingsCategory` value (title coupling). |

## Charge nouns

The armor's medicine suite has the `chargeNoun` "dose" (its row is *pass*,
below the inherited rows). A charge noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and the Spanish worker's rule pluralizes only the last word, so a multi-word noun ships a `singular;plural` line in `1.6/Languages/Spanish/WordInfo/plural.txt`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## To ground in the first pass

- the trait label `medic warcasket` (Bio tab, beside VFEP's `warcasket`
  trait): mirror the community pack's rendering of VFEP's trait label where
  one exists, else the inherited warcasket noun under the label pattern; the
  description reuses the pilot and pain vocabulary settled for the armor.
- siegebreaker (the chassis, lore paragraph 3): the inherited set-name row above.
- the 10th-generation paragraph: the community pack's Siegebreaker / Brute / Guardian / Sarcophagus / Controller descriptions share it verbatim; take that rendering.
- medic (this set's name) and the Workshop title.
- pilot (the wearer, every description), where no inherited row covers it.
- Core medical vocabulary once the abilities exist: tend, medicine, surgery, bleeding, infection, pain, and whatever hediffs or stats the final text names.
- This mod's coinages, once the descriptions are final.

## Pending native review

Nothing inherited is open for the VFEP rows (they are the community pack's).
Add this mod's own coinages here as the pass makes them.
