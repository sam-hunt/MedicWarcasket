# Simplified Chinese glossary, Medic Warcasket

First pass: machine-assisted generation, 2026-11-05, no native review yet.
Only mod-specific coinages and grounding decisions. The VFEP warcasket
vocabulary (warcasket, the label pattern, foundry, entomb, VFEP's set names
and research labels, pilot, the spacer-tier opener and its "standard
protection" tail) lives upstream in `l10n/languages/ChineseSimplified.md`
under "Grounded common vocabulary, VFE Pirates warcasket domain", with its
grounding source (the community Vanilla Expanded Simplified Chinese pack,
GitHub `q847633684/Vanilla-Expanded-ZH`, `Vanilla Factions Expanded - Pirates`
folder) and what is pending native review; the settings words the family
settled (Off / On / default, the settings suffixes, cooldown, the
`cooldownGerund`) sit in that file's main table. Only the spacer opener is the
family's here: Medic's descriptions carry their own paragraphs 2 and 3 (the
refit lore and the medic-series paragraph), not VFEP's 10th-generation
paragraph, so those two are this mod's prose and are recorded below. Nothing
here restates an upstream row; a VFEP term is changed there first. Vanilla
terms are grounded against the Core, Royalty, Biotech, Anomaly and Odyssey
`ChineseSimplified (简体中文).tar`.

## Set-specific rows

| English | Simplified Chinese | Grounding |
|---|---|---|
| medic (the set name, `<set>`型) | 医疗型 | Coined on the upstream `<set>`型 pattern (破袭型, 毒素型); 医疗 is Core's `Medicine.skillLabel`. Labels 医疗型战棺装甲 / 医疗型战棺护肩 / 医疗型战棺头盔. |
| Workshop title / `MDWC_SettingsCategory` | 医疗型战棺 | Set name + 战棺 (the anchor term); the two are equal (title coupling). |
| trait label `warcasket` | 战棺 | The community pack's `VFEP_WarcasketTrait.degreeDatas.0.label`, so the Bio tab reads as VFEP's. |
| mechanite(s) | 纳米机械 | Core `FibrousMechanites` / `SensoryMechanites` (肌纤维纳米机械感染). |
| mechanite dressing (ability, section header) / dress | 纳米机械包扎 / 包扎 | Coined. 包扎 is the everyday word for dressing a wound; reportString 包扎TargetA的伤口。 on Core's verb-first shape. |
| mechanite dressing suite (the armor's reloadable) | 纳米机械包扎系统 | Coined; 系统 rather than 套件 so it reads as built in. |
| carrier mechanites | 载药纳米机械 | Coined. |
| adrenaline / adrenaline shot (ability, section header) | 肾上腺素 / 肾上腺素注射 | 肾上腺素 is Core prose (`KingOfPirates25` backstory); the ability noun phrase is coined. Hediff label 肾上腺素, labelNoun 肾上腺素注射. |
| adrenaline injector | 肾上腺素注射器 | 注射器 per Royalty `VenomFangs.description`. |
| the four levels: rush / steady / wearing off / crash | 飙升 / 平稳 / 消退 / 虚脱 | Coined, short and bare as vanilla's stage labels (`HeartAttack` 绞痛 / 虚弱 / 致命; `Anesthetic.stages.wearing_off` 渐渐恢复 is a different sense). 虚脱 also renders "crash" and "crashing" in the settings. |
| diagnostic visor | 诊断面罩 | Coined; 诊断 per Core `MedicineIndustrial.description` (诊断设备). |
| medical assistance module | 医疗辅助模块 | Coined on Shipcracker's shipped 近战辅助模块 (melee assistor modules). |
| field-medicine manipulators | 战地医疗机械臂 | Coined. |
| field surgeons / surgeons | 战地军医 / 军医 | Coined, lore paragraph 2. |
| squadmate | 战友 | Plain word, lore paragraph 3 and the Workshop. |
| raider medic / allied medic | 袭击者医疗兵 / 盟友中的医疗兵 | 袭击者 per Core (`Difficulty_RaidLootPointsFactor_Info`); 医疗兵 coined. |
| reload item / reload target / never reload | 装填物品 / 装填目标 / 从不装填。 | 装填 per Core `Reload` (以{AMMO_label}重新装填{GEAR_labelShort}); "Reloads with {0}." = 以{0}装填。 on the same frame. |
| medicine (generic) / medicine gauge | 药品 / 药品计量表 | 药品 rather than 医药, which is `MedicineIndustrial.label` and would read as one item; 计量表 per Toxin's shipped gauge. |
| injury severity / severity per kit | 伤势严重程度 / 每医疗包治疗量 | 严重程度 per Core `ConfigurableSeverity`; the stat label is coined. |
| effect color / Blue / Green / Pink | 特效颜色 / 蓝 / 绿 / 粉 | 特效 per the mote label; 蓝 / 绿 per Core Keyed `Blue` / `Green`; Core has no `Pink` key, 粉 follows `Structure_Pink` (淡粉) and `Hair_Pink` (粉发) at Keyed's one-character length. |
| time per wound / boost duration / crash duration | 每处伤口用时 / 增益持续时间 / 虚脱持续时间 | Coined; 持续时间 per Toxin. |
| autocast | 自动施放 | Coined; no vanilla zh string names autocast, VEF ships English only. |
| enabled work types | 已启用的工作类型 | Coined; 启用 per Core `Enabled`. |
| the Medic paragraphs 2 and 3 | 这款第十代战棺出自一项政府项目…… / 装甲内置纳米机械包扎系统，可就地治疗战友的伤口…… | Medic's own prose, byte-identical across the three descriptions; the full text is `1.6/Languages/ChineseSimplified/DefInjected/ThingDef/Apparel_Medic.xml`. |
| Starcraft (Workshop FAQ) | 星际争霸 | The game's official mainland title. |
| shot stops a heart attack (setting label) | 针剂可止住{0} | Coined on the shipped `MDWC_ShotStopsHeartAttack` (可止住心脏病发作); `{0}` is Core `HeartAttack.label` at runtime. |
| warcasket kit (the stranger summaries) | 一整套黑色战棺 | Coined. 一整套 names the whole set (armor, shoulders, helmet) and keeps it apart from 医疗包, the dressing's charge noun; 套 per the shipped 这套装备 / 这套战棺. |
| once warcaskets are researched | 完成战棺研究后 | Coined; the gate is VFEP's base `VFEP_Warcaskets` project, cited generically, not by its label. |
| no forced gender | 性别不固定 | Coined. Vanilla forces the stranger male only through the incident, so the Chinese names no gender either. |
| stranger in black letter (unfixed gender) | 黑衣人 / 仿佛嗅到危机的气息，一位神秘的黑衣人出现了。\n\n那么此人能不能解决这一切呢？ | Vanilla `StrangerInBlackJoin.letterLabel` verbatim; letterText follows vanilla's sentences with the kind label written out and 此人 for vanilla's `[PAWN_pronoun]`, so the letter is genderless as `MDWC_StrangerAnyGenderDesc` says and needs no `PAWN` token. |
| another mod chooses the stranger | 另一个模组决定了黑衣人的人选 | Coined, naming the outcome rather than the replaced incident worker. |

## Ported from the siblings

| English | Simplified Chinese | Source |
|---|---|---|
| Default: {0}. | 默认：{0}。 | Toxin and Shipcracker Keyed, every tooltip; also `MDWC_WorkDefault`. |
| When enabled, … When disabled, … | 启用后，… 禁用后，… | Shipcracker `SCWC_VacuumBurnDesc`. |
| enabled / disabled (the state words) | 启用 / 禁用 | Shipcracker; Core `Enabled` / `Disabled`. |
| These settings are unavailable… built-in values. | 这些设置不可用。另一个模组修改了医疗型战棺，因此这套装备将沿用内置数值。 | Shipcracker `SCWC_SettingsUnavailable`; "definitions" dropped as modder vocabulary. |
| Workshop frames | 为[url=…]…[/url]新增一套太空时代战棺。 / 简介 / 模组兼容性 / 常见问题 / 链接 / `[b]躯干：[/b]` / 可安全添加至现有存档。 / 仍有殖民者封在这套战棺里时，不可安全移除。 / …：必需。 / CE 护甲值、体积与体积携带能力和…相当 / SOS2 line / 各部件可以与VFE海盗自带的套装随意混搭。 / 问：怎么获得？ 答：研究VFE海盗的「太空时代战棺」项目，然后像其他套装一样在战棺铸造厂完成改造。 / 问：这会不会不平衡？ / Mod 设置 / links line | Shipcracker and Toxin Workshop pages (Toxin's links line). |
| VFE Pirates (in prose) | VFE海盗 | Both sibling Workshop pages; also `MDWC_WorkSummary`. |
| keeps X at full | 让…始终保持满额 | Shipcracker Workshop (呼吸能力). |

## Charge nouns

| `chargeNoun` | Simplified Chinese | Forms and lookup |
|---|---|---|
| kit (the armor's `CompDressingSuite`) | 医疗包 | One form. Chinese falls to the base `Pluralize` (lookup, else unchanged), and the Chinese Keyed never switches on `CHARGENOUN_gender`, so no `WordInfo` file is shipped. Core `MedicineIndustrial.description` calls a medicine 医疗包. Read aloud in every slot: 医疗包剩余, 这个物体最多能保持的医疗包数量, 医疗包装填消耗, 每医疗包装填消耗, 每装填一医疗包需要消耗物品的数量, 最后一个医疗包使用完后消失, 没有医疗包。, 剩余医疗包 ({0}). The mod's own strings count it with 个 (装甲最多可容纳{0}个医疗包). |
| shot (the shoulders' `CompApparelReloadable`) | 针剂 | One form, no lookup file, as above. 针剂 per Anomaly's serum descriptions (注射针剂). Reads in every slot like 医疗包; vanilla's own counted nouns (烟罐, 燃料罐, 榴弹) sit in the same slots. The mod's Keyed counts it with 支 (护肩可容纳{0}支针剂) and calls one dose 一针. |

## Grounded in the first pass

| English | Simplified Chinese | Source |
|---|---|---|
| herbal medicine / medicine / glitterworld medicine | 草药 / 医药 / 闪耀世界医药 | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` labels (arrive at runtime; named in no string). |
| go-juice / wake-up | 活力水 / 清醒丸 | Core `GoJuice.label`, `WakeUp.label`. |
| heart attack | 心脏病发作 | Core `HeartAttack.label`. |
| infection / disease | 感染 / 疾病 | Core `WoundInfection.label`; 疾病 per Core `Medicine.description` (治疗伤口和疾病). |
| missing body part | 缺失的身体部件 | Core `MissingBodyPart.labelNoun` (缺失身体部件). |
| tend (the action) / tend quality / tend speed | 治疗 / 治疗质量 / 治疗速度 | Core `TendPatient.reportString` (治疗TargetA。), `MedicalTendQuality.label`, `MedicalTendSpeed.label`, Keyed `TendQuality`. |
| has nothing to tend (invalid target) | {0}不需要照料。 | Core Keyed `TendingNotRequired` (不需要照料), `Tend` (照料{0}). |
| medical skill | 医疗技能 | Core `Medicine.skillLabel` (医疗). |
| doctor / doctor work | 医生 / 医生工作 | Core `Doctor.labelShort` (the `{0}` of `MDWC_WorkSummary` is this label). |
| caring work | 医疗工作 | Core Keyed `WorkTagCaring`. |
| Work tab | 「工作」面板 | Core `WorkTab.helpText`. |
| manipulation / sight | 操作能力 / 视觉能力 | Core `Manipulation.label`, `Sight.label`. |
| pain / pain shock | 疼痛 / 因疼痛而休克 | Core Keyed `Pain`, `PainShock`. |
| downed | 倒地不起 | Core `Wait_Downed.reportString`, `DownedCannotCrawl`. |
| casting time / range / line of sight required | 释放时间 / 射程 / 目标必须可视 | Core Keyed `AbilityCastingTime`, `Range`, `AbilityRequiresLOS`. |
| Mote (the three motes' inherited label) | 特效 | Core `Mote_*` labels (`Mote_Clean`, `Mote_Stun` and most others). |
| trait description tokens | {PAWN_nameDef} / {PAWN_pronoun} / {PAWN_objective} kept as is | Core `Wimp`, `Tough`, `Abrasive` trait descriptions. |
| suppressability (Workshop, CE) | 压制敏感度 | CE's own zh-Hans `Suppressability.label`. |
| vacuum resistance (Workshop, VGE) | 真空抗性 | Odyssey `VacuumResistance.label`. |
| stranger in black / man in black / woman in black / strangers in black | 黑衣人 (all four labels) | Core `StrangerInBlack` PawnKindDef label, labelMale, labelFemale and labelPlural, all 黑衣人; `MDWC_StrangerInBlack` copies them, and 黑衣人 is also the settings section header, matching the Workshop's 黑衣人 section. |
| duster / revolver | 防尘大衣 / 左轮手枪 | Core `Apparel_Duster.label`, `Gun_Revolver.label`. |
| arrival letter | 信件 | Core Keyed `ShowLetters` (显示信件), `MoreLetters`. |
| warcasket slugthrower | 战棺霰弹枪 | Community VFEP pack (`q847633684/Vanilla-Expanded-ZH`); 霰弹枪 as Core `Gun_PumpShotgun` (泵动霰弹枪). |

## Pending native review

Every coinage in "Set-specific rows": 医疗型, 纳米机械包扎 / 包扎, 纳米机械包扎系统,
载药纳米机械, 肾上腺素注射 (ability and labelNoun), 肾上腺素注射器, the stage
labels 飙升 / 平稳 / 消退 / 虚脱, 诊断面罩, 医疗辅助模块, 战地医疗机械臂, 战地军医,
袭击者医疗兵, 药品 for generic medicine, 伤势严重程度 and 每医疗包治疗量, 粉,
每处伤口用时, 增益持续时间, 虚脱持续时间, 自动施放, 已启用的工作类型, 针剂可止住{0},
一整套黑色战棺, 完成战棺研究后, 性别不固定, 此人 in the stranger letter, and both
charge nouns. All free prose: the three descriptions (paragraph 1 after the family
opener, paragraphs 2 and 3), the two ability descriptions, the hediff and trait
descriptions, every settings tooltip and summary, and the whole Workshop page.
Three family frames the register read questioned, left as shipped so the sets
read alike until a native review rules: 让…始终保持满额 for "keeps X at full"
(vanilla states capacities as percentages), 躯干 as the Workshop name for the
body piece, and 体积携带能力 for Combat Extended's carry bulk.
The VFEP terms' review is tracked upstream, under the section named above.
