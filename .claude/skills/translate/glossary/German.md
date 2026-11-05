# German glossary, Medic Warcasket

First machine-assisted pass 2026-11-05. Only mod-specific coinages and
grounding decisions. The VFEP warcasket vocabulary (warcasket, the parts, the
label pattern, foundry, entomb, VFEP's set names and research labels, the
spacer-tier opener) lives upstream in `l10n/languages/German.md` under
"Grounded common vocabulary, VFE Pirates warcasket domain", with the
grounding source (no community VFEP German pack) and what is pending native
review; the settings and ability words the family settled (spacer-tech,
Off/On, When enabled, cooldown, cells, the VEF `jobReportString` frame and the
`cooldownGerund`) sit in that file's main table. Medic's descriptions carry
their own paragraphs 2 and 3; only the 10th-generation opener (`Eine
spezialisierte Kriegssarg-Hülle aus der Raumfahrttechnik mit Standardschutz.`
and its shoulders and helmet forms) is the family's. Nothing here restates
the upstream rows; a VFEP term is changed there first.

## Set-specific rows

| English | German | Grounding |
|---|---|---|
| medic warcasket / shoulders / helmet | Sanitäter-Kriegssarg / Sanitäter-Kriegssarg-Schulterpanzer / Sanitäter-Kriegssarg-Helm | Upstream label pattern; medic = Sanitäter per Core backstories `Paramedic45.titleShort`, `SpaceMarineMedic10.title` (Space-Marine-Sanitäter), `VatgrownMedic53`. |
| Medic (the series, in prose) | der Sanitäter | Same grounding; masculine, so the article matches Kriegssarg. |
| Workshop title / `MDWC_SettingsCategory` | Sanitäter-Kriegssarg | Contains Kriegssarg; the two are coupled. |
| warcasket (the trait label) | Kriegssarg | The bare upstream noun (no community pack to mirror). Cited in settings as `Merkmal 'Kriegssarg'` per Core Keyed `TraitLabelWithDesc` `Merkmal: {TRAITLABEL}` and the single-quote rule. |
| mechanite dressing (ability, section header) | Mechanitenverband (m.) | Coined. Mechaniten per Core `FibrousMechanites` = Fasermechaniten, `SensoryMechanites` = Sensorikmechaniten; Verband per Core `labelTendedWell` = bandagiert and `Nurse61` "Verbände wechseln". |
| dress (verb) / dressing (the act) | verbinden, einen Mechanitenverband anlegen; der Verband | Same grounding; the register read unified `ansetzen` onto `anlegen`, the reportStrings' verb. |
| mechanite dressing suite (the armor's reloadable, in prose) | System für Mechanitenverbände | Coined; phrased out instead of the hyphenated compound `Mechaniten-Verbandssystem` (hyphen compounds already run 6x vanilla's rate from the label pattern alone). |
| carrier mechanites | Trägermechaniten | Coined, closed compound like Core's Fasermechaniten. |
| the stream (floating text) | Mechanitenstrom | Coined; `strömen lassen` for "stream" in prose. |
| adrenaline / adrenaline shot (ability, hediff `labelNoun`, section) | Adrenalin / Adrenalinspritze, `eine Adrenalinspritze` | Standard German medical usage; no vanilla hit. Hediff label and description follow Core's drug highs (`GoJuiceHigh`: "Go-Juice im Blutkreislauf."). |
| adrenaline injector | Adrenalininjektor | Coined. Core `GoJuice.ingestible.ingestCommandString` `Injiziere {0}` for the verb. |
| field-medicine manipulators | feldmedizinische Manipulatoren | Coined. |
| diagnostic visor / medical assistance module | Diagnosevisier / medizinisches Assistenzmodul | Coined; Visier per Odyssey `ImprovedSights.label`, Assistenzmodul per Shipcracker's `Nahkampf-Assistenzmodule`. |
| adrenaline stages: rush / steady / wearing off / crash | Schub / anhaltend / klingt ab / Tief | `klingt ab` is Core `Anesthetic.stages.wearing_off.label` verbatim; vanilla mixes nouns and adjectives in one hediff (`AlcoholHigh`: angeheitert, betrunken, Filmriss), so Schub (from Adrenalinschub) and Tief are capitalised nouns. Tief also carries the crash in the settings (`Dauer des Tiefs`) and prose. |
| boost duration | Wirkdauer | Coined. |
| dull (crashing patient) | benommen | Core `Anesthetic.stages.woozy.label`. |
| injury severity | Verletzungsschweregrad; `Verletzungen mit einem Schweregrad von {0}` before a number | Core `AddictionSeverityInitial` = Suchtschweregrad zu Beginn; Verletzung per Keyed `UntendedInjury`. |
| open kit (gauge) | angebrochenes Medizinkit | Coined. |
| reload item (settings rows) | Nachladegegenstand | Coined; Core `Stat_Thing_ReloadRefill_Desc` "Gegenständen ... zum vollständigen Nachladen". |
| raider and allied medics / their downed | die Sanitäter der Raider und Verbündeten, Raider-Sanitäter / ihre eigenen Leute, die am Boden liegen | Core Keyed `Difficulty_RaidLootPointsFactor_Label` Raider-Inventar, `Ally` Verbündet, `DownedLower` liegt am Boden, `Raid` Überfall. |
| squadmate | Kamerad | Core backstories (`[PAWN_possessive]e Kameraden`). |
| autocast | automatisches Wirken | Coined from Core `AbilityCastingTime` = Wirkzeit; VEF ships English only, so the gizmo itself reads "autocasting". |
| hostile target | {0} ist feindlich. | Core Keyed `Hostile` = Feindlich. |
| work types / caring (settings) | Arbeitstyp, `Aktivierte Arbeitstypen`, `Der Pilot kann diese Arbeitstypen übernehmen: {0}.` / Fürsorge | Core Keyed `NeedDoctorDesc` "Weise einem Kolonisten den Arbeitstyp 'Arzt' zu", `IncapableOfTooltipWorkTypes` Deaktivierte Arbeiten, `WorkTagCaring` Fürsorge; `{0}` is the `labelShort` list (Arzt), so it sits after a colon. |
| suppressability (CE, Workshop) | Anfälligkeit für Unterdrückung | Coined; CE ships no German. |

## Ported from the siblings

| English | German | From |
|---|---|---|
| These settings are unavailable ... | Diese Einstellungen sind nicht verfügbar. Eine andere Mod hat den Sanitäter-Kriegssarg verändert, daher behält das Set seine eingebauten Werte. | Shipcracker `SCWC_SettingsUnavailable`; "definitions" dropped as modder vocabulary (`Definition` never appears in vanilla player text), "keeps" from Medic's English. |
| (default) / Default: {0}. / When enabled / When disabled | ` (Standard)` / `Standard: {0}.` / `Wenn aktiviert, ...` / `Wenn deaktiviert, ...` | Both siblings. |
| How long / How many / The time the pilot must wait ... (tooltip openers) | Wie lange ... / Wie viele ... / Wie lange der Pilot nach einem ... warten muss, bevor ... | Shipcracker `SCWC_CooldownDesc`, `SCWC_TankCapacityDesc`, Toxin's `Wie ...` openers (also vanilla's most common tooltip openers). |
| reload | nachladen, `Wird mit {0} nachgeladen.` | Toxin `TXWC_JetReserveDesc`; Core `Stat_Thing_Reload*`. |
| Workshop headers, safe to add / remove, SOS2 and VGE bullets, CE armor clause, mix and match, links | `[h1]Überblick[/h1]`, `[h2]Das Set[/h2]`, `[h1]Mod-Kompatibilität[/h1]`, `[h1]FAQ[/h1]`, `[h1]Links[/h1]`, `[b]Torso:[/b]` / `[b]Schulterpanzer:[/b]` / `[b]Helm:[/b]`, the two save bullets (`eingesargt`, per the upstream entomb row), `Rüstungswerte, Bulk und Bulk-Kapazität liegen auf dem Niveau ...`, `Es gelten dessen Vakuumregeln für Kriegssärge.`, `EVA-tauglich nach denselben Regeln wie die Sets aus VFE Pirates.`, `Die Teile lassen sich nach Belieben mit den Sets aus VFE Pirates kombinieren.`, the GitHub and art lines | Toxin and Shipcracker Workshop pages. |
| Workshop intro | Ein neuer Kriegssarg der Techstufe Raumfahrt für [url=...]Vanilla Factions Expanded - Pirates[/url]. | Shipcracker. |
| FAQ frames | `[b]F: Wie bekomme ich es?[/b]` / `A: Erforsche das VFE-Pirates-Projekt 'Raumfahrt-Kriegssärge' ...`, `[b]F: Ist das Set unausgewogen?[/b]` | Shipcracker; the answer body follows Medic's English ("build it"). |

## Charge nouns

| English | German | Forms | Lookup files |
|---|---|---|---|
| kit (the armor's `CompDressingSuite.chargeNoun`) | Medizinkit (n.) | pl. Medizinkits | `1.6/Languages/German/WordInfo/plural.txt` (`Medizinkit;Medizinkits`), `WordInfo/Gender/Neuter.txt` (`medizinkit`) |
| shot (the shoulders' `CompApparelReloadable.chargeNoun`) | Spritze (f.) | pl. Spritzen | `plural.txt` (`Spritze;Spritzen`), `WordInfo/Gender/Female.txt` (`spritze`) |

Neither noun has a natural masculine equivalent: Schuss is Core's gunshot
(and drug slang for an injection), Stoß reads as the body's own adrenaline
surge, and no masculine word for a medicine kit is idiomatic. The Gender
files carry them instead (decompile-verified on the local 1.7 assembly:
`LanguageWordInfo.LoadFrom` reads `Male.txt`, `Female.txt` and `Neuter.txt`,
`Neuter.txt` as `Gender.None`, which `ResolveGenderSymbol` maps to the third
option). Read aloud in Core's slots: `Medizinkits verbleibend` / `Spritzen
verbleibend`, `Die maximale Anzahl der Medizinkits` (the `plural_decline`
lookup misses harmlessly), `Medizinkit-Nachladekosten` / `Spritze-Nachladekosten`
(Core's fixed `{CHARGENOUN}-` compound drops the Fugen-n; acceptable in a
hyphenated UI compound), `Nachladekosten pro Spritze`, `ein Medizinkit` / `eine
Spritze nachzuladen`, `dessen letztes Medizinkit` / `letzte Spritze`, `Keine
Spritzen mehr.` The mod's own strings agree: `Medizinkits: {0} / {1}`,
`Spritzen: {0} / {1}`, `Anzahl Medizinkits`, `Anzahl Spritzen`.

## Grounded in the first pass

| English | German | Source |
|---|---|---|
| herbal / industrial / glitterworld medicine | Kräutermedizin / Medizin / Glitzerweltmedizin | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` `.label`; all feminine, so `Keine {0} geladen.` agrees |
| go-juice / wake-up | Go-Juice / Muntermacher | Core `GoJuice.label`, `WakeUp.label` |
| heart attack / wound infection / disease / missing body part | Herzinfarkt / Wundinfektion, Infektion / Krankheit / fehlender Körperteil | Core `HeartAttack.label`, `WoundInfection.label`, Keyed `LetterLabelTraitDisease`, `MissingBodyPart.label` |
| mechanites | Mechaniten | Core `FibrousMechanites.label`, `SensoryMechanites.label` |
| tend / tend quality / tend speed / medical skill | behandeln / Behandlungsqualität / Behandlungsgeschwindigkeit / Medizin-Fertigkeit | Core `TendPatient.reportString` `behandelt TargetA.`, Keyed `TendQuality`, `MedicalTendSpeed.label`, Keyed `SkillRequiredToBuild` `{0}-Fertigkeit` |
| has nothing to tend / no wounds | {0} muss nicht behandelt werden. / Wunden | Core Keyed `TendingNotRequired`, `NeedDoctorDesc` "Unbehandelte Wunden" |
| doctor (work type) / Work tab | Arzt, `als Arzt arbeiten` / Tab 'Arbeit' | Core `Doctor.labelShort`, MainButtonDef `Work.label` |
| manipulation / sight / pain shock / unconscious | Arbeiten / Sehen / Schmerzschock / bewusstlos | Core `Manipulation.label`, `Sight.label`, Keyed `PainShock` "Unter Schock wegen Schmerzen", `Unconscious` |
| shield / plasteel / spacer | Schutzschild / Plastahl / Raumfahrt | Core `Apparel_ShieldBelt.description`, `Plasteel.label`, Keyed `TechLevel_Spacer` |
| Mote | Mote | Core `Mote_*` labels (untranslated in de) |
| casting time / cooldown / range / line of sight | Wirkzeit / Abklingzeit / Reichweite / freie Sicht | Core Keyed `AbilityCastingTime`, `CooldownTime`, `Range`, `AbilityRequiresLOS` "Freie Sicht erforderlich" (vanilla's phrase, preferred over Shipcracker's Sichtlinie) |
| Blue / Green / Pink | Blau / Grün / Rosa | Core Keyed `Blue` blau, `Green` grün (lowercase there, capitalised as option labels like `Aus`); Core has no `Pink`, rosa per its descriptions (`Flamingo`) |
| vacuum | Vakuum | Odyssey `VacuumResistance.label` |
| patient | Patient | Core Keyed `PatientsAwaitingMedicalOperation` |
| bloodstream (hediff description) | im Blutkreislauf | Core `GoJuiceHigh.description` and every drug high |
| trait pawn tokens | `{PAWN_objective}` is the accusative (ihn/sie), so the description uses `beeinträchtigen` (+acc.); `{PAWN_pronoun}` the nominative | Core `Abrasive`, `BodyPurist` descriptions |
| report strings | `legt TargetA einen Mechanitenverband an.`, `verabreicht TargetA eine Adrenalinspritze.`; the abilities' `jobReportString` the same without TargetA | Core `JobDef` reportStrings: third person present, lowercase, trailing period; `FeedPatient` has TargetA in a dative slot |

## Pending native review

- Every coinage in "Set-specific rows": `Mechanitenverband` and the verb
  `anlegen`, `System für Mechanitenverbände`, `Trägermechaniten`,
  `Mechanitenstrom`, `Adrenalininjektor`, `feldmedizinische Manipulatoren`,
  `Diagnosevisier`, `medizinisches Assistenzmodul`, the four stage labels
  (`Schub` and `Tief` especially), `Wirkdauer`, `Verletzungsschweregrad`,
  `Nachladegegenstand`, `automatisches Wirken`, `Anfälligkeit für
  Unterdrückung`.
- The charge nouns `Medizinkit` and `Spritze` in the reload slots, and
  `Spritze-Nachladekosten`.
- `Keine {0} geladen.` assumes a feminine medicine label (true for vanilla's
  three); a modded masculine medicine would read `Keine Verband geladen`.
- All free prose: the three descriptions' first paragraphs and the shared
  paragraphs 2 and 3, both ability descriptions, the hediff and trait
  descriptions, every settings tooltip and summary, and the whole Workshop
  page. A fresh register read rewrote the calques it found.
