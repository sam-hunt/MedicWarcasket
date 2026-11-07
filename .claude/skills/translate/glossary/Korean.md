# Korean glossary, Medic Warcasket

First pass: machine-assisted generation, 2026-11-05, no native review yet.
Only mod-specific coinages and grounding decisions; family-wide mechanics, josa
markers and style live in `l10n/languages/Korean.md`. The VFEP warcasket
vocabulary (warcasket, the parts, the label pattern, foundry, entomb, VFEP's
set names and research labels, pilot, the spacer-tier opener) lives upstream
in that file under "Grounded common vocabulary, VFE Pirates warcasket domain",
with the grounding source (the RimWorldKorea `RMK` pack, GitHub
RimWorldKorea/RMK) and what is pending native review; the settings and ability
words the family settled (spacer-tech, Off/On, default, cooldown, range, the
`cooldownGerund`) sit in that file's main table. Only the spacer opener is the
family's here: Medic's descriptions carry their own paragraphs 2 and 3 (the
refit lore and the medic-series paragraph), not VFEP's 10th-generation
paragraph, so those two are this mod's prose and are recorded below. Nothing
here restates an upstream row; a VFEP term is changed there first. Vanilla
terms are grounded against the Core and Odyssey `Korean (한국어).tar`.

## Set-specific rows

| English | Korean | Grounding |
|---|---|---|
| medic (the set name, the bare label prefix) | 의무병 | Core backstory titles `CombatMedic82` (전투 의무병), `MedicSoldier54.titleShort` (의무병). Labels 의무병 워캐스킷 / 의무병 워캐스킷 어깨 / 의무병 워캐스킷 헬멧 on the upstream pattern. Also "medic pilots" 의무병 조종사 and "raider medics" 습격자 의무병. |
| Workshop title / `MDWC_SettingsCategory` | 의무병 워캐스킷 | Set name + 워캐스킷 (the anchor term); the two are equal (title coupling). |
| trait label `warcasket` | 워캐스킷 | The bare upstream noun, as RMK labels VFEP's own warcasket research and base set (`VFEP_Warcaskets.label`, `VFEP_Warcasket_Warcasket.label`). |
| mechanite(s) | 기계입자 | Core `FibrousMechanites` (근섬유질 기계입자), `SensoryMechanites`, `MechSerumResurrector.description`. |
| mechanite dressing (ability, section header) / dress | 기계입자 처치 / 처치하다 | Coined. 처치 is the wound-care word of 응급 처치 (Core `MarbleDoctor99`, `BoyScout42`); 붕대 감음 is Core's tended-wound state, too narrow for the stream. |
| mechanite dressing suite (the armor's reloadable) | 기계입자 처치 장치 | Coined; 장치 so it reads as built in. The Workshop says 의약품 키트를 담는 장치. |
| carrier mechanites | 운반용 기계입자 | Coined. |
| adrenaline / adrenaline shot (ability, section header, labelNoun) | 아드레날린 / 아드레날린 주사 | No vanilla ko string names adrenaline; 아드레날린 is the standard medical loanword. Hediff label 아드레날린, labelNoun 아드레날린 주사. |
| adrenaline injector | 아드레날린 주입기 | Coined; 주입기 per Core's fuelling port (연료 주입기). |
| the four levels: rush / steady / wearing off / crash | 고조됨 / 안정됨 / 약해짐 / 탈진 | Coined on vanilla's `-됨` stage-label family (`Anesthetic` 혼미함 / 안정됨 / 마취 풀림, `Flu` 가벼움 / 심각함). 탈진 also renders "crash" and "crashing" in the settings and summaries. |
| diagnostic visor | 진단 바이저 | Coined; 진단 per Core `MedicineIndustrial.description` (진단 장치). Vanilla has no visor; 바이저 is the usual gaming loanword. |
| medical assistance module | 의료 보조 모듈 | Coined on Shipcracker's shipped 근접 전투 보조 모듈. |
| field-medicine manipulators | 야전 의료용 보조 팔 | Coined, after Shipcracker's 돌파용 강화 팔 (breaching arms). |
| field surgeons / surgeons | 야전 외과의 / 외과의 | 외과의 per Core `GlitterworldSurgeon15.titleShort`; 야전 per Core `Gun_AssaultRifle.description`. Lore paragraph 2. |
| government program (lore) | 정부 사업단 | Coined; the program is given a people-agent (사업단) so it can 깨닫다. |
| squadmate | 동료 | Core backstories (`CombatMedic82` 동료 병사들). |
| raider / allied medics, their own downed | 습격자와 아군의 의무병 / 쓰러진 자기편 | 습격자 per Core backstories, 아군 per Core `RaidFriendly.label`, 쓰러진 per Core `Downed.label` (쓰러짐). |
| your pilots | 내 조종사 | Upstream's 내 rule for "your". |
| reload item / reload target / never reload / reloads with {0} | 재장전 물품 / 재장전 목표 / 재장전하지 않습니다. / {0}(으)로 재장전합니다. | 재장전 per Core `Reload` ({AMMO_label}(으)로 {GEAR_labelShort} 재장전). |
| medicine (generic, the gauge header) / medicine gauge | 의약품 / 의약품 게이지 | Core `Medicine.label` (ThingCategory 의약품); 약품 is `MedicineIndustrial.label` and would read as one item. |
| injury severity / severity per kit | 부상 심각도 / 키트당 부상 심각도 | 심각도 per Core `AddictionSeverityPerDose` (복용당 중독 심각도); Core's `ConfigurableSeverity` says 심각성 in a different slot. The stat label is coined on the `~당` shape. |
| effect color / Blue / Green / Pink | 효과 색상 / 파란색 / 녹색 / 분홍색 | 파란색 / 녹색 per Core Keyed `Blue` / `Green`; Core has no `Pink` key, 분홍색 per `Hair_Pink.label` (분홍색 머리카락). |
| time per wound / boost duration / crash duration | 상처당 시간 / 강화 지속 시간 / 탈진 지속 시간 | Coined; 지속 시간 per Toxin. |
| open kit | 개봉한 키트 | Coined. |
| autocast / automatic shots | 자동 시전 (one term; "automatic shots" is 자동 시전으로 주사) | Coined; 시전 per Core `AbilityCastingTime`; VEF ships English only. |
| enabled work types | 활성화된 작업 유형 | Core `IncapableOfTooltipWorkTypes` (비활성화된 작업 유형:). |
| the Medic paragraphs 2 and 3 | 10세대 워캐스킷은 노획한 해적 장비를 정부 사업단이…… / 몸통의 기계입자 처치 장치는 동료의 상처를…… | Medic's own prose, byte-identical across the three descriptions; the full text is `1.6/Languages/Korean/DefInjected/ThingDef/Apparel_Medic.xml`. |
| Starcraft (Workshop FAQ) | 스타크래프트 | The game's official Korean title. |

## Ported from the siblings

| English | Korean | Source |
|---|---|---|
| Default: {0}. | 기본값: {0}. | Toxin and Shipcracker Keyed, every tooltip; also `MDWC_WorkDefault`. |
| (default) suffix | ` (기본값)` | Toxin and Shipcracker `*_DefaultSuffix`, leading space kept. |
| When enabled, … When disabled, … | 켜면 …. 끄면 …. | Shipcracker `SCWC_VacuumBurnDesc`. |
| enabled / disabled (the state words) | 켜짐 / 꺼짐 | The siblings' `기본값: 켜짐.` / `기본값: 꺼짐.` |
| These settings are unavailable… built-in values. | 이 설정은 사용할 수 없습니다. 다른 모드가 의무병 워캐스킷을 변경했기 때문에, 세트는 내장된 값을 사용합니다. | Shipcracker `SCWC_SettingsUnavailable`; "definitions" dropped as modder vocabulary (정의 never names game content in vanilla). |
| cooldown / range / casting time / line of sight | 재사용 대기 / 사거리 / 시전 시간 / 시야 안에 있어야 합니다 | Upstream rows; Shipcracker's 시야가 확보된. |
| cells | 칸 단위 | Toxin and Shipcracker `(칸)` captions. |
| Workshop frames | …[/url]에 새로운 우주시대 워캐스킷을 추가합니다. / 소개 / 세트 구성 / 모드 호환성 / 자주 묻는 질문 / 링크 / `[b]몸통:[/b]` `[b]어깨:[/b]` `[b]헬멧:[/b]` / 기존 세이브에 추가해도 안전합니다. / 정착민이 세트에 안치된 동안에는 모드를 제거하지 마세요. (the siblings' 제거하면 안 됩니다 drops the object; the register read named the mod) / CE head 방어도, 부피, 허용 부피가 다른 우주시대 워캐스킷과 비슷한 수준 / VGE head 워캐스킷용 진공 … 규칙을 따릅니다 / SOS2 line / 기본 세트와 부위를 마음대로 섞어 써도 됩니다. / Q: 어떻게 얻나요? A: …'우주시대 워캐스킷' 연구를 완료한 뒤, 다른 세트처럼 워캐스킷 주조대에서 안치하면 됩니다. / Q: 밸런스 붕괴 아닌가요? / 모드 설정 / links line | Shipcracker Workshop (opener, CE, VGE, SOS2, FAQ) and Toxin Workshop (세트 구성, links line). |
| VFE Pirates (in prose) | VFE Pirates | Both sibling Workshop pages keep the English short name; also `MDWC_WorkSummary`. |
| keeps X at full | …을 온전히 유지해 줍니다 | Shipcracker Workshop (호흡 능력을 온전히 유지해 주며). |

## Charge nouns

| `chargeNoun` | Korean | Forms and lookup |
|---|---|---|
| kit (the armor's `CompDressingSuite`) | 키트 | One form. Korean falls to the base `Pluralize` (lookup, else unchanged) and ko Keyed never switches on `CHARGENOUN_gender`, so no `WordInfo` file is shipped. Read aloud in every slot: `Stat_Thing_ReloadMaxCharges_Name` 키트, `_Desc` 이 개체의 최대 키트 수입니다., `Stat_Thing_ReloadPerCharge_Name` 키트당 재장전 비용, `_Desc` 하나의 키트를 재장전하는 데…, `Stat_Thing_ReloadDestroyOnEmpty_Desc` 마지막 키트를 사용하면…, `CommandReload_NoCharges` 남은 키트가 없습니다. 키트 is vowel-final, so every `(을)를` / `(이)가` marker after `{CHARGENOUN}` resolves to 를 / 가. `ChargesRemaining` is Core's fixed 잔탄 and carries no noun. The mod's own strings count it with 개 (최대 {0}개) and call it 의약품 키트 where the medicine matters. |
| shot (the shoulders' `CompApparelReloadable`) | 주사 | One form, no lookup file, as above. Reads in every slot: 주사, 이 개체의 최대 주사 수입니다., 주사당 재장전 비용, 하나의 주사를, 마지막 주사를, 남은 주사가 없습니다. Vowel-final, so the markers resolve as for 키트. Named alike in the mod's Keyed (주사: {0} / {1}, 장전된 주사가 없습니다., 주사 용량), counted with 개 and, for one dose given, 한 대. |

## Grounded in the first pass

| English | Korean | Source |
|---|---|---|
| herbal medicine / medicine / glitterworld medicine | 생약 / 약품 / 번화계 약품 | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` labels (arrive at runtime; named in no string). All three are consonant-final, which the `(이)가` / `(으)로` markers after `{0}` handle. |
| go-juice / wake-up | 고주스 / 웨이크업 | Core `GoJuice.label`, `WakeUp.label`. |
| heart attack | 심근경색 | Core `HeartAttack.label`. |
| infection / disease / illness | 감염 / 질병 / 질병 | Core `WoundInfection.label`; 질병 per Core `Disease_*` letter labels. |
| missing body part | 신체 누락 | Core `MissingBodyPart.label`. |
| wound | 상처 | Core `WoundInfection.description` (상처에 세균이 침투했습니다). |
| tend (the action) / tend quality / tend speed | 치료 / 치료 품질 / 치료 속도 | Core Keyed `Tend` ({0} 치료), `TendQuality`, `MedicalTendQuality.label`, `MedicalTendSpeed.label`. |
| has nothing to tend (invalid target) | {0}(은)는 치료가 필요하지 않습니다. | Core Keyed `TendingNotRequired` ({0}(은)는 치료가 필요하지 않음), in sentence form. |
| medical skill | 의학 기술 | Core `Medicine.skillLabel` (의학). |
| doctor work / caring work | 치료 작업 | Core `Doctor.labelShort` (치료; the `{0}` of `MDWC_WorkSummary` is this label) and Keyed `WorkTagCaring` (치료) agree, so both English phrases render alike. |
| tending, surgery, feeding, rescue (Workshop) | 간호, 수술, 음식 먹이기, 구조 | Core `TendPatient.reportString` (간호 중), `Surgery.label`, `FeedPatient.reportString`, `Rescue.reportString`. |
| Work tab | 작업 탭 | Core MainButtonDef `Work.label` (작업). |
| manipulation / sight | 조작 / 시력 | Core `Manipulation.label`, `Sight.label`. |
| pain / pain shock | 통증 (hediff, ability) and 고통 (trait) / 고통으로 실신 | Core Keyed `Pain` (통증) and `PainShock` (고통에 의한 실신); the trait keeps 고통 beside the pain-shock phrase, as Core's `Pain` hediff stages do (가벼운 고통). |
| downed | 쓰러진 | Core `Downed.label` (쓰러짐), `Wait_Downed.reportString`. |
| is hostile | {0}(은)는 적대적입니다. | Core Keyed `Hostile` (적대). |
| casting time / range | 시전 시간 / 사거리 | Core Keyed `AbilityCastingTime`, `Range`. |
| power armor (lore) | 동력 갑옷 | Core `Apparel_PowerArmor.description` (동력 갑옷). |
| plating / shields | 장갑판 / 보호막 | Core `Turret_Sniper.description` (장갑판); upstream 보호막 row. |
| Mote (the three motes' inherited label) | 모트 | Odyssey `Mote_*` labels (`Mote_FishingRod`, `Mote_BeamRepeaterLaser`, 7 of 7). Core leaves all 59 of its own `Mote_*` labels in English, so Odyssey's is the only Korean rendering vanilla ships. |
| trait description tokens | `{PAWN_nameDef}(은)는`, `{PAWN_pronoun}는`, `{PAWN_objective}` bare | Core trait descriptions (`Pyromaniac`, `GreatMemory`): the name takes a marker, the pronoun (그 / 그녀, always vowel-final) takes bare 는, and `{PAWN_objective}` already carries 를 (Grammar `ProhimObj` 그를). |
| reportString shape | TargetA의 상처 처치 중 / TargetA에게 아드레날린 주사 놓는 중 | Core `TendPatient` (TargetA 간호 중), `FeedPatient` (TargetB에게 …중): invariant particles only, no period. The abilities' `jobReportString`s read 기계입자 처치 중 / 아드레날린 주사 놓는 중. |
| suppression (Workshop, CE) | 제압 상태에 잘 빠지지 않습니다 | CE's own ko `Suppressed` stages (제압 당하는중) and `Suppressability.label` (제압 임계치); the stat name reads inverted against "reduces suppressability", so the line states the effect. |
| vacuum resistance (Workshop, VGE) | 진공 저항 | Odyssey `VacuumResistance_Partial.label`. |

## The stranger in black and the heart attack toggle

| English | Korean | Source |
|---|---|---|
| stranger / woman / man in black, strangers in black (`MDWC_StrangerInBlack` labels) | 검은 옷의 이방인 / 검은 옷의 여성 / 검은 옷의 남성 / 검은 이방인 | Core `StrangerInBlack` PawnKindDef labels, verbatim (the def is a copy of it). |
| Man in black (the settings section header) | 검은 옷의 이방인 | The neutral kind label, as the Workshop's section header already says; the incident itself is 구세주 in vanilla ko, so the section tooltip names it 구세주 too. Bare "the stranger" in settings prose is 이방인. |
| Stranger in black (the gender-free letter's label) | 구세주 | Core `StrangerInBlackJoin.letterLabel`, verbatim: already gender-free, so the swapped letter keeps the title players see in vanilla. |
| the gender-free letter's text | 위기의 순간 신비한 검은 옷의 이방인이 나타났습니다.\n\n과연 이 위기에서 정착지를 구할 수 있을까요? | Core `StrangerInBlackJoin.letterText` with `[PAWN_kind](이)가` replaced by the neutral label and `[PAWN_pronoun]는` dropped (Korean leaves the subject unsaid), so no token can reveal a gender and `MDWC_StrangerAnyGenderDesc`'s claim stays true. |
| arrival letter | 이방인이 나타날 때 오는 편지 | Core Keyed `AutomaticPauseModeSetting` (편지 도착 시 일시 정지), `MoreLetters`. |
| forced gender / remove it / no forced gender | 성별 고정 / 성별 고정을 풀다 / 이방인의 성별 고정 해제 (label), 성별이 고정되지 않습니다 (prose) | Coined. |
| duster / revolver | 바람막이 / 리볼버 | Core `Apparel_Duster.label`, `Gun_Revolver.label`. |
| warcasket slugthrower (VFEP) | 워캐스킷 산탄총 | RMK VFEP pack's label. |
| warcasket kit (the stranger summaries) / black | 워캐스킷 세트 / 검은색 | 세트 so it cannot read as the medicine 키트; 검은색 per the upstream `~색` colour rule. |
| researched warcaskets | '워캐스킷' 연구 (완료) | RMK `VFEP_Warcaskets.label` (the gate's research), cited in ASCII single quotes per upstream. |
| Another mod chooses the stranger | 다른 모드가 이방인을 정하기 때문에 | Shaped like `MDWC_SettingsUnavailable` (다른 모드가 …했기 때문에). |
| Shot stops a {0} / the boost | 주사로 {0}(을)를 멈춤 / 강화 효과 | `{0}` is Core `HeartAttack.label` 심근경색 at runtime, marked anyway; the label mirrors `MDWC_ShotStopsHeartAttack` (심근경색을 멈춤); 강화 per `MDWC_ShotBoost`. |

## Pending native review

Every coinage in "Set-specific rows": 의무병 as the set prefix, 기계입자 처치 /
처치하다, 기계입자 처치 장치, 운반용 기계입자, 아드레날린 주사, 아드레날린 주입기, the
stage labels 고조됨 / 안정됨 / 약해짐 / 탈진, 진단 바이저, 의료 보조 모듈, 야전 의료용 보조
팔, 정부 사업단, 의약품 as generic medicine, 부상 심각도 and 키트당 부상 심각도, 분홍색,
상처당 시간, 강화 지속 시간, 탈진 지속 시간, 개봉한 키트, 자동 시전 / 자동 주사, and both
charge nouns. All free prose: the three descriptions (paragraph 1 after the
family opener, paragraphs 2 and 3), the two ability descriptions, the hediff
and trait descriptions, every settings tooltip and summary, and the whole
Workshop page. A fresh register read rewrote the calques it found. Two family frames the vanilla corpus questions, plus 사업단 for the lore's "government program" and 허용 부피 for Combat Extended's carry bulk,, left as shipped so the
sets read alike until a native review rules: 켜면 / 끄면 for the toggle frame
(vanilla tooltips say 활성화하면 / 비활성화하면) and 켜짐 / 꺼짐 for the state words
(Core `Enabled` 활성화됨, `Disabled` 사용 안 함).
From the stranger update: the section header 검은 옷의 이방인 against vanilla's
incident name 구세주, the letter title 구세주 for English "Stranger in black",
the gender-free letter text, 성별 고정 해제, 워캐스킷 세트 for "warcasket kit", and
the toggle labels 의무병 워캐스킷을 입은 이방인 and 주사로 {0}(을)를 멈춤.
The VFEP terms' review is tracked upstream, under the section named above.
