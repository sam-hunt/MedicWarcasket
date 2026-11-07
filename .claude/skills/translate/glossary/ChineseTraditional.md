# Traditional Chinese glossary, Medic Warcasket

First pass: machine-assisted generation, 2026-11-05, no native review yet.
Only mod-specific coinages and grounding decisions. The VFEP warcasket
vocabulary (warcasket, the label pattern, the parts, foundry, entomb, VFEP's
set names and research labels, pilot, the spacer-tier opener and its
"standard protection" tail) lives upstream in
`l10n/languages/ChineseTraditional.md` under "Grounded common vocabulary, VFE
Pirates warcasket domain", with its grounding source (the zh-Hans pack
`q847633684/Vanilla-Expanded-ZH`, converted to zh-Hant vocabulary; the
Traditional Workshop pack's files could not be obtained) and what is pending
native review; the settings words the family settled (Default/Off/On, the
default suffix, the `cooldownGerund`) sit in that file's main table. Only the
spacer opener is the family's here: Medic's descriptions carry their own
paragraphs 2 and 3 (the refit lore and the medic-series paragraph), not VFEP's
10th-generation paragraph, so those two are this mod's prose and are recorded
below. Nothing here restates an upstream row; a VFEP term is changed there
first. Vanilla terms are grounded against the Core, Royalty, Biotech, Anomaly
and Odyssey `ChineseTraditional (繁體中文).tar`. The zh-Hans pass was read as a
sibling precedent only; every row below was re-grounded in the zh-Hant tars,
and several differ (kit, mechanite, heart attack, go-juice, wake-up).

## Set-specific rows

| English | Traditional Chinese | Grounding / notes |
|---|---|---|
| medic (the set name, `<set>`型) | 醫療型 | Coined on the upstream `<set>`型 pattern (破襲型, 毒素型, 破艦型); 醫療 is Core `Medicine.skillLabel`. Labels 醫療型戰棺裝甲 / 醫療型戰棺護肩 / 醫療型戰棺頭盔. |
| Workshop title / `MDWC_SettingsCategory` | 醫療型戰棺 | Set name + 戰棺 (the anchor term); the two are equal (title coupling). |
| trait label `warcasket` | 戰棺 | The upstream bare warcasket noun, as VFEP's own trait reads in the zh-Hans pack (战棺). |
| shell (the opener's body-piece slot) | 裝甲 | Toxin's shipped 戰棺裝甲, matching the label 醫療型戰棺裝甲; Shipcracker's 戰棺外殼 was not followed. Upstream flags the slot as unreconciled. |
| mechanite(s) | 機械分子 | Vanilla's dominant rendering: Core `MechSerumHealer`, `MechSerumResurrector`, `Luciferium*` descriptions (19 Core hits, 12 Biotech). The infection hediffs' 機械蟲 (`FibrousMechanites`, 4 hits) names a parasite, the wrong sense for a healing tool. |
| mechanite dressing (ability, section header) / dress | 機械分子包紮 / 包紮 | Coined. 包紮 is vanilla's own word for a dressed wound (`*.comps.HediffComp_TendDuration.labelTendedWell` 已包紮). |
| mechanite dressing suite (the armor's reloadable) | 機械分子包紮系統 | Coined; 系統 so it reads as built in. |
| carrier mechanites | 載藥機械分子 | Coined. |
| adrenaline / adrenaline shot (ability, section header, labelNoun) | 腎上腺素 / 腎上腺素注射 | 腎上腺素 per Core `KingOfPirates25`, Biotech `Aggression_HyperAggressive`, Anomaly `GhoulFrenzy`; the `X注射` noun follows Anomaly's hediff label `MetalbloodInjection` 鐵血注射. |
| adrenaline injector | 腎上腺素注射器 | 注射器 per Royalty `VenomFangs.description` and Anomaly serum descriptions. |
| the four levels: rush / steady / wearing off / crash | 亢奮 / 平穩 / 消退 / 虛脫 | Coined, two characters bare like vanilla's stage labels (`HeartAttack` 痛苦 / 衰弱 / 致命). 亢奮 follows `GoJuiceHigh.stages.high_on_gojuice` 能量果飲亢奮; `Anesthetic.stages.wearing_off` 漸漸恢復 is recovery from sedation, a different sense. 虛脫 also renders "crash" and "crashing" in the settings. |
| diagnostic visor | 診斷面罩 | Coined; 診斷 per Core `MedicineIndustrial.description` (診斷設備); 面罩 as a helmet visor (Core's `Apparel_ClothMask` is also 面罩, a known overlap). |
| medical assistance module | 醫療輔助模組 | Coined on Shipcracker's shipped 近戰輔助模組; 模組 is the Taiwan form. |
| field-medicine manipulators | 戰地醫療機械臂 | Coined; 戰地 per Core `CombatMedtech59.title` 戰地醫護兵, 臂 as Shipcracker's 破壁臂. |
| field surgeons / surgeons | 戰地外科醫生 / 外科醫生 | Core `GlitterworldSurgeon15.titleShort` 外科醫生, 戰地 as above. Lore paragraph 2. |
| combat medic (Workshop) / raider medic | 戰地醫護兵 / 襲擊者醫療兵 | Core `CombatMedtech59.title` 戰地醫護兵 and `CombatMedic82.title` 醫療兵; 襲擊者 per upstream. |
| squadmate | 戰友 | Core `OverwatchSniper42.description`. Lore paragraph 3 and the Workshop. |
| patient | 患者 | Core `Doctor.description` (幫患者拿食物) and the mechanite hediff descriptions. |
| medicine (generic) / medicine gauge | 藥品 / 藥品儀表 | Core `ThingCategoryDef` `Medicine.label` 藥品; 儀表 per Toxin's shipped gauge. |
| injury severity / severity per kit | 傷勢嚴重程度 / 每藥包治療量 | 傷勢 per Core `MechSerumHealer.description`, 嚴重程度 per Core Keyed `ConfigurableSeverity`; the stat label is coined. |
| illness (disease, infection or missing part, in the summaries) | 病症 | Core `HeartAttack.description` (病症可能會惡化). |
| reload item / reload target / never reload / reloads with | 裝填物品 / 裝填目標 / 從不裝填。 / 以{0}重新裝填。 | 裝填 per Core Keyed `Reload` (以{AMMO_label}重新裝填{GEAR_labelShort}). |
| open kit (gauge) | 已拆開的藥包 | Coined. |
| effect color / Blue / Green / Pink | 特效顏色 / 藍色 / 綠色 / 粉紅色 | Core ColorDefs `Blue`, `Green`, `Pink` labels (Core Keyed `Blue` / `Green` are untranslated English); 特效 coined. |
| time per wound / boost duration / crash duration | 每處傷口用時 / 增益持續時間 / 虛脫持續時間 | Coined; 持續時間 per Core `Ability_Duration.label`. |
| range (settings label) | 距離 | Core `Ability_Range.label`, as Toxin and Shipcracker ship; Core Keyed `Range` 射程 is the weapon slot. |
| autocast | 自動施放 | Coined on Core `AbilityCastingTime` 施放時間; no vanilla string names autocast, VEF ships English only. |
| enabled work types / enabled / disabled | 已啟用的工作類型 / 啟用 / 停用 | Core Keyed `Enabled` 啟用, `Disabled` 停用; they match the English state words, and Shipcracker's toggle frame uses the same pair. |
| the Medic paragraphs 2 and 3 | 醫療型屬於第十代戰棺，出自一項政府計畫…… / 裝甲內建機械分子包紮系統，可就地治療戰友的傷口…… | Medic's own prose, byte-identical across the three descriptions; the full text is `1.6/Languages/ChineseTraditional/DefInjected/ThingDef/Apparel_Medic.xml`. 計畫 and 水準 per the Taiwan register (corpus 計畫:計劃 147:3, 水準:水平 26:1). |
| StarCraft (Workshop FAQ) | 《星海爭霸》 | The game's official Taiwan title, in 《》 as vanilla cites product names. |

## Ported from the siblings

| English | Traditional Chinese | Source |
|---|---|---|
| Default: {0}. | 預設值：{0}。 | Toxin and Shipcracker Keyed, every tooltip; also `MDWC_WorkDefault`. |
| (default) | (預設) | Both siblings' `DefaultSuffix`, no leading space. |
| When enabled, … When disabled, … | 啟用時，… 停用時，… | Shipcracker `SCWC_VacuumBurnDesc`. |
| These settings are unavailable… built-in values. | 這些設定目前無法使用。另一個模組修改了醫療型戰棺，因此這套戰棺會沿用內建數值。 | Shipcracker `SCWC_SettingsUnavailable` ("will use" 會使用 → "keeps" 會沿用); "definitions" dropped as modder vocabulary. |
| The time the pilot must wait after X before Y. | 駕駛員X後，必須等待這段時間才能再次Y。 | Shipcracker `SCWC_CooldownDesc`. |
| cooldown / casting time | 冷卻時間 / 施放時間 | Toxin and Shipcracker labels; Core Keyed `CooldownTime`, `AbilityCastingTime`. |
| in cells | 以格為單位 | Shipcracker / Toxin unit wording (格). |
| Workshop frames | 為[url=…]…[/url]新增一套太空級的戰棺。 / 關於 / 套裝 / 模組相容性 / 常見問題 / 連結 / `[b]裝甲主體：[/b]` / 護肩 / 頭盔 / 可安全加入現有的存檔。 / 有殖民者封在這套戰棺內時，請勿移除本模組。 / CE 護甲數值、穿著體積與體積攜帶能力比照… / VGE 遵循其戰棺的…規則 / SOS2 line / 各部件也能和VFE Pirates原有的套裝自由混搭。 / 問：要怎麼取得？ 答：完成VFE Pirates的「太空戰棺」研究後，和其他套裝一樣到戰棺鑄造廠組裝即可。 / 問：這會不會不平衡？ / 模組設定 / the links line | Toxin and Shipcracker Workshop pages (Toxin's links line, Toxin's 太空級 over Shipcracker's 太空等級). |
| keeps X at full | 讓…保持滿值 | Shipcracker Workshop (呼吸能力保持滿值). |
| VFE Pirates (in prose) | VFE Pirates | Both siblings keep the mod name in English; also `MDWC_WorkSummary`. |

## Charge nouns

| `chargeNoun` | Traditional Chinese | Forms and lookup |
|---|---|---|
| kit (the armor's `CompDressingSuite`) | 藥包 | One form. Chinese falls to the base `Pluralize` (lookup, else unchanged), and the zh-Hant Keyed never switches on `CHARGENOUN_gender` (0 hits), so no `WordInfo` file is shipped. **Not 醫療包**: that is Core's `MedicineIndustrial.label`, so the medicine the kit holds would collide with the kit itself (最多可容納{0}個醫療包，使用醫療包). 藥包 has no vanilla hit and is coined. Read aloud in every vanilla slot: 藥包剩餘 (`Stat_Thing_ReloadChargesRemaining_Name`), 這個物體最多能保持的藥包數量, 每裝填一藥包需要消耗物品的數量, 每藥包裝填消耗, 藥包裝填消耗, 這個道具會在最後一個藥包使用完後消失, 沒有藥包。 (`CommandReload_NoCharges`). The mod's own strings count it with 個. |
| shot (the shoulders' `CompApparelReloadable`) | 針劑 | One form, no lookup file, as above. No vanilla hit; the ordinary pharmacy word for an injectable dose, chosen because 注射 is the verb in vanilla (Anomaly `ingestCommandString` 注射{0}) and does not stand as a counted noun (沒有注射。 does not read). Reads in every slot as 藥包 does (沒有針劑。, 針劑剩餘). The mod's Keyed counts it with 支 and calls one dose 一針. |

## Grounded in the first pass

| English | Traditional Chinese | Source |
|---|---|---|
| herbal medicine / medicine / glitterworld medicine | 草藥 / 醫療包 / 高科技藥品 | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` labels (arrive at runtime; named in no string). |
| go-juice / wake-up | 能量果飲 / 醒神劑 | Core `GoJuice.label`, `WakeUp.label`. |
| heart attack | 心臟衰竭 | Core `HeartAttack.label` (vanilla's own rendering, kept although 心臟病發作 is the medical term). |
| infection / disease | 感染 / 疾病 | Core `WoundInfection.label` 傷口感染; 疾病 per Core `Doctor.description`. |
| missing body part | 缺少身體部位 | Core `MissingBodyPart.label`. |
| tend (the action) / tend quality / medical tend speed | 治療 / 治療品質 / 醫療速度 | Core `TendPatient.reportString` 治療TargetA, Keyed `TendQuality` 治療品質, `MedicalTendSpeed.label` 醫療速度. |
| has nothing to tend (invalid target) | {0}不需要照護。 | Core Keyed `TendingNotRequired` {0}不需要照護. |
| is hostile | {0}是敵對的。 | Core Keyed `ConfirmLandOnHostileFactionBase` ({0}是敵對的。). |
| medical skill | 醫療技能 | Core `Medicine.skillLabel` 醫療. |
| doctor work (trait) / the `{0}` of `MDWC_WorkSummary` | 醫護工作 / 醫生 | Core `Doctor.label` 醫護 for prose; `Doctor.labelShort` 醫生 is what the code passes in. |
| caring work | 照顧工作 | Core Keyed `WorkTagCaring` 照顧, on the 知識工作 pattern of `WorkTagIntellectual`. |
| Work tab | 「工作」面板 | Core `WorkTab.helpText` (`WorkTab.label` is 分配工作). |
| manipulation / sight | 操作能力 / 視力 | Core `Manipulation.label` 操作 (操作能力 in prose, `ThrumboRoar.description`), `Sight.label`. |
| pain / pain shock / unconscious | 疼痛 / 劇痛休克 / 失去意識 | Core prose 疼痛 (`PainShockThreshold.label` 疼痛忍受值), Keyed `PainShock`, `Unconscious`. |
| downed | 倒地 | Core `PainShockThreshold.description`; upstream row. |
| line of sight | 視線內 | Core `AbilityRequiresLOS` 所需視線; Shipcracker's prose 視線內. |
| surgery / feeding / rescue (Workshop) | 手術 / 餵食 / 救援 | Core `MedicalSurgerySuccessChance.label` 手術成功率, `FeedPatient.reportString` 給TargetB餵食TargetA, `Rescue.reportString` 救援TargetA. |
| JobDef reportString shape | 包紮TargetA的傷口 / 為TargetA注射腎上腺素 | Bare verb phrase, no 。, as every Core reportString; the abilities' `jobReportString`s (進行機械分子包紮 / 注射腎上腺素) take the same bare shape. |
| Mote (the three motes' inherited label) | 塵土 | Core `Mote_*` labels (52 of 64 are 塵土). |
| trait description tokens | {PAWN_nameDef} / {PAWN_pronoun} / {PAWN_objective} kept as is | Core `Wimp`, `Kind`, `Tough` trait descriptions (curly-brace tokens, objective as the object of 讓/影響). |
| vacuum resistance (Workshop, VGE) | 真空抵抗力 | Odyssey `VacuumResistance.label`. |
| suppressability (Workshop, CE) | not named; 更難被壓制 | CE 16.7.4's zh-Hant `Suppressability.label` is 抗抑制能力, which reads as the inverse of the stat (lower suppressability means harder to suppress), so the line states the effect instead. |
| x of y (counts) | {0}/{1} | Vanilla zh-Hant sets the slash solid (`MassCarried` 負重：{0}/{1}公斤, `PregnantIconDesc`), where the English spaces it. |

## Man in black

| English | Traditional Chinese | Grounding / notes |
|---|---|---|
| `MDWC_StrangerInBlack` label / labelFemale / labelMale / labelPlural | 神秘黑衣人 / 黑衣女 / 黑衣男 / 神秘黑衣人 | Core `PawnKindDef` `StrangerInBlack.*` verbatim (the kind is a copy of vanilla's, and the incident's letter names the arrival by its kind label). |
| Man in black (section header) | 神秘黑衣人 | The kind's neutral label, as the Workshop page's `[h2]` heads the same feature; the incident's own label 黑衣男 (`StrangerInBlackJoin.label`) is male, wrong for a section that can lift the forced gender. |
| the stranger (in prose) | 黑衣人 | The Workshop page's prose (讓黑衣人穿著這套戰棺登場); short form of the kind label. |
| arrive (the stranger) | 登場 | Core `StrangerInBlackJoin.letterText` (一名神秘的[PAWN_kind]登場了). |
| duster / revolver | 風衣 / 左輪手槍 | Core `Apparel_Duster.label`, `Gun_Revolver.label`. |
| warcasket slugthrower | 戰棺霰彈槍 | Derived from the zh-Hans pack's 战棺霰弹枪; 霰彈槍 matches Core `Gun_PumpShotgun` 泵動霰彈槍. |
| warcasket kit (the summaries: the whole set) | 全套…戰棺 (全套黑色戰棺) | Coined. 全套 names the set and keeps clear of the 藥包 charge noun and Core's 醫療包. |
| no forced gender | 不限性別 / 性別不再固定 | Coined; the label and summary use 不限性別, the tooltip 性別不再固定 … 停用時，性別會照常固定. |
| colony close to falling | 殖民地瀕臨崩潰 | 瀕臨 per Core backstory prose (瀕臨滅絕); 瀕臨崩潰 has a corpus hit. |
| researched warcaskets | 研究出戰棺 | Generic, no research label cited: VFEP's `VFEP_Warcaskets` label is not recorded upstream. |
| letter label while the gender is unfixed (`MDWC_StrangerLetterLabel`) | 神秘黑衣人 | The kind's neutral label, as the English uses its own; vanilla's letter title 黑衣男 is male. |
| letter text while the gender is unfixed (`MDWC_StrangerLetterText`) | 危險的氣息，一名神秘的[PAWN_kind]登場了。\n\n這位神秘人是否能搞定一切呢？ | Core `StrangerInBlackJoin.letterText`, its first sentence verbatim ([PAWN_kind] resolves to 黑衣男 / 黑衣女 by gender). Vanilla's `{PAWN_pronoun}` subject is replaced by 這位神秘人 because `check-translations.py` rejects a curly placeholder the English lacks. The text is gender-matched rather than neutral, so `MDWC_StrangerAnyGenderDesc` says the letter's wording 隨之調整 (adjusts) instead of "neutral wording". |
| a shot stops a {0} (the heart attack toggle) | 針劑可止住{0} / 腎上腺素注射可以止住{1}，此時不會提供增益 | {0}/{1} arrive as Core `HeartAttack.label` 心臟衰竭; 針劑 is the shot charge noun, 可止住 as `MDWC_ShotStopsHeartAttack`. |

## Pending native review

Every coinage in "Set-specific rows": 醫療型, 機械分子 over 機械蟲, 機械分子包紮 /
包紮, 機械分子包紮系統, 載藥機械分子, 腎上腺素注射 (ability and labelNoun),
腎上腺素注射器, the stage labels 亢奮 / 平穩 / 消退 / 虛脫, 診斷面罩, 醫療輔助模組,
戰地醫療機械臂, 戰地外科醫生, 已拆開的藥包, 每藥包治療量, 特效顏色, 每處傷口用時,
增益持續時間, 虛脫持續時間, 自動施放, 已啟用的工作類型, both charge nouns (藥包,
針劑) and the 裝甲 shell slot. Vanilla's own 心臟衰竭 for heart attack makes
可止住心臟衰竭 read oddly; kept for consistency with the health tab. All free
prose: the three descriptions (paragraph 1 after the family opener,
paragraphs 2 and 3), the two ability descriptions, the hediff and trait
descriptions, every settings tooltip and summary, and the whole Workshop page.
A fresh register read rewrote the calques it found (the hediff opener now
follows vanilla's 流淌在血液裡的X。, the autocast and programme agents became
people). 自動施放 has no vanilla or VEF rendering and stays pending. The
"Man in black" rows: the derived 戰棺霰彈槍, the coined 全套黑色戰棺 and
不限性別, the letter's 這位神秘人 and the tooltip's 隨之調整 claim. The VFEP terms' review is tracked upstream,
under the section named above.
