# French glossary, Medic Warcasket

First pass: machine-assisted, 2026-11-05, no native review yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and style
(the elision worker, where `PostProcessed` runs, the hand-written contractions,
spaced `:` `?` `!`, guillemets for clicked UI) live in `l10n/languages/French.md`.
Folder `French`, grounded against the Core, Royalty, Biotech and Odyssey
`French (Français).tar`s. The VFEP warcasket vocabulary (warcasket, kept
untranslated after the Shipcracker native review, the label pattern, the parts,
foundry, entomb, VFEP's set names and research labels, the 10th-generation
opener `Coque warcasket spécialisée de technologie spatiale, offrant une
protection standard.`) lives upstream under "Grounded common vocabulary, VFE
Pirates warcasket domain", with the grounding source (no community VFEP French
pack survives); the settings and ability words the family settled (spacer-tech,
Off/On, cooldown, range, cells, the `cooldownGerund`) sit in that file's main
table. Medic's descriptions carry their own paragraphs 2 and 3 (the program
paragraph and the series paragraph), so VFEP's shared 10th-generation lore
paragraph is not used here; only the spacer opener is the family's. A VFEP
term is changed upstream first.

## Set-specific rows

| English | French | Grounding |
|---|---|---|
| medic (set name) | médecin (`warcasket médecin`, `épaulières de warcasket médecin`, `casque de warcasket médecin`; in-game prose `le modèle médecin`, Workshop `le Médecin`) | Core backstory `titleShort` for every "medic" (`CombatMedic82`, `MedicSoldier54`, `InfantryMedic6`, ...) and `Doctor.labelShort`; invariable noun in apposition, like `warcasket toxine`. |
| Workshop title / `MDWC_SettingsCategory` | Warcasket médecin | Title coupling; contains `warcasket`. |
| mechanite dressing (ability, section header) | pansement de mécanites | **Coined.** Core `FibrousMechanites.label` `mécanites fibreuses`; `pansement` is the ordinary dressing noun. Noun phrase, as Biotech ability labels are. |
| dress (verb) / a dressing | panser / un pansement | **Coined**, ordinary French (`panser une plaie`). |
| mechanite dressing suite (armor) | système de pansement de mécanites | **Coined.** `système` is vanilla's device noun (131 uses). |
| carrier mechanites | mécanites porteuses | **Coined**; feminine as Core's `mécanites fibreuses`. |
| diagnostic visor | visière de diagnostic | **Coined.** |
| medical assistance module | module d'assistance médicale | **Coined.** |
| field-medicine manipulators | manipulateurs médicaux de terrain | **Coined.** |
| adrenaline (hediff label) / adrenaline shot (ability, labelNoun) | adrénaline / injection d'adrénaline, `une injection d'adrénaline` | Core backstory prose `poussée d'adrénaline`; `injection` as `GoJuice.ingestCommandString` `S'injecter`. |
| adrenaline injector | injecteur d'adrénaline | **Coined.** |
| hediff stages rush / steady / wearing off / crash | montée / stable / se dissipe / descente | `se dissipe` is Core `Anesthetic.stages.wearing_off.label`; `montée` and `descente` are the ordinary French onset and comedown of a drug (**coined** here). |
| boost duration / crash duration | Durée de l'effet / Durée de la descente | **Coined.** |
| levels (of the boost) | paliers | **Coined.** |
| field surgeons | chirurgiens de campagne | **Coined** (`hôpital de campagne`). |
| squadmate | camarade | Core `CombatMedic82.description` `ses camarades`. |
| raider medics | les médecins pillards (label), les médecins des pillards et des alliés (tooltip) | Core `pillard` for raider. |
| reload item | Objet de rechargement | Core Keyed `Stat_Thing_ReloadRefill_Name` `Coût de rechargement`. |
| Reloads with {0}. | Rechargement : {0}. | Colon frame, so the bare label needs no article. |
| open kit / reload target / never reload | Kit entamé : reste {0} / Objectif de rechargement : {0} / Ne jamais recharger. | **Coined.** |
| effect color, Blue / Green / Pink | Couleur des effets, Bleu / Vert / Rose | Core Keyed `Color`, `Blue` `bleu`, `Green` `vert`; no Core `Pink`, `rose` is plain French. Capitalised as option labels, as the English. |
| autocast | lancement automatique | **Coined**; VEF ships English only, Core's auto-gizmos use `automatiquement`. |
| work types / caring work | tâches / tâches de soins | Core `Doctor.gerundLabel` `soins`; the summary `Le pilote peut travailler comme {0}.` takes the work types' `labelShort` (`médecin`, `nourrice`). |
| Work tab | l'onglet « Travail » | Core `Work.label` `travail`; guillemets for a clicked UI element. |

## Ported from the siblings

| English | French | From |
|---|---|---|
| Default: {0}. / (default) | Par défaut : {0}. / (par défaut) | both siblings' Keyed |
| When enabled, ... When disabled, ... | Si cette option est activée, ... Si elle est désactivée, ... | Shipcracker `SCWC_VacuumBurnDesc` |
| These settings are unavailable ... keeps its built-in values. | Ces paramètres sont indisponibles. Un autre mod a modifié le warcasket médecin. L'ensemble conserve donc ses valeurs d'origine. | Shipcracker's `SCWC_SettingsUnavailable`; "definitions" dropped as modder vocabulary (`définition` never reaches vanilla player text) |
| The time the pilot must wait after X before ... Default: {0}. | Le temps que le pilote doit attendre après un pansement avant de pouvoir en commencer un autre. | Shipcracker `SCWC_CooldownDesc` |
| The amount ... holds when full | ... que contient l'armure quand elle est pleine | Shipcracker `SCWC_TankCapacityDesc` |
| Workshop headers, Torse / Épaulières / Casque bullets, save-safety lines, SOS2 line, `Q :` / `R :`, research FAQ, links line | as shipped, except the research answer's `soudez l'ensemble sur un colon` (the siblings' `soudez-le` points back at the research project) | Toxin and Shipcracker French Workshop pages |
| cooldown gerund | rechargement | upstream main table (Shipcracker, Toxin) |

## Charge nouns

| comp | English | French | Forms | File |
|---|---|---|---|---|
| `CompDressingSuite` (armor) | kit | kit (m.) | kits by rule (`+s`) | none |
| `CompApparelReloadable` (shoulders) | shot | dose (f.) | doses by rule (`+s`) | none |

`LanguageWorker_French.Pluralize` is rule-only and never reads
`WordInfo/plural.txt`, so both are single words whose rule plural is right. fr
Keyed never switches on `CHARGENOUN_gender`, so no Gender file helps: `dose`
reads `doses restants` and `son dernier dose` in two of vanilla's reload
strings, exactly as vanilla's own feminine charge nouns do (`charge`,
`grenade`, `fusée`). `dose` is vanilla's medical dose noun (`une super-dose de
mécanites`, `une dose de sérum`), and the mod's own Keyed text names the shots
`doses` to match.

## Grounded in the first pass

| English | French | Source |
|---|---|---|
| herbal / industrial / glitterworld medicine | herbe médicinale / médicament / médicament avancé | Core `MedicineHerbal`, `MedicineIndustrial`, `MedicineUltratech` `.label` |
| medicine (category, gauge header) | médicaments | Core `ThingCategoryDef` `Medicine.label` |
| go-juice / wake-up | Go-Juice / Wake-up (capitalised in prose too) | Core `GoJuice.label`, `WakeUp.label`, `GoJuiceAddiction.description` |
| X in the bloodstream (drug hediff opener) | X dans la circulation sanguine. | Core `GoJuiceHigh.description`, `WakeUpHigh.description` |
| heart attack | attaque cardiaque | Core `HeartAttack.label` |
| infection / missing body part | infection / partie manquante | Core `WoundInfection.label`, `MissingBodyPart.label` |
| mechanites | mécanites (f.) | Core `FibrousMechanites.label`, `SensoryMechanites.label` |
| tend / tend quality / medical tend speed | soigner / qualité des soins / vitesse des soins médicaux | Core `TendPatient.reportString`, Keyed `Tend`, `MedicalTendQuality.label`, `MedicalTendSpeed.label` |
| medical skill | compétence en médecine | Core `Medicine.skillLabel` `médecine`, prose pattern `compétence en recherche` |
| injury severity | gravité des blessures | Core prose `gravité` (addiction severity) |
| doctor work | tâches de médecin | Core `Doctor.labelShort`, Keyed `NeedDoctorDesc` `la tâche de médecin` |
| manipulation / sight | manipulation / vue | Core `Manipulation.label`, `Sight.label` |
| pain shock | état de choc à cause de la douleur | Core Keyed `PainShock` `Choqué à cause de la douleur` |
| downed | à terre | Toxin's prose; Core `Downed.label` is `KO`, used as a state label only |
| raider / allied / raid | pillard / allié / raid | Core backstories, Keyed `AllyLower`, `Raid` |
| unassigned | non affecté | Core Keyed `NotAssigned` |
| casting time / range / line of sight | Temps de réalisation / Portée / Ligne de vue requise | Core Keyed `AbilityCastingTime`, `Range`, `AbilityRequiresLOS` |
| Mote (inherited mote label) | Particule | Core `Mote_*.label` (62 entries) |
| power armor | armure motorisée | Core `Apparel_PowerArmor.description` |
| shield bubble / plating | bouclier / blindage | Core `Apparel_ShieldBelt.label`; upstream `blindage` |
| vacuum resistance | résistance au vide | Odyssey `VacuumResistance.label` |
| reportString shape | `applique un pansement de mécanites à TargetA.` / `fait une injection d'adrénaline à TargetA.` | Core `TendPatient` `soigne TargetA.`, `Rescue` `porte secours à TargetA.`; the ability `jobReportString`s drop `à TargetA` |
| trait tokens | `soud{PAWN_gender ? é : ée : é(e)}`, `sur {PAWN_objective}`, mid-sentence `{PAWN_pronoun}` | Core `TraitDef` descriptions (`Masochist`, `BodyPurist`) and Keyed `ProhimObj` `lui` / `ProherObj` `elle` |

## Pending native review

Every coined row above; the set name `médecin` (against `infirmier` or
`secouriste`); `lancement automatique` (against `utilisation automatique`);
the charge noun `dose`; and all free prose, after a fresh register read
rewrote the calques it found: the three
descriptions (paragraph 1 per part, the shared paragraphs 2 and 3), both
ability descriptions, the hediff and trait descriptions, the settings
tooltips and summaries, and the whole Workshop page.
