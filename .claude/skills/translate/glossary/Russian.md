# Russian glossary, Medic Warcasket

First machine-assisted pass 2026-11-05. Only mod-specific coinages and
grounding decisions. The VFEP warcasket vocabulary (warcasket, the parts, the
label pattern, foundry, entomb, VFEP's set names and research labels, the
spacer-tier opener) lives upstream in `l10n/languages/Russian.md` under
"Grounded common vocabulary, VFE Pirates warcasket domain", with the
grounding source (the community pack OneCodeUnit/VanillaRussianExpanded) and
what is pending native review; the settings and ability words the family
settled (spacer-tech, Off/default, When enabled, cooldown, range, the VEF
`jobReportString` frame and the `cooldownGerund`) sit in that file's main
table. Medic's descriptions carry their own paragraphs 2 and 3; only the
10th-generation opener (`Специализированная броня мертвеца со стандартными
защитными характеристиками, сделанная по технологиям космического уровня.`
and its shoulders and helmet forms) is the family's. Nothing here restates
the upstream rows; a VFEP term is changed there first.

## Set-specific rows

| English | Russian | Grounding |
|---|---|---|
| medic warcasket / shoulders / helmet | медицинская броня мертвеца / наплечники медицинской брони / шлем медицинской брони | Upstream label pattern with the set adjective `медицинская`; a functional adjective like the pack's `прорывная` / `охранная`. |
| Medic (the series, in prose) | медицинская серия | Toxin's `Токсичная серия` frame. |
| Workshop title / `MDWC_SettingsCategory` | Медицинская броня мертвеца | Contains `броня мертвеца`; the two are coupled. |
| warcasket (the trait label) | броня мертвеца | Pack `VFEP_WarcasketTrait.degreeDatas.0.label`, fetched 2026-11-05. The description opens with the pack's `{PAWN_nameDef} находится внутри брони мертвеца.` |
| the warcasket trait (cited in settings) | черта «броня мертвеца» | Core Keyed `TraitLabelWithDesc` `черта «{TRAITLABEL}»`. |
| mechanite dressing (ability, section header) | механитная перевязка | Coined. `механиты` per Core `FibrousMechanites` / `SensoryMechanites`; `перевязка` per Core `labelTendedWell` `перевязано` on every injury hediff. |
| dress (verb) | перевязывать / делать перевязку | Same grounding. |
| mechanite dressing suite (the armor's reloadable, in prose) | система механитной перевязки | Coined. |
| carrier mechanites | механиты-переносчики | Coined. |
| adrenaline / adrenaline shot (ability, hediff `labelNoun`, section) | адреналин / укол адреналина | Standard medical term; no vanilla hit for either. |
| adrenaline injector | инъектор адреналина | Coined (medical `инъектор`). |
| field-medicine manipulators | медицинские манипуляторы | Coined; `манипуляторы` as in Shipcracker's `пробивные манипуляторы`; the register read dropped the genitive chain `манипуляторы полевой медицины`. |
| diagnostic visor / medical assistance module | диагностический визор / модуль медицинской помощи | Coined. |
| adrenaline stages: rush / steady / wearing off / crash | прилив / плато / спад / упадок сил | Coined; nouns, as Core's `ломка`, `головокружение`, `выздоровление` stage labels. |
| injury severity | тяжесть ранений, `{0} ед. тяжести ранений` | Coined; `тяжесть` as Core `AddictionSeverityInitial`, `ед.` per the family quantity row. |
| raider and allied medics / downed | медики налётчиков и союзников / выведенные из строя | Core `налётчик`, `медик` (`SpaceMarineMedic10`), `выведен из строя` (`Doctor.description`, `Wimp`). |
| autocast | автоприменение | VEF pack Keyed `VFEA.RClickToAuto` `Правый клик включит автоприменение.` (the tooltip line `MDWC_ShotAutocastDesc` follows). |
| hostile target | Цель враждебна: {0}. | Coined to avoid a gendered predicate on a name; VEF pack `VFEA.TargetMustBeHostile` `Цель должна быть враждебной`. |
| suppressability (CE, Workshop) | чувствительность к подавлению | CE's own ru `Suppressability.label`. |
| work types (settings) | Разрешённые работы, `Пилоту доступны работы: {0}.` | `{0}` is the work types' `labelShort` list (`врач`), so it sits after a colon. |
| Shot stops a {0} (heart attack setting) | `Укол останавливает {0}`, tooltip `укол адреналина остановит {1} и не даст обычного прилива сил` | `{0}`/`{1}` is Core `HeartAttack.label` `сердечный приступ` (m. inanimate), whose accusative equals the nominative, so the plain slot reads correctly with no `lookup`. "The boost" is `прилив сил`, after the `прилив` rush stage. |

## Stranger in black

| English | Russian | Grounding |
|---|---|---|
| man in black (section header) / the stranger (in prose) | Незнакомец в чёрном / незнакомец | Core `StrangerInBlackJoin.label` / `.letterLabel`; the Workshop page's `[h2]Незнакомец в чёрном[/h2]`. Settings prose keeps the generic masculine even with the gender unfixed. |
| `MDWC_StrangerInBlack` label / labelFemale / labelMale / labelPlural | незнакомец в чёрном / незнакомка в чёрном / незнакомец в чёрном / незнакомцы в чёрном | Core `StrangerInBlack` PawnKindDef, verbatim (the def is a copy of it). |
| arrive (the stranger) | приходит / придёт | Core `StrangerInBlackJoin.letterText` `приходит`. |
| close to falling / duster / revolver | на грани гибели / плащ / револьвер | Coined phrase; Core `Apparel_Duster.label`, `Gun_Revolver.label`. |
| warcasket slugthrower (VFEP gun) | пулевик мертвеца | Community VFEP pack (OneCodeUnit/VanillaRussianExpanded). |
| warcasket kit (the set, in the summaries) | комплект медицинской брони, `в чёрном комплекте медицинской брони` | `комплект` as `MDWC_SettingsUnavailable` and the Workshop's `в этом комплекте`; the set's shoulders/helmet pattern drops `мертвеца`. Never `аптечка`, the charge noun. |
| once warcaskets are researched | после исследования брони мертвеца, `только когда колония её исследует` | VFEP_Warcaskets cited uncapitalised, as the English leaves it uncited. |
| forced gender / no forced gender | заранее заданный пол / `Пол незнакомца не задан заранее` | Coined. |
| arrival letter | письмо о прибытии | Core letters are `письмо`. |
| the letter with the gender unfixed (`MDWC_StrangerLetterLabel`, `MDWC_StrangerLetterText`) | `{PAWN_gender ? Незнакомец : Незнакомка} в чёрном` / `Чувствуя беду, приходит {PAWN_gender ? таинственный незнакомец : таинственная незнакомка} в чёрном.\n\nСможет ли {PAWN_gender ? он : она} навести порядок в этих краях?` | Core `StrangerInBlackJoin.letterLabel` / `.letterText` with the gender resolved per pawn (Russian has no neutral `незнакомец`). Vanilla's `{PAWN_kind}` and `{PAWN_pronoun}` are replaced by `{PAWN_gender ? ...}` constructs because the checker rejects argument tokens absent from the English; the decompiled `IncidentWorker_WandererJoin` formats both fields with the pawn as `PAWN`. So `MDWC_StrangerAnyGenderDesc` says the letter is written for the stranger's gender (`с учётом пола`), not "neutral wording". |
| Another mod chooses the stranger | `Незнакомца выбирает другой мод, поэтому настройки брони мертвеца не действуют.` | Shaped like `MDWC_SettingsUnavailable`. |

## Ported from the siblings

| English | Russian | From |
|---|---|---|
| These settings are unavailable ... | Эти настройки недоступны. Другой мод изменил медицинскую броню мертвеца, поэтому у комплекта останутся исходные значения. | Shipcracker `SCWC_SettingsUnavailable`, adjective swapped. |
| (default) / Default: {0}. / When enabled / When disabled | ` (по умолчанию)` / `По умолчанию: {0}.` / `Если включено, ...` / `Если отключено, ...` | Both siblings. |
| How long ... / The time the pilot must wait ... (settings tooltips) | Сколько ... / Сколько пилоту ждать после ..., прежде чем ... | Shipcracker `SCWC_CooldownDesc`, Toxin's `Сколько ...` openers. |
| Workshop headers, safe to add / remove, required, SOS2 and VGE bullets, CE armor clause, How do I get it, links | `[h1]О моде[/h1]`, `[h1]Совместимость[/h1]`, `[h1]Вопросы и ответы[/h1]`, `[h1]Ссылки[/h1]`, the two save bullets, `... обязателен.`, the SOS2 and VGE bullets, `показатели брони, объёмность и переносимый объём как у другой брони мертвеца космического уровня`, the FAQ answer, the GitHub and art lines | Toxin and Shipcracker Workshop pages. |
| Workshop intro | Новая броня мертвеца космического уровня для [url=...]Vanilla Factions Expanded - Pirates[/url]. | Shipcracker. |
| research cited in prose | «Космическая броня мертвеца» | Upstream row, as both siblings' pages. |

## Charge nouns

| English | Russian | Forms | Lookup files |
|---|---|---|---|
| kit (the armor's `CompDressingSuite.chargeNoun`) | аптечка | pl. аптечки; gen. pl. аптечек | `1.6/Languages/Russian/WordInfo/plural.txt` (`аптечка;аптечки`) and `case.txt` rows for both numbers |
| shot (the shoulders' `CompApparelReloadable.chargeNoun`) | укол | pl. уколы; gen. pl. уколов | same two files (`укол;уколы`, two `case.txt` rows) |

Read aloud in Core's five slots: `Осталось аптечек`, `аптечки` (max charges
stat name), `Стоимость 1 аптечки`, `аптечки закончились`, `как только
закончатся уколы`. No Gender file: Russian's Keyed never switches on
`CHARGENOUN_gender`. The mod's own strings agree: `Аптечки: {0} / {1}`,
`Уколы: {0} / {1}`, and the counts in the summaries use
`{0_numCase ? аптечки : аптечек : аптечек}` after `до` and
`{0_numCase ? укол : укола : уколов}`. Medicine and reload-item labels go
through `{lookup: {N}; Case; 1}` (genitive) or `Case; 4` (instrumental);
Core's `Case.txt` lists all three medicines, Go-сок and бодрин, and a modded
label degrades to the nominative.

## Grounded in the first pass

| English | Russian | Source |
|---|---|---|
| herbal / industrial / glitterworld medicine | лекарственные травы / медикаменты / передовые медикаменты | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` `.label` |
| go-juice / wake-up | Go-сок / бодрин | Core `GoJuice.label`, `WakeUp.label` |
| heart attack / infection / disease | сердечный приступ / заражение / болезнь | Core `HeartAttack.label`, `WoundInfection.label`, Keyed `LetterLabelTraitDisease` |
| mechanites | механиты | Core `FibrousMechanites.label` `волоконные механиты` |
| tend / tend quality / tend speed | лечить / качество лечения / скорость лечения | Core `TendPatient.reportString` `лечит TargetA`, Keyed `TendQuality`, `MedicalTendQuality.label`, `MedicalTendSpeed.label` |
| has no wounds to tend / does not need tending | У {0} нет ран, требующих лечения. / {0} не требует лечения. | Core Keyed `AbilityCannotCastNoHealableInjury`, `TendingNotRequired` |
| doctor (work type) / Work tab / caring | врач, работать врачом / вкладка «Работа» / уход | Core `Doctor.labelShort`, `Work.label`, Keyed `WorkTagCaring` |
| manipulation / sight / pain shock | «Работа» / зрение / болевой шок | Core `Manipulation.label` `работа`, `Sight.label`, Keyed `PainShock` |
| shield belt / plasteel / spacer | пояс-щит / пласталь / космический | Core `Apparel_ShieldBelt.label`, `Plasteel.label`, Keyed `TechLevel_Spacer` |
| Mote | частица | Core `Mote_*` labels |
| casting time / cooldown / range / line of sight | Время для подготовки / Перезарядка / Дальность / Требуется прямая видимость | Core Keyed `AbilityCastingTime`, `CooldownTime`, `Range`, `AbilityRequiresLOS` |
| Blue / Green / Pink | Синий / Зелёный / Розовый | Core Keyed `Blue`, `Green` (lowercase there, capitalised as option labels); Core has no `Pink` |
| missing body part | отсутствующая часть тела | Core Keyed `MissingBodyPart` `Отсутствует` |
| vacuum | вакуум | Odyssey `VacuumResistance.label` `устойчивость к вакууму` |
| trait pawn tokens | `{PAWN_objective}` is the dative (`ему`), `{PAWN_pronoun}` the nominative | Core `Bloodlust`, `SlowLearner` descriptions |
| report strings | `делает механитную перевязку TargetA`, `делает укол адреналина TargetA`; the abilities' `jobReportString` the same without TargetA | Core `JobDef` reportStrings: third person present, lowercase, no period |

## Pending native review

- Every coinage in "Set-specific rows": the set adjective `медицинская`,
  `механитная перевязка`, `система механитной перевязки`,
  `механиты-переносчики`, `инъектор адреналина`, `медицинские
  манипуляторы`, `диагностический визор`, `модуль медицинской помощи`, the four
  stage labels (`плато` especially), `тяжесть ранений`, `Цель враждебна:
  {0}.`
- The charge nouns `аптечка` and `укол` in the reload slots.
- Three family frames kept as shipped until a native review rules: `объёмность`
  for Combat Extended's bulk (both siblings' Workshop pages), `механиты-переносчики`
  for "carrier mechanites", and Core's own `Время для подготовки` as the casting
  time label.
- All free prose: the three descriptions' first paragraphs and the shared
  paragraphs 2 and 3, both ability descriptions, the hediff and trait
  descriptions, every settings tooltip and summary, and the whole Workshop
  page.
- The stranger section: `на грани гибели`, `заранее заданный пол`, `комплект
  медицинской брони` in the summaries, and the gender-resolved arrival letter
  (label and text); an in-game arrival with the gender unfixed confirms it.
