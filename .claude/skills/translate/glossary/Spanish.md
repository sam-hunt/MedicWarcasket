# Spanish (Castellano) glossary, Medic Warcasket

First machine-assisted pass 2026-11-05. Only mod-specific coinages and
grounding decisions. The VFEP warcasket vocabulary (warcasket, the parts, the
label pattern, foundry, entomb, VFEP's set names and research labels, the
spacer-tier opener) lives upstream in `l10n/languages/Spanish.md` under
"Grounded common vocabulary, VFE Pirates warcasket domain", with the
grounding source (the community Castilian-leaning VFE Pirates translation by
Cito2310, GitHub `Cito2310/translate-project-rimworld`; Castilian `Spanish`
only, never `SpanishLatin`) and what is pending native review; the settings
and ability words the family settled (Off/Default, cooldown, cells, the VEF
`jobReportString` frame and the `cooldownGerund`) sit in that file's main
table. Medic's descriptions carry their own paragraphs 2 and 3; only the
10th-generation opener (`Un armazón especializado de ataúd de guerra de
tecnología espacial con una protección estándar`, and its `Unas hombreras
especializadas ...` and `Un casco especializado ...` forms) is the family's.
Nothing here restates the upstream rows; a VFEP term is changed there first.

## Set-specific rows

| English | Spanish | Grounding |
|---|---|---|
| medic warcasket / shoulders / helmet | ataúd de guerra médico / hombreras médico de ataúd de guerra / casco médico de ataúd de guerra | Upstream label pattern, set name invariable in apposition (`hombreras bruto`, Toxin's `hombreras toxina`); medic = médico per Core backstories `CombatMedic82.title` (médico de combate), `MedicSoldier54.title`, `SpaceMarineMedic10`. |
| Medic (the series, in prose) | el Médico, un Médico | Capitalised as the upstream set names are in prose (`La serie Bruto`). |
| Workshop title / `MDWC_SettingsCategory` | Ataúd de guerra médico | Contains `ataúd de guerra`; the two are coupled. |
| warcasket (the trait label) | ataúd de guerra | The bare upstream noun. Cited in settings as `el rasgo "ataúd de guerra"` per Core Keyed `TraitLabelWithDesc` `rasgo {TRAITLABEL}` and the ASCII-quote rule. |
| mechanite dressing (ability, section header) | vendaje de mecanitas (m.) | Coined. Mecanitas per Core `FibrousMechanites` = mecanitas fibrosos, `SensoryMechanites`, and `SoldierExperiment50.description` "inyecciones de mecanitas"; vendaje per Core `labelTendedWell` = vendado/vendada and `Nurse61.description` "cambiar vendajes". Prose treats mecanitas as feminine (`mecanitas portadoras`), as in Core `FibrousMechanites.description` (`mecanitas aumentadoras`). |
| dress (verb) | vendar (`vendarán a sus compañeros`), `aplicar un vendaje de mecanitas` | Same grounding. |
| mechanite dressing suite (the armor's reloadable, in prose) | sistema de vendaje de mecanitas | Coined; "suite" is not a player-facing noun in Core es. |
| carrier mechanites | mecanitas portadoras | Coined. |
| the stream (floating text, ability, Workshop) | flujo (`Flujo cortado: ...`, `un flujo de mecanitas`), proyectar for the verb | Coined. |
| adrenaline / adrenaline shot (ability, hediff `labelNoun`, section) | adrenalina / inyección de adrenalina, `una inyección de adrenalina` | Core `KingOfPirates25` "subidón de adrenalina"; inyección per Core `ResurrectionSickness` "inyección de mecanitas". Hediff description follows Core's drug highs (`GoJuiceHigh`: "Neurozumo en el torrente sanguíneo."). |
| adrenaline injector | inyector de adrenalina | Coined; verb `Inyecta` per Core `GoJuice.ingestible.ingestCommandString` `Inyectar {0}`. |
| give a shot (report strings, AI toggle) | poner una inyección (`poniendo una inyección de adrenalina a TargetA.`) | Plain Castilian; no vanilla hit. |
| field-medicine manipulators | manipuladores de medicina de campaña | Coined. |
| diagnostic visor / medical assistance module | visor de diagnóstico / módulo de asistencia médica | Coined; módulo de asistencia per Shipcracker's `módulos de asistencia cuerpo a cuerpo`. |
| adrenaline stages: rush / steady / wearing off / crash | subidón / estable / desvaneciéndose / bajón | `subidón` per Core `GoJuiceHigh.label` (subidón de neurozumo) and `FlakeHigh`; `desvaneciéndose` is Core `Anesthetic.stages.wearing_off.label` verbatim; `estable` and `bajón` coined (no vanilla crash word; `bajón` is the plain Castilian word for the drop after a high). Both bajón and subidón carry the settings rows (`Duración del subidón`, `Duración del bajón`). |
| dull (crashing patient) | atontado | Core `Anesthetic.stages.woozy.label`. |
| injury severity | gravedad (`heridas con una gravedad total de {0}`, `Gravedad por kit`) | Core Keyed `Stat_Thing_Drug_AddictionSeverityPerDose_Desc` "Cuánta gravedad se añade a una adicción" (prose uses gravedad; `AddictionSeverityInitial` Severidad is the stat label). |
| open kit (gauge) | kit abierto (`Kit abierto: queda un {0}`) | Coined. |
| reload item (settings rows) | objeto de recarga | Coined; Core `Stat_Thing_ReloadChargesRemaining_Desc` "este objeto", `Reload` Recargar. |
| raider and allied medics / their downed | los médicos asaltantes y aliados / sus compañeros incapacitados; a revived raider rejoins the raid = `un asaltante reanimado volverá a unirse al asalto` | Core backstory titles `RaiderKing38` (rey asaltante), Keyed `Ally` Aliado, `Raid` Asalto, `DownedCannotCrawl` incapacitado, `MechSerumResurrector` (suero reanimador). |
| squadmate | compañero de escuadrón | Core backstories `Pilot63`, `SpaceMarine19` (escuadrón), `CombatMedic82` "mantener a sus compañeros vivos". |
| autocast | lanzamiento automático | Coined from Core `AbilityCastingTime` = Tiempo de lanzamiento; VEF ships English only, so the gizmo itself reads "autocast". |
| suppressability (CE, Workshop) | supresibilidad | CE 2890901044 es `Suppressability.label`. |
| work types / caring (settings) | tipos de trabajo, `Tipos de trabajo activados`, `El piloto puede hacer trabajos de {0}.` / cuidados | Core Keyed `WorkTabChangeAllColonists` "un tipo de trabajo en la pestaña "Trabajo"", `WorkTagCaring` cuidados; `{0}` is the `labelShort` list (médico), lowercase. |

## Ported from the siblings

| English | Spanish | From |
|---|---|---|
| These settings are unavailable ... | Estos ajustes no están disponibles. Otro mod ha modificado el ataúd de guerra médico, así que el conjunto mantiene sus valores originales. | Shipcracker `SCWC_SettingsUnavailable`; "definitions" dropped as modder vocabulary, "keeps" from Medic's English. |
| (default) / Default: {0}. / When enabled / When disabled / enabled / disabled | ` (por defecto)` / `Por defecto: {0}.` / `Si está activado, ...` / `Si está desactivado, ...` / activado / desactivado | Both siblings; Shipcracker `SCWC_VacuumBurnDesc`. |
| tooltip openers | `Tiempo que tarda ...`, `Tiempo que el piloto debe esperar tras ...`, `Número de ...`, `Distancia ...` | Toxin `TXWC_JetWarmupDesc`, Shipcracker `SCWC_CooldownDesc`, Toxin's noun-led openers. |
| range / cooldown | Alcance / Enfriamiento | Both siblings (`Alcance (casillas)`); see the pending note on Core's `Range` = Rango. |
| line of sight | línea de visión (`Requiere línea de visión.`) | Shipcracker; Core Keyed `AbilityRequiresLOS` "Línea de visión requerida". |
| gauge | indicador | Toxin `TXWC_TankCapacityDesc` "El indicador". |
| Workshop intro, headers, compat and FAQ frames | `Un nuevo ataúd de guerra de la era espacial para [url=...]...[/url].`, `[h1]Acerca de[/h1]`, `[h2]El conjunto[/h2]`, `[h1]Compatibilidad con mods[/h1]`, `[h1]Preguntas frecuentes[/h1]`, `[h1]Enlaces[/h1]`, `P:` / `R:`, `[b]Torso:[/b]` / `[b]Hombreras:[/b]` / `[b]Casco:[/b]`, the remove-while-entombed bullet (the add bullet became `Se puede añadir sin riesgo a partidas existentes.` and the art credit `Arte de`, both on the register read; the siblings ship `Seguro de añadir` and `Arte por`), the SOS2 and VGE bullets, the CE armor clause, `Combina las piezas con las series de VFE Pirates como quieras.`, `¿Cómo lo consigo?` with its answer, `¿Está desequilibrado?`, the GitHub and art lines | Toxin and Shipcracker Workshop pages. |

## Charge nouns

| English | Spanish | Forms | Lookup files |
|---|---|---|---|
| kit (the armor's `CompDressingSuite.chargeNoun`) | kit (m.) | pl. kits | none: the worker's rule appends `s` after `t` (decompile-verified on the local 1.7 assembly) and a gender miss is masculine |
| shot (the shoulders' `CompApparelReloadable.chargeNoun`) | inyección (f.) | pl. inyecciones | `1.6/Languages/Spanish/WordInfo/plural.txt` (`inyección;inyecciones`: the rule would append `es` to the accented `ó` and ship `inyecciónes`), `WordInfo/Gender/Female.txt` (`inyección`) |

`kit` is Core's own word for a medicine pack (`MedicineIndustrial.description`
"Un kit de equipo médico", `Make_MedicineIndustrial` "Elabora un kit de
medicinas"). No masculine word for a shot reads naturally: `pinchazo` and
`chute` are colloquial, and `dosis` is feminine too and would need a line in
both files (`dosis;dosis`, since the rule appends `es` after `s`). `inyección`
also matches the ability name. Read aloud in Core's slots: `Kits restantes` /
`Inyecciones restantes`, `El número máximo de kits` / `de inyecciones`,
`recargar un kit` / `una inyección`, `Coste por kit` / `por inyección`, `su
último kit` / `su última inyección`, `No quedan kits.` / `No quedan
inyecciones.` `StatsReport_ReloadRemainingChargesMultipler` hard-codes `las
{CHARGENOUN_plural}` (a vanilla slip for `kits`; nothing to do). The mod's own
strings agree: `Kits: {0} / {1}`, `Inyecciones: {0} / {1}`, `Capacidad de
kits`, `Capacidad de inyecciones`.

## Grounded in the first pass

| English | Spanish | Source |
|---|---|---|
| herbal / industrial / glitterworld medicine | hierbas medicinales / medicina / medicina mundobrillante | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` `.label`; the herbal label is plural, so `{0}` sits in number-neutral frames (`No hay {0} en la armadura.`, `Sin {0}:`, `kits de {0}`, `Se recarga con {0}.`) |
| go-juice / wake-up | neurozumo / despertar | Core `GoJuice.label`, `WakeUp.label` |
| heart attack / infection / disease / missing body part | ataque cardíaco / infección / enfermedad / parte del cuerpo faltante | Core `HeartAttack.label`, `WoundInfection.label`, Core prose, `MissingBodyPart.label` |
| mechanites | mecanitas | Core `FibrousMechanites.label`, `SensoryMechanites.label` |
| tend / tend quality / tend speed / medical skill | tratar, atender / calidad del tratamiento / velocidad de tratamiento médico / habilidad de medicina | Core `TendPatient.reportString` `tratando a TargetA.`, Keyed `Tend` `Atender {0}`, `TendQuality`, `MedicalTendSpeed.label`, SkillDef `Medicine.label` medicina |
| has nothing to tend / no wounds | {0} no necesita atención. / heridas | Core Keyed `TendingNotRequired`, `UntendedInjury` herida sin atender |
| doctor (work) / Work tab | médico, `trabajar como médico` / la pestaña "Trabajo" | Core `Doctor.labelShort`, Keyed `NeedDoctorDesc` "asignado como médico", `WorkTabChangeAllColonists` |
| manipulation / sight / pain shock / unconscious | manipulación / vista / en shock por el dolor / inconsciente | Core `Manipulation.label`, `Sight.label`, Keyed `PainShock` "En shock por el dolor", `Unconscious` |
| shield / plasteel / spacer | escudo / plastiacero / era espacial | Core `Apparel_ShieldBelt`, `Plasteel.label`, Keyed `TechLevel_Spacer` |
| Mote | Mota | Core `Mote_*` labels |
| casting time / cooldown / line of sight | Tiempo de lanzamiento / Enfriamiento / línea de visión | Core Keyed `AbilityCastingTime`, `CooldownTime`, `AbilityRequiresLOS` |
| Blue / Green / Pink | Azul / Verde / Rosa | Core Keyed `Blue` azul, `Green` verde (lowercase there, capitalised as option buttons); Core has no `Pink`, rosa per `Hair_Pink.label` (pelo rosa) |
| patient / hostile | paciente / hostil | Core Keyed `PatientsAwaitingMedicalOperation`, `Hostile` |
| bloodstream (hediff description) | en el torrente sanguíneo | Core `GoJuiceHigh.description` and every drug high |
| trait pawn tokens | `{PAWN_objective}` is the clitic lo/la before the verb (`Ningún dolor {PAWN_objective} afecta`), `{PAWN_pronoun}` the subject él/ella (kept though Spanish would drop it: the checker requires every English token), `sepultad{PAWN_gender ? o : a}` | Core `Wimp` ("Incluso un poco de dolor {PAWN_objective} inmovilizará"), `Jealous`, `Masochist` (herid{PAWN_gender ? o : a}); Core Grammar `ProhimObj` lo, `Prohe` él |
| report strings | `aplicando un vendaje de mecanitas a TargetA.`, `poniendo una inyección de adrenalina a TargetA.`; the abilities' `jobReportString` the same without TargetA | Core `JobDef` reportStrings: lowercase gerund, trailing period, personal `a` before an animate target (`tratando a TargetA.`) |

## Pending native review

- Every coinage in "Set-specific rows": `vendaje de mecanitas` and `vendar`,
  `sistema de vendaje de mecanitas`, `mecanitas portadoras`, `flujo`,
  `inyector de adrenalina`, `manipuladores de medicina de campaña`, `visor de
  diagnóstico`, `módulo de asistencia médica`, the stage labels `estable` and
  `bajón`, `objeto de recarga`, `kit abierto`, `lanzamiento automático`, and
  `hombreras médico` (the invariable set name reads like a gender slip next to
  a feminine noun; it follows the family pattern on the labels, and prose says
  `las hombreras del Médico`), `niveles` for the adrenaline's levels (vanilla
  stages are `fases`).
- `Alcance` for the settings row while Core's ability tooltip shows Keyed
  `Range` = `Rango`; kept for the family (both siblings ship `Alcance`).
- The charge noun `inyección` in the reload slots.
- All free prose: the three descriptions' first paragraphs and the shared
  paragraphs 2 and 3, both ability descriptions, the hediff and trait
  descriptions, every settings tooltip and summary, and the whole Workshop
  page. A fresh register read rewrote the calques it found.
