# Korean glossary, Medic Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); no translation pass has run here yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics, josa
markers and style live in `l10n/languages/Korean.md`.

Community VFEP translation grounded against: the RimWorldKorea `RMK` pack
(GitHub RimWorldKorea/RMK), VFE Pirates' ThingDef, WarcasketDef and Keyed
files. Its warcasket set labels follow `<bare set prefix> 워캐스킷 [어깨|헬멧]`
and every set description opens with `우주시대 기술로 특화된 워캐스킷 ...입니다.`;
this mod copies both patterns, and takes the pack's Siegebreaker
prefix and its 10th-generation paragraph.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Korean | Grounding |
|---|---|---|
| warcasket | 워캐스킷 | RMK VFEP, every warcasket label. No separate word for "shell": RMK drops it (`...워캐스킷입니다.`), and so does this mod. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<prefix>` 워캐스킷 / `<prefix>` 워캐스킷 어깨 / `<prefix>` 워캐스킷 헬멧 | RMK `VFEP_Warcasket*_*.label`; "pauldrons" in the description is rendered the same way (`워캐스킷 어깨입니다.`). |
| warcasket foundry | 워캐스킷 주조대 | RMK `VFEP_WarcasketFoundry.label`; bare "foundries" in lore is `주조대`. |
| entomb / weld on | 안치 (워캐스킷에 안치) | RMK `VFEPirates.EntombInWarcasket`; "welded into" / "weld it on" both use 안치. |
| siegebreaker / brute / guardian (set prefixes) | 돌격 / 야수 / 수호 | RMK set prefixes. |
| Imperial | 제국 (bare prefix) | Royalty backstories: `제국 부대`, `제국군`. |
| spacer-tech / spacer | 우주시대 기술 / 우주시대 | RMK VFEP descriptions (`우주시대 기술로 특화된`); Core `TechLevel_Spacer` = `우주`. |
| chassis | 프레임 | RMK's foundry text `워캐스킷 프레임`. |
| pilot | 조종사 | RMK `VFEP_WarcasketHelmet_Shock` (`조종사의 반응 시간`). |
| onboard respirator | 내장 호흡 장치 | Coined; Core `Breathing.label` = `호흡`. |
| sealed (helmet) | 밀폐형 | `밀폐` is the usual word for an airtight suit. |
| plasteel / uranium / chemfuel | 플라스틸 / 우라늄 / 화학연료 | Core / Odyssey labels. |
| jobReportString (`Using ability: {0}`) | {0} 능력 사용 중 | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | 재사용 대기 중 | Shipcracker's rendering of the same vanilla comp field. |
| cooldown (slider caption) | 재사용 대기 | Core Keyed `StatsReport_Cooldown` = `재사용 대기시간`; `시간` dropped from the caption so it does not repeat the `(시간)` unit. |
| radius | 범위 | Core Keyed `AbilityEffectRadius` = `효과 범위`. |
| Off (slider value at 0) | 없음 | Core Keyed `None` = `없음`; Core `Off` = `끄기` is a toggle verb. A toggle's "on" is `켜짐`. |
| default / (requires Odyssey) | 기본값 / (오디세이 필요) | Core Keyed `Default`; ExpansionDef `Odyssey.label` = `오디세이`; shape after Core `BlockedByTerrain` (`({0_label} 필요)`). |
| fuel tank (settings) | 탱크 / 연료 탱크 | Shipcracker's Workshop description; if a tank ever lands here. |
| Workshop title | `<prefix>` 워캐스킷, the armor label | Contains `워캐스킷`. Also the `MDWC_SettingsCategory` value (title coupling). |

## Charge nouns

The armor's medicine suite has the `chargeNoun` "dose" (its row is *pass*,
below the inherited rows). A charge noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and Korean falls to the base `Pluralize` (lookup, else unchanged), so one noun serves every slot and no lookup file is needed. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## To ground in the first pass

- the trait label `medic warcasket` (Bio tab, beside VFEP's `warcasket`
  trait): mirror the community pack's rendering of VFEP's trait label where
  one exists, else the inherited warcasket noun under the label pattern; the
  description reuses the pilot and pain vocabulary settled for the armor.
- siegebreaker (the chassis, lore paragraph 3): the inherited set-name row above.
- the 10th-generation paragraph: RMK's Siegebreaker / Brute / Guardian / Sarcophagus / Controller descriptions share it verbatim; take that rendering.
- medic (this set's name) and the Workshop title.
- pilot (the wearer, every description), where no inherited row covers it.
- Core medical vocabulary once the abilities exist: tend, medicine, surgery, bleeding, infection, pain, and whatever hediffs or stats the final text names.
- This mod's coinages, once the descriptions are final.

## Pending native review

Nothing inherited is open for the VFEP rows (they are RMK's). Add this mod's
own coinages here as the pass makes them.
