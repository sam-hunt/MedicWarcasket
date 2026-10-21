# Simplified Chinese glossary, Medic Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); no translation pass has run here yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/ChineseSimplified.md`.

VFEP terms are grounded against the community Vanilla Expanded Simplified
Chinese pack (GitHub `q847633684/Vanilla-Expanded-ZH`, the
`Vanilla Factions Expanded - Pirates` folder: its WarcasketDef ThingDef
injections, Keyed UI and ResearchProjectDef files), which also carries the
Siegebreaker set and the 10th-generation paragraph. Vanilla terms
are grounded against the Core, Royalty and Biotech `ChineseSimplified (简体中文).tar`.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Simplified Chinese | Grounding |
|---|---|---|
| warcasket | 战棺 | Community VFEP pack throughout (`VFEP.RequiresWarcasket` = 需要战棺, every set label). Workshop title anchor. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<set>`型战棺装甲 / `<set>`型战棺护肩 / `<set>`型战棺头盔 | Community pattern (`VFEP_Warcasket_Siegebreaker.label` = 破袭型战棺装甲 and siblings). The body piece takes 装甲 although the English label has no noun, exactly as the pack does. |
| siegebreaker / guardian / brute / aerial (set names) | 破袭型 / 圣盾型 / 狂怒型 / 突击型 | Community pack labels. The third-party 无畏 prefix (WarCasket Expanded) is not used; the VFEP pack has none. |
| Specialized spacer-tech warcasket shell/pauldrons/helmet | 特化型太空时代级别战棺装甲/护肩/头盔 | Community pack's opening phrase for every 10th-generation part. |
| warcasket foundry | 战棺铸造厂 | Community `VFEP_WarcasketFoundry.label`. |
| weld on / entomb (Workshop) | 在战棺铸造厂完成改造; 封在战棺里 | Community renders entomb loosely (进入铸造厂, 战棺改造 in `VFEPirates.EntombingWarning`); the Workshop follows that looseness. |
| spacer warcaskets (research) | 「太空时代战棺」 | Community `VFEP_SpacerWarcaskets.label`; corner brackets per vanilla research-name citation (研究「基础逆重科技」). Cite `specialised warcaskets` the same way. |
| pilot | 驾驶员 | Community Keyed/lore (成为驾驶员的殖民者). |
| respirator | 呼吸器 | Coined (no vanilla respirator item or gene in the zh tars); plain everyday word. |
| breathing | 呼吸能力 | Core `Breathing.label`. |
| Imperial | 帝国 | Royalty adjective use (帝国逃兵). |
| plasteel / uranium | 玻璃钢 / 铀 | Core labels. |
| jobReportString (`Using ability: {0}`) | 使用{0}。 | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | 冷却中 | Shipcracker's rendering of the same vanilla comp field. |
| cooldown | 冷却时间 | Core Keyed `CooldownTime`. |
| Off / default / Default: on | 关 / 默认 / 默认：启用 | Core Keyed `Off`, `Default`, `Enabled`. |
| settings suffixes | （需要奥德赛） / （默认） | Full-width parens with no leading space, as vanilla 锻造台（燃料）. |
| Royalty / Odyssey | 皇权 / 奥德赛 | Core `ExpansionDefs`; ground Biotech's name the same way. |
| Workshop title | `<set>`型战棺 | Set name + 战棺 (the anchor term). Also the `MDWC_SettingsCategory` value (title coupling). |

## Charge nouns

The armor's medicine suite has the `chargeNoun` "dose" (its row is *pass*,
below the inherited rows). A charge noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and Chinese falls to the base `Pluralize` (lookup, else unchanged), so one noun serves every slot and no lookup file is needed. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## To ground in the first pass

- siegebreaker (the chassis, lore paragraph 3): the inherited set-name row above.
- the 10th-generation paragraph: the pack's Siegebreaker / Brute / Guardian / Sarcophagus / Controller descriptions share it verbatim; take that rendering.
- medic (this set's name) and the Workshop title.
- pilot (the wearer, every description), where no inherited row covers it.
- Core medical vocabulary once the abilities exist: tend, medicine, surgery, bleeding, infection, pain, and whatever hediffs or stats the final text names.
- This mod's coinages, once the descriptions are final.

## Pending native review

Inherited and still open there: 呼吸器 (coined, no vanilla or community
source). Add this mod's own coinages here as the pass makes them.
