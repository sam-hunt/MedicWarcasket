# Japanese glossary, Medic Warcasket

First pass: machine-assisted generation, 2026-11-05, no native review yet.
Only mod-specific coinages and grounding decisions; engine mechanics,
punctuation (ASCII `.` and `,` with no following space) and style live in
`l10n/languages/Japanese.md`. The VFEP warcasket vocabulary (warcasket, the
parts, the label pattern, foundry, entomb, VFEP's set names and research
labels, pilot, the spacer-tier opener) lives upstream in that file under
"Grounded common vocabulary, VFE Pirates warcasket domain", with the grounding
source (no community VFEP Japanese pack; only the noun ウォーキャスケット is
attested) and what is pending native review; the settings and ability words
the family settled (spacer-tech, Off/On, default, cooldown, the
`cooldownGerund`) sit in that file's main table. Only the spacer opener is the
family's here: Medic's descriptions carry their own paragraphs 2 and 3 (the
refit lore and the medic-series paragraph), not VFEP's 10th-generation
paragraph, so those two are this mod's prose and are recorded below. Nothing
here restates an upstream row; a VFEP term is changed there first. Vanilla
terms are grounded against the Core and Odyssey `Japanese (日本語).tar`.

## Set-specific rows

| English | Japanese | Grounding |
|---|---|---|
| medic (the set name, the bare label prefix) | メディック | Core backstory `SpaceMarineMedic10.titleShort` (メディック); katakana per the family's set-name habit (トキシン, シップクラッカー). Labels メディック・ウォーキャスケット / メディック・ウォーキャスケットショルダーアーマー / メディック・ウォーキャスケットヘルメット on the upstream pattern. Also "medic pilots" メディックのパイロット and "raider medics" 襲撃者のメディック. The Workshop's "combat medic" is 衛生兵 (Core `CombatMedic82.titleShort`). |
| Workshop title / `MDWC_SettingsCategory` | メディック・ウォーキャスケット | Equal to the armor label and to each other (title coupling); contains the anchor ウォーキャスケット. |
| trait label `warcasket` | ウォーキャスケット | The bare upstream noun; no community pack renders VFEP's trait. The description says 封入されています for "is entombed" (upstream entomb row) and names the work 治療の作業 (Core `Doctor.gerundLabel`). |
| entombed (trait, `MDWC_WorkDesc`, Workshop compat line) | 封入 | The upstream entomb row, followed over Toxin's shipped 溶接されている間. Shipcracker's Workshop line ships 封入. |
| build at the foundry (Workshop set line and FAQ) | ウォーキャスケット鋳造台で溶接します | Both siblings' Workshop FAQ; grounded in VFEP's own `VFEP_DoWelding` work giver (verb "weld at", label "welding at warcasket foundry"). Kept over 製作 so the family's FAQ reads alike. |
| mechanite(s) | メカナイト | Core `FibrousMechanites.description`, `SensoryMechanites.description`, `SentienceCatalyst.description`. The hediff labels say 機械化症, a disease name, not the particle. |
| mechanite dressing (ability, section header) / dress | メカナイト処置 / 処置する | Coined. 処置 is the medical "treatment, dressing" word (応急処置); 看護 stays for vanilla's tend. The `reportString` reads TargetAの傷を処置中, the `jobReportString` メカナイト処置中. |
| mechanite dressing suite (the armor's reloadable) | メカナイト処置装置 | Coined; 装置 so it reads as built in. The Workshop says 薬のキットを蓄える装置. |
| carrier mechanites | 運搬用メカナイト | Coined. |
| adrenaline / adrenaline shot (ability, section header, labelNoun) | アドレナリン / アドレナリン注射 | アドレナリン per Core backstory `KingOfPirates25` (アドレナリンラッシュ). The hediff label is アドレナリン, its labelNoun アドレナリン注射. |
| adrenaline injector | アドレナリン注射器 | Coined; the plain medical word. |
| the four levels: rush / steady / wearing off / crash | 高揚 / 安定 / 減退 / 反動 | Coined as short nouns, on vanilla's noun stage labels shown in brackets (`HeartAttack` 苦痛 / 衰弱 / 致命的, `Flu` 軽度 / 中度 / 重度). 反動 also renders "crash" and "crashing" in the settings and summaries (反動中の患者). |
| diagnostic visor | 診断バイザー | Coined; 診断 per Core `MedicineUltratech.description` (ナノマシン診断). Vanilla has no visor. |
| medical assistance module | 医療補助モジュール | Coined on Shipcracker's shipped 格闘補助モジュール. |
| field-medicine manipulators | 野戦医療用の補助アーム | Coined after Shipcracker's ブリーチングアーム; 野戦 per Core `Gun_AssaultRifle.description`. |
| field surgeons / surgeons | 野戦外科医 / 外科医 | 外科医 per Core `GlitterworldSurgeon15.titleShort`. Lore paragraph 2. |
| government program (lore) | 政府の計画 | Coined; 計画 as the upstream 10th-generation paragraph uses it (この計画は). Its finding is agentless (〜ことが分かりました). |
| veterans (lore) | 退役軍人 | Core `MechWarVeteran77.title`. |
| squadmate / the wounded | 仲間 / 負傷者 | 仲間 per Core backstories (59 hits). |
| raider / allied medics, their own downed | 襲撃者や友軍のメディック / 倒れた味方 | 襲撃者 per Core backstories, 友軍 per Core `MessageFriendlyFightersLeaving`, 倒れた per Core `Wait_Downed.reportString` (倒れている), 味方 per Core `FriendlyFireSafety`. |
| your pilots | あなたのパイロット | Vanilla letters' あなたの; used once per toggle tooltip. |
| reload / reload item / reload target / never reload / reloads with {0} | 充填 / 充填アイテム / 充填目標 / 充填しません. / {0}を使用して充填できます. | 充填 per Core Keyed `Reload` ({GEAR_labelShort} に {AMMO_label}を充填する) and `CommandReload_NoAmmo`, the reloadable comp's own word; the `{0}を使用して…できます.` frame per Core `LaunchFragGrenade.description` (スチールを使用して再装填できます.). Loaded is 充填済み / 充填されていません. |
| medicine (generic, gauge header) / kit of medicine | 薬 / 薬のキット | Core ThingCategory `Medicine.label` (薬); 医薬品 is `MedicineIndustrial.label` and would read as one item. 医薬品キット in Core `MedicineIndustrial.description` grounds キット. |
| injury severity / severity per kit | 怪我の深刻度 / キットあたりの深刻度 | 深刻度 per Core `AddictionSeverityPerDose` (消費毎に増える深刻度); 怪我 per Core `AbilityCannotCastNoHealableInjury` (治療可能な怪我). |
| effect color / Blue / Green / Pink | エフェクトの色 / 青 / 緑 / ピンク | 青 / 緑 per Core Keyed `Blue` / `Green`; Core has no `Pink` key, ピンク per `Hair_Pink.label` (ピンク色の髪). エフェクト is coined (no vanilla hit). |
| kit capacity / shot capacity | キット容量 / 注射容量 | On Toxin's タンク容量. |
| time per wound / boost duration / crash duration | 傷1箇所あたりの時間 / 効果の持続時間 / 反動の持続時間 | Coined; 持続時間 per Toxin; 〜あたり per Toxin's バイオ液化燃料1あたり. Wounds count with 箇所. |
| open kit | 開封済みのキット | Coined. |
| interrupted / out of {0} / stream broken (floating texts) | 中断 / {0}切れ / 照射途絶 | 中断 per Core `LetterSpeechInterrupted`; the other two coined. |
| autocast / automatic shots | 自動使用 | Coined (VEF ships English only); "automatic shots" is 自動使用でも注射を打ちません. |
| enabled work types | 有効な作業 | 有効 per Core Keyed `Enabled`; 作業 per Core `IncapableOfWorkTypeBecauseOfCapacities` (このタイプのすべての作業). |
| the warcasket trait (settings) | 才能・性格「ウォーキャスケット」 | Core Keyed `Traits` (才能・性格, the Bio tab section); the label cited in 「」 per the prose quoting rule. |
| casting time / range | 詠唱時間 / 射程 | Core Keyed `AbilityCastingTime`, `Range`, the labels vanilla's ability tooltip prints. 射程 is chosen over the siblings' 範囲 (タイル), which captions a jump distance; the summary prose says 始めるまでに{3}かかり rather than 詠唱. |
| the Medic paragraphs 2 and 3 | 政府の計画から生まれた第10世代の設計です.…… / 外殻のメカナイト処置装置は,仲間の傷を…… | Medic's own prose, byte-identical across the three descriptions; the full text is `1.6/Languages/Japanese/DefInjected/ThingDef/Apparel_Medic.xml`. |
| Starcraft (Workshop FAQ) | スタークラフト | The game's usual Japanese name. |
| man in black (the vanilla incident, `MDWC_SectionStranger`) | 黒衣の男 | Core IncidentDef `StrangerInBlackJoin.label`, verbatim; the section header names the vanilla incident as the English does. |
| stranger in black (`MDWC_StrangerInBlack` labels) / the stranger (settings prose) | 黒衣の不審者 / 黒衣の女 / 黒衣の男 / 黒衣の不審者達; 不審者 | Core PawnKindDef `StrangerInBlack` label, labelFemale, labelMale, labelPlural, verbatim (the def copies vanilla's kind). The settings say bare 不審者 for "the stranger"; the Workshop section is 黒衣の不審者. |
| the neutral arrival letter (`MDWC_StrangerLetterLabel` / `MDWC_StrangerLetterText`) | 黒衣の不審者 / 騒動を察知して,不思議な黒衣の不審者が到着した.\n\nこの状況をどうにかしてくれないだろうか? | Vanilla `StrangerInBlackJoin.letterText` with the kind's neutral label written in place of `[PAWN_kind]`, and `[PAWN_pronoun]は` dropped, the くれる carrying the subject; no `PAWN` token, so it reads the same for either gender and `MDWC_StrangerAnyGenderDesc` stays true. Plain past 到着した as in the vanilla letter. |
| arrives in a warcasket / as usual | ウォーキャスケット姿で登場 / 通常どおり(の姿で) | 姿で登場 from the Workshop section (このウォーキャスケット姿で登場させる); 通常どおり per Core Keyed `FactionsDisabledWarning`. |
| forced gender / no forced gender | 性別の固定 / 性別を固定しない | Core Keyed `Gender` (性別). |
| arrival letter | 登場時のレター | Core Keyed `ShowLetters` (レターを表示); vanilla never says 手紙 for a game letter. |
| warcasket slugthrower (VFEP weapon) | ウォーキャスケット散弾銃 | Coined; VFEP ships no Japanese and describes it as an upscaled pump shotgun (Core `Gun_PumpShotgun` ポンプ式散弾銃). Attached directly like ウォーキャスケット鋳造台. In game the weapon's own label stays VFEP's English. |
| warcasket kit (the set, in the stranger summaries) | ウォーキャスケット一式 | 一式 per Core `Apparel_PowerArmor.description` (軽装パワーアーマーの一式); keeps it apart from the medicine キット. |
| warcaskets researched | ウォーキャスケットの研究後 / ウォーキャスケットを研究してから | Coined, plain wording; the research's own label is the upstream 「宇宙世紀のウォーキャスケット」, not named here as the English names none. |
| Shot stops a {0} (heart attack toggle) / the boost | 注射で{0}を止める / 効果を発揮する | On `MDWC_ShotStopsHeartAttack` (心筋梗塞を止める); "boost" is 効果 as in `MDWC_ShotBoost` (効果の持続時間). |

## Ported from the siblings

| English | Japanese | Source |
|---|---|---|
| Default: {0}. | デフォルト: {0}. | Toxin and Shipcracker Keyed, every tooltip; also `MDWC_WorkDefault`. |
| (default) suffix | ` (デフォルト)` | Toxin and Shipcracker `*_DefaultSuffix`, leading space kept. |
| When enabled, … When disabled, … | 有効にすると,…. 無効にすると,…. | Shipcracker `SCWC_VacuumBurnDesc`; also vanilla's frame (`RespectSystemScaling_Tooltip`). |
| enabled / disabled (the state words) | 有効 / 無効 | Core Keyed `Enabled`, `Disabled`; the siblings' booleans say On / Off, Medic's state words are its own keys. |
| These settings are unavailable… built-in values. | これらの設定は使用できません.別のMODがメディック・ウォーキャスケットを変更しているため,セットは本来の値を使用します. | Shipcracker `SCWC_SettingsUnavailable`; "definitions" dropped as modder vocabulary (定義 appears once in vanilla). |
| cooldown / line of sight / cells | クールダウン / 視線が通ることが必要です / 単位はタイル | Upstream rows; Core Keyed `AbilityRequiresLOS`. |
| The time the pilot must wait after X before Y. | 〜した後,パイロットが次の〜を始められるようになるまでの待ち時間です. | Shipcracker `SCWC_CooldownDesc`. |
| The amount … holds when full. | 満タン時に〜に入る〜の数です. | Shipcracker `SCWC_TankCapacityDesc`. |
| Workshop frames | …[/url]に,宇宙世紀の新型ウォーキャスケットを追加します. / 概要 / セット構成 / 互換性 / よくある質問 / リンク / `[b]胴体:[/b]` `[b]ショルダーアーマー:[/b]` `[b]ヘルメット:[/b]` / 既存のセーブデータに追加しても安全です. / ポーンがセットに封入されている間は,MODを削除しないでください. / CE head アーマー値,かさ,積載量が他の宇宙世紀のウォーキャスケットと同水準に調整され / VGE head ウォーキャスケット向けの真空…ルールに従います / SOS2 line / VFE Pirates本体のセットとも自由に組み合わせられます. / Q: どうやって入手しますか? A: …研究「宇宙世紀のウォーキャスケット」を完了したら,ほかのセットと同じくウォーキャスケット鋳造台で溶接します. / Q: バランスが崩れていませんか? / Mod 設定 / links line | Shipcracker Workshop (opener, compat lines, FAQ) and Toxin Workshop (links line). |
| keeps X at full | パイロットの〜を最大に保ちます | Shipcracker Workshop (肺機能を常に最大に保ち). |

## Charge nouns

| `chargeNoun` | Japanese | Forms and lookup |
|---|---|---|
| kit (the armor's `CompDressingSuite`) | キット | One form. Japanese falls to the base `Pluralize` (lookup, else unchanged) and ja Keyed never switches on `CHARGENOUN_gender`, so no `WordInfo` file is shipped. Read aloud in every slot: `Stat_Thing_ReloadChargesRemaining_Name` キット残数, `Stat_Thing_ReloadMaxCharges_Name` 最大キット数, `Stat_Thing_ReloadPerCharge_Name` キット回復コスト, `_Desc` キットを1回分回復するために…, `Stat_Thing_ReloadRefill_Name` キットフル充填コスト, `Stat_Thing_ReloadDestroyOnEmpty_Desc` 最後のキットを使用すると…, `CommandReload_NoCharges` キットが残っていません. `ChargesRemaining` is Core's fixed 残りの使用可能数 and carries no noun. The mod's own strings count it with 個 and call it 薬のキット where the medicine matters. |
| shot (the shoulders' `CompApparelReloadable`) | 注射 | One form, no lookup file, as above. Reads in every slot: 注射残数, 最大注射数, 注射回復コスト, 注射を1回分回復する, 最後の注射を使用すると, 注射が残っていません. Named alike in the mod's Keyed (注射: {0} / {1}, 注射が充填されていません., 注射容量), counted with 回分. Chosen over 投与, which reads as a dosing act rather than a held dose. |

## Grounded in the first pass

| English | Japanese | Source |
|---|---|---|
| herbal medicine / medicine / glitterworld medicine | 薬草 / 医薬品 / 最先端医薬品 | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` labels (arrive at runtime; named in no string). |
| go-juice / wake-up | ゴージュース / ウェイクアップ | Core `GoJuice.label`, `WakeUp.label`. |
| heart attack | 心筋梗塞 | Core `HeartAttack.label`. |
| infection / disease / illness | 感染症 / 病気 / 病気 | Core `WoundInfection.label` (傷からの感染症) and its description; 病気 per Core `Disease_*` letter texts (病気に罹りました). |
| missing body part | 身体欠損 | Core `MissingBodyPart.labelNoun`; the label itself is a past-tense phrase (身体の一部分を失った). |
| wound | 傷 | Core `WoundInfection.description` (傷口への細菌感染). |
| tend (the action) / tend quality / tend speed | 看護 / 看護品質 / 看護速度 | Core Keyed `Tend` ({0}を看護する), `WorkTagCaring`, `MedicalTendQuality.label` (看護・品質), `MedicalTendSpeed.label` (看護・速度); Keyed `TendQuality` says 看病品質, the stat labels' 看護 is kept throughout. |
| has nothing to tend (invalid target) | {0}には看護の必要がありません. | Core Keyed `TendingNotRequired` ({0}には看護の必要がない), in polite form. |
| has no wounds to tend | {0}には看護できる傷がありません. | Shaped from Core `AbilityCannotCastNoHealableInjury` ({PAWN_labelShort}には治療可能な怪我がありません.). |
| treat (disease and infection) | 治療 | Core `Doctor.verb`, `Doctor.description` (病を治療し). |
| medical skill | 医術のスキル | Core `Medicine.skillLabel` (医術), `AllowBookMedicine.description` (医術のスキル). |
| doctor work / caring work | 治療の作業 / 看護の作業 | Core `Doctor.gerundLabel` (治療) and Keyed `WorkTagCaring` (看護). The `{0}` of `MDWC_WorkSummary` is `Doctor.labelShort` (医者), so it reads 医者の作業. |
| tending, surgery, feeding, rescue (Workshop) | 看護,手術,食事の世話,救助 | Core `TendPatient.reportString` (看病中), `Surgery` prose (手術), `Doctor.description` (食事を与え), `Rescue.label`. |
| Work tab | 「優先順位」タブ | Core `WorkTab.helpText` (「優先順位」タブから); MainButtonDef `Work.label` is 優先順位. |
| manipulation / sight | 指の機能 / 視覚 | Core `Manipulation.label`, `Sight.label`. |
| pain / pain shock | 痛み / 痛みでショック状態 | Core Keyed `Pain`, `PainShock` (痛みでショック); `PainShockThreshold.description` (痛みによって失神する). |
| downed | 倒れた | Core `Wait_Downed.reportString` (倒れている); `Downed.label` is 失神, which reads as fainting. |
| is hostile | {0}は敵対しています. | Core Keyed `Hostile` (敵対). |
| power armor (lore) | パワーアーマー | Core `Fabrication.description`, upstream 10th-generation paragraph; the item label is マリーンアーマー. |
| plating / shields | 装甲 / シールド | Core `Apparel_PowerArmorHelmet.description` (積層装甲板), `Apparel_ShieldBelt.label` (シールドベルト). |
| pirate (lore) | 宙賊 | Core `Pirate.label`, upstream row. |
| Mote (the three motes' inherited label) | 塵 | Core `Mote_*` labels, 64 of 64. |
| trait description tokens | `{PAWN_nameDef}は`, `{PAWN_pronoun}が`, `{PAWN_objective}` bare | Core trait descriptions (`FastLearner`, `Ascetic`, `AnnoyingVoice`): the pronoun (彼 / 彼女) takes a particle, and `{PAWN_objective}` already carries を (Grammar `ProhimObj` 彼を), so it takes none. |
| reportString shape | TargetAの傷を処置中 / TargetAにアドレナリンを注射中 | Core `TendPatient` (TargetAの看病中), `FeedPatient` (TargetAをTargetBに給仕中): progressive 〜中, no period. The abilities' `jobReportString`s read メカナイト処置中 / アドレナリンを注射中. |
| hediff description opener | 血中にアドレナリンが浸透しています. | Core `GoJuiceHigh.description` (血中にゴージュースが浸透しています.). |
| suppression (Workshop, CE) | 制圧されにくくなります | Coined; CE ships no Japanese. |
| vacuum resistance (Workshop, VGE) | 真空耐性 | Odyssey `VacuumResistance_Partial.label`. |

## Pending native review

A fresh register read rewrote the calques it found (頭が鈍る became vanilla's
頭がぼんやり, the pawn tokens were kept where the checker requires them). The
range unit 単位はタイル follows the family caption; vanilla's def stats measure
map distance in セル (11 hits against 2 for タイル, which is also the world
tile), so a native ruling may move the family to セル.


Every coinage in "Set-specific rows": メディック as the set prefix, メカナイト処置 /
処置する, メカナイト処置装置, 運搬用メカナイト, アドレナリン注射器, the stage labels
高揚 / 安定 / 減退 / 反動, 診断バイザー, 医療補助モジュール, 野戦医療用の補助アーム,
政府の計画, 薬 as generic medicine, 怪我の深刻度 and キットあたりの深刻度, エフェクトの色,
傷1箇所あたりの時間, 効果の持続時間, 反動の持続時間, 開封済みのキット, 照射途絶, 自動使用,
有効な作業, 射程 over the family's 範囲, both charge nouns, ウォーキャスケット散弾銃,
ウォーキャスケット一式, 登場時のレター and the neutral letter's 不思議な黒衣の不審者 (不審者 after
不思議な may read doubled). All free prose: the
three descriptions (paragraph 1 after the family opener, paragraphs 2 and 3),
the two ability descriptions, the hediff and trait descriptions, every settings
tooltip and summary, and the whole Workshop page. The pass re-read every
sentence against the language file's tells and reflowed the calques it found
(a comma-split relative clause, a double を, a dangling object in the toggle
tooltips). The VFEP terms' review is tracked upstream, under the section named
above.
