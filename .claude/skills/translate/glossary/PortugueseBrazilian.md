# Brazilian Portuguese glossary: Medic Warcasket

First machine-assisted pass 2026-11-05, pending native review. Only
mod-specific coinages and grounding decisions. The VFEP warcasket vocabulary
(warcasket, the parts, the label pattern, foundry, entomb, VFEP's set names
and research labels, the spacer-tier opener) lives upstream in
`l10n/languages/PortugueseBrazilian.md` under "Grounded common vocabulary,
VFE Pirates warcasket domain", with the grounding source (RafaelNaymaier's
"Tradução Vanilla Expanded PT-BR" pack, GitHub
`RafaelNaymaier/Traducao-Vanilla-Expanded-PT-BR`, Core winning over it:
plastiaço over the pack's plastoaço). Only the spacer opener is the family's:
Medic's descriptions carry their own paragraphs 2 and 3 (the program lore and
the set paragraph), shared byte-identically across the three parts and
recorded below. The settings and ability words the family settled
(spacer-tech, plasteel, Off/On, cooldown, Odyssey, the `cooldownGerund`) sit
in that file's main table. Nothing here restates them; a VFEP term is changed
there first.

## Set-specific rows

| English | Brazilian Portuguese | Grounding |
|---|---|---|
| Medic (set name) | médica, adjective agreeing with carcaça (f.): `carcaça de guerra médica`, `ombreiras de carcaça de guerra médica`, `capacete de carcaça de guerra médica`; in prose `a série Médica`, `a Médica` | Coined. Follows the pack's adjectival set names (`catafrática`, `aérea`). |
| Workshop title / `MDWC_SettingsCategory` | Carcaça de Guerra Médica | Contains the pack's "carcaça de guerra"; title coupling. |
| trait label `warcasket` | carcaça de guerra | Verbatim the pack's `VFEP_WarcasketTrait.degreeDatas.warcasket.label`. |
| trait description | `{PAWN_nameDef} está integrado(a) em uma carcaça de guerra. A dor não tem efeito sobre o corpo {PAWN_objective}, e {PAWN_pronoun} nunca entra em choque ... por causa disso` (not `dela`, which collides with a female `{PAWN_objective}`) | Pack's trait text for the frame, its singular-they `eles` replaced by the pawn tokens. `{PAWN_objective}` is dele/dela here (Core Grammar `ProhimObj`), so it sits after a `de` frame, as in Core `Beauty.degreeDatas.pretty.description`; participle hedged with a literal `(a)` like Core's trait descriptions (none uses `PAWN_gender`). |
| mechanite dressing (ability, section header) | curativo de mecanitos (m.) | "curativo" from Core `Nurse61.description` ("mudança curativos"); "mecanitos" from Core `FibrousMechanites.label` "mecanitos fibrosos". |
| dress (verb) | fazer curativo / aplicar um curativo | Coined around the noun. |
| mechanite dressing suite (armor) | sistema de curativo de mecanitos | Coined; "conjunto" is taken by "set". |
| carrier mechanites | mecanitos transportadores | Coined. |
| kit (settings and runtime) | kit (m.), `kits de {1}`, `Capacidade de kits`, `Gravidade por kit` | Core `MedicineIndustrial.description` "Um kit de equipamentos médicos". |
| diagnostic visor | viseira de diagnóstico | Coined. |
| medical assistance module | módulo de assistência médica | Shipcracker's "módulos de assistência corpo a corpo" for "melee assistor modules". |
| field-medicine manipulators | manipuladores de medicina de campo | Coined. |
| adrenaline shot (ability, act) | injeção de adrenalina (f.); the act in prose is `injeção`, the stored dose is `cartucho` | Coined. Hediff `labelNoun` `uma injeção de adrenalina`. |
| adrenaline injector | injetor de adrenalina | Coined. |
| adrenaline (hediff) | adrenalina | Medical term. |
| stage labels rush / steady / wearing off / crash | pico / estável / em declínio / esgotamento | Coined, lowercase and short like Core's `HeartAttack.stages.*` ("doloroso", "debilitante"). "esgotamento" also carries "crash" in the settings (`Duração do esgotamento`) and prose. |
| boost duration | Duração do estímulo | Coined. |
| field surgeons | cirurgiões de campo | Coined. |
| squadmate | companheiro de esquadrão | "esquadrão" from Core backstories (`SpaceMarine19`). |
| raider / allied medics | médicos invasores e aliados | "invasores" as Toxin ships it; Core uses "invasores" 16 times. |
| their own downed | os próprios companheiros abatidos | Core `Downed.label` "abatido". |
| autocast | uso automático | Coined; VEF ships English only and Core has no autocast key ("automático" as in Core `Autodoors.label`). |
| effect color / Blue / Green / Pink | Cor do efeito / Azul / Verde / Rosa | Core Keyed `Blue` "azul", `Green` "verde"; "rosa" from Core `Hair_Pink.label` "cabelo rosa". Capitalised as dropdown options, like the English. |
| medicine gauge | medidor de medicamento | "medidor" as Toxin ships it for its gauge. |
| Reload item / Reload target / Never reload | Item de recarga / Meta de recarga / Nunca recarregar. | Coined around Core's "recarga" (`Stat_Thing_ReloadPerCharge_Name`). |
| Interrupted / Stream broken / Out of {0} | Interrompido / Fluxo rompido / Sem {0} | Coined. |
| Enabled work types | Tipos de trabalho ativados | Core Keyed `Enabled` "Ativado(s)". |
| `{0}` work (WorkSummary) | `Trabalhos que o piloto pode fazer: {0}.` | `{0}` is the Doctor/Childcare `labelShort` list (Core "medicina", Biotech "cuidador(a)"), which no "trabalhos de {0}" frame survives, so the colon frame. |
| caring work | trabalho de cuidados | Core Keyed `WorkTagCaring` "cuidar". |
| warcasket trait (settings prose) | o traço "carcaça de guerra" | Core Keyed `Traits` "Traços"; quoted as the research labels are. |
| suppressability (CE) | suscetibilidade à supressão | Coined; CE 16.7.4 ships no pt-BR `Suppressability.label`, its pt-BR Keyed uses "Suprimido!". |
| Default (toggle default word) | ativado / desativado (`MDWC_Enabled`, `MDWC_Disabled`) | Core Keyed `Enabled` / `Disabled`, singular. |
| Description paragraph 2 (program lore) | `É um design de 10ª geração, saído do programa do governo que reconstruiu equipamentos piratas capturados em torno das próprias armaduras energizadas ... criou a série Médica para os cirurgiões de campo que tinham servido ao lado desses veteranos.` | Own text; "equipamentos piratas capturados", "armaduras energizadas", "veteranos" from the pack's 10th-generation paragraph. Full text in `DefInjected/ThingDef/Apparel_Medic.xml`. |
| Description paragraph 3 (set paragraph) | `A armadura leva um sistema de curativo de mecanitos ... o lugar da Médica é logo atrás da linha de frente, e não nela.` | Own text, split into three sentences; "on the spot" is `em pleno campo de batalha` (the register read dropped `ali mesmo`, which had nothing to point back to). |

## Stranger in black and heart attack toggle

| English | Brazilian Portuguese | Grounding |
|---|---|---|
| stranger / woman / man / strangers in black (`MDWC_StrangerInBlack` labels) | estranho de preto / mulher de preto / homem de preto / estranhos de preto | Verbatim Core `PawnKindDef` `StrangerInBlack.label`, `labelFemale`, `labelMale`, `labelPlural`; the def is a copy of vanilla's, so its labels must match. |
| Man in black (`MDWC_SectionStranger`) / the stranger in prose | Estranho de preto / o estranho | Core `IncidentDef` `StrangerInBlackJoin.label` and `letterLabel` "estranho de preto" (vanilla pt-BR never says "homem de preto" for the incident); the Workshop section header "Estranho de preto". |
| duster / revolver | gabardina / revólver (`vestindo uma gabardina e armado com um revólver`) | Core `Apparel_Duster.label`, `Gun_Revolver.label`. |
| warcasket slugthrower | lança-balotes de carcaça de guerra (m.) | The community VFEP pack's `VFEP_WarcasketGun_Slugthrower.label`. |
| close to falling | à beira do colapso | Own wording. |
| forced gender / no forced gender | gênero fixo / sem gênero fixo (`Estranho sem gênero fixo`, `O estranho não tem gênero fixo.`) | Core Keyed `Gender` "Gênero"; "fixo" coined, "forçado" rejected as a calque. |
| letter (the incident's) | carta (`a carta de chegada`) | Core Keyed `MoreLetters` "Você possui mais {0} cartas". |
| Stranger in black (`MDWC_StrangerLetterLabel`) | `{PAWN_gender ? Estranho : Estranha} de preto` | Vanilla's letterLabel "estranho de preto", capitalised like the majority of Core's letter labels, gender resolved by the pawn (always known; Core Keyed uses `{PAWN_gender ? um guerreiro : uma guerreira}`-style multi-word splits). |
| the letter text (`MDWC_StrangerLetterText`) | `Problema detectado, chegou {PAWN_gender ? um misterioso estranho : uma misteriosa estranha} de preto.\n\nSerá que [PAWN_pronoun] conseguirá consertar as coisas?` | Vanilla's `StrangerInBlackJoin.letterText` verbatim, with its fixed masculine noun phrase split by `PAWN_gender`. The game formats it with `Formatted(pawn.Named("PAWN")).AdjustedFor(pawn)` (`IncidentWorker_WandererJoin`, decompiled), so both token styles resolve. Gender-resolved rather than neutral, since Portuguese has no neutral form here, so `MDWC_StrangerAnyGenderDesc` says the letter "se ajustará ao gênero de quem chegar" instead of the English "neutral wording". |
| warcasket kit (the stranger summaries) | conjunto de carcaça de guerra (`um conjunto preto de carcaça de guerra`) | "conjunto" is this file's word for the set, so it cannot read as the medicine `kit` charge noun. |
| black medic warcasket | carcaça de guerra médica pintada de preto | Coined; "pintada de preto" avoids stacking a second adjective on "carcaça de guerra médica". |
| Shot stops a {0} (heart attack toggle) | `Injeção interrompe um {0}`; tooltip `interromperá um {1} em vez de dar o estímulo` | `{0}`/`{1}` is Core `HeartAttack.label` "ataque cardíaco" (m.), so the masculine article is safe; "interrompe" matches `MDWC_ShotStopsHeartAttack`, "estímulo" the boost row. |

## Ported from the siblings

| English | Brazilian Portuguese | Source |
|---|---|---|
| Default: {0}. / suffix (default) | Padrão: {0}. / ` (padrão)` | Toxin and Shipcracker Keyed. |
| SettingsUnavailable | Estas configurações estão indisponíveis. Outro mod alterou a carcaça de guerra médica, então o conjunto mantém seus valores originais. | Shipcracker's frame; the English "definitions" is not carried, as Shipcracker's is not. |
| cooldown / range / cells | Tempo de recarga / Alcance / células | Shipcracker and Toxin Keyed. |
| line of sight | linha de visão (`Exige linha de visão.`) | Shipcracker; Core `AbilityRequiresLOS`. |
| The time the pilot must wait after X before Y | Quanto tempo o piloto precisa esperar depois de X para Y | Shipcracker `SCWC_CooldownDesc`. |
| holds when full | comporta quando cheio(a) | Shipcracker `SCWC_TankCapacityDesc`. |
| Workshop headers and bullets | Sobre / O conjunto / Compatibilidade / Perguntas frequentes / Links; `[b]Tronco:[/b]` / `[b]Ombreiras:[/b]` / `[b]Capacete:[/b]`; `[b]P: ...[/b]` / `R: ...` | Both siblings. |
| compat openers, SOS2 line, CE armor line, mix and match | verbatim Toxin and Shipcracker | Both siblings; the CE line drops the sibling helmet-smoke clause, the VGE line uses Odyssey `VacuumResistance.label` "Resistência ao Vácuo" ("regras de resistência ao vácuo dele"). |
| Is this imbalanced? | Isso não está desbalanceado? | Shipcracker FAQ. |
| mod settings | configurações do mod | Shipcracker FAQ. |
| Source code ... issue tracker | Código-fonte, downloads manuais e rastreador de problemas: | Toxin Workshop. |

## Charge nouns

Vanilla prints a `chargeNoun` only through `{CHARGENOUN}` and
`{CHARGENOUN_plural}` in pt-BR's reload strings (no `{lookup: ...}` and no
`CHARGENOUN_gender` anywhere in the tar's Keyed), and its frames hard-code
the masculine: `recarregar um {CHARGENOUN}`, `seu último {CHARGENOUN}`
(`Stat_Thing_ReloadPerCharge_Desc`, `Stat_Thing_ReloadDestroyOnEmpty_Desc`).
Portuguese runs the base `Pluralize` (lookup, else the word unchanged), so both
nouns ship a line in `1.6/Languages/PortugueseBrazilian/WordInfo/plural.txt`.
No `WordInfo/Gender` file: nothing reads it here.

| English | Singular / plural | Gender | Read-aloud |
|---|---|---|---|
| kit (armor, `CompDressingSuite.chargeNoun`) | kit / kits (`kit;kits`) | m. | `Kits restantes`, `Custo de recarga por kit`, `recarregar um kit`, `seu último kit`, `Sem kits restantes` |
| shot (shoulders, `CompApparelReloadable.chargeNoun`) | cartucho / cartuchos (`cartucho;cartuchos`) | m. | `Cartuchos restantes`, `Custo de recarga por cartucho`, `recarregar um cartucho`, `seu último cartucho`, `Sem cartuchos restantes` |

"shot" is `cartucho`, the stored dose, because the natural `dose` and
`injeção` are feminine and would read "um dose" in the masculine frames; the
mod's own Keyed counts name it alike (`Nenhum cartucho carregado.`,
`Cartuchos: {0} / {1}`, `Capacidade de cartuchos`, `um por cartucho`), while
the act of giving one stays `injeção` (`entre uma injeção e outra`).

## Grounded in the first pass

| English | Brazilian Portuguese | Source |
|---|---|---|
| herbal / industrial / glitterworld medicine | medicina natural / medicina / medicina de glitterworld | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` `.label` |
| medicine (the category, prose) | medicamento | Core `ThingCategoryDef` `Medicine.label` "medicamento" |
| go-juice / wake-up | suco-turbo / despertar | Core `GoJuice.label`, `WakeUp.label` |
| heart attack | ataque cardíaco | Core `HeartAttack.label` |
| infection / disease | infecção / doença | Core `WoundInfection.label`; "doença" throughout Core |
| missing body part | parte ausente do corpo | Core `MissingBodyPart.label` |
| mechanites | mecanitos (m.) | Core `FibrousMechanites.label`, `SensoryMechanites.label` |
| wound / injury | ferida / ferimento | Core `Medicine.description` "Cuida de feridas e doenças"; `UntendedInjury` "ferimento não tratado" |
| severity | gravidade | Core Keyed `ConfigurableSeverity` "Gravidade: {0}" |
| tend / tend quality | tratar / qualidade do tratamento | Core `Doctor.verb` "Tratar de", Keyed `TendQuality` "Qualidade do tratamento" |
| medical tend speed | velocidade de cuidado médico | Core `MedicalTendSpeed.label` |
| doctor work (`labelShort`) | medicina | Core `Doctor.labelShort` |
| Work tab | aba de Trabalho | Core `Work.label` "Trabalho", `WorkTab.helpText` "aba de TRABALHO" |
| pain shock | choque (`entra em choque`) | Core Keyed `PainShock` "Em choque" |
| manipulation / sight | manipulação / visão | Core `Manipulation.label`, `Sight.label` |
| downed | abatido | Core `Downed.label` |
| patient | paciente | Core prose |
| casting time | Tempo de invocação | Core Keyed `AbilityCastingTime` |
| Mote (the three mote labels) | Partícula | Core `Mote_*.label` |
| toggle frame | Quando ativado, ... Quando desativado, ... | Core Keyed `VisibleMoodDesc`, `SmoothCameraJumpsDesc`, `CommandAllowSowDesc` (vanilla's majority frame; Shipcracker's `Se ativado` is the minority one) |
| reportString shape | lowercase gerund with the period, `aplicando ... em TargetA.` | Core `Rescue.reportString` "resgatando TargetA."; Core's `TendPatient`/`FeedPatient` not copied |
| vacuum resistance | resistência ao vácuo | Odyssey `VacuumResistance.label` |

## Pending native review

Every coinage in "Set-specific rows": the set name `médica`, `curativo de
mecanitos` and the verb `fazer curativo`, `sistema de curativo de mecanitos`,
`mecanitos transportadores`, `viseira de diagnóstico`, `manipuladores de
medicina de campo`, `injeção de adrenalina` / `injetor de adrenalina`, the
charge noun `cartucho`, the stage labels `pico` / `estável` / `em declínio` /
`esgotamento`, `Duração do estímulo`, `uso automático`, `Meta de recarga`,
`Fluxo rompido`, `companheiro de esquadrão`, `cirurgiões de campo`,
`suscetibilidade à supressão` and the `Trabalhos que o piloto pode fazer:`
frame. From the stranger pass: `gênero fixo`, `à beira do colapso`,
`conjunto preto de carcaça de guerra`, `pintada de preto` and the
gender-split letter (`uma misteriosa estranha de preto` for a woman). All free prose: the three descriptions (the openers and paragraphs 2
and 3), the two ability descriptions, the hediff and trait descriptions, the
settings tooltips and the whole Workshop page, after a fresh register read
rewrote the calques it found (`reanimado` is Anomaly's shambler word, so a
revived raider is `um invasor que voltar a ficar de pé`).
