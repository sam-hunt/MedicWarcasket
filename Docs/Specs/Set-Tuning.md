# SPEC: Set tuning (plating, shield, floors, costs and the shot's crash)

The design record behind the set's tuning: the review that framed it and the decisions taken
on it. The def headers carry the landed values; this file carries the comparison they were
priced against. Market values use vanilla's
ingredient prices (ComponentSpacer 200, Plasteel 9, Uranium 6, MedicineUltratech 50,
MedicineIndustrial 18, Steel 1.9); sums are ingredients only, as the Shipcracker and Toxin
headers compute them.

## 1. Where the set stood before tuning

### Identity among the family

Every other warcasket spends its budget on the pilot:

| Set                | Budget goes to                           | Model                    |
| ------------------ | ---------------------------------------- | ------------------------ |
| Siegebreaker       | Own ranged shield (2.5)                  | Self                     |
| Brute              | Own shield (5.0) spent as a bomb         | Self                     |
| Sarcophagus        | Pilot cannot die                         | Self                     |
| Controller         | Drones and mines                         | Summoner                 |
| Guardian           | Ten-second dome every ninety, 25% negate | Squad, prevents damage   |
| Shipcracker (ours) | Breach jump through walls                | Self, mobility           |
| Toxin (ours)       | Gas tank shared by three pieces          | Area denial              |
| **Medic**          | **Dressing and shot on other pawns**     | **Squad, undoes damage** |

The Medic is the only set that undoes damage rather than preventing it, the only one whose
abilities work undrafted and in the hospital, and the only one that consumes a real economy
item (medicine) rather than chemfuel or components, so it has an ongoing cost no other set
has. Guardian is its nearest neighbour in role; Sarcophagus in theme.

The three-piece loop is as tight as Brute's, the tightest in VFEP:

- shoulders' MedicalTendSpeed and helmet's MedicalTendQuality feed the armor's dressing
  through vanilla's tend formula
- the helmet gates disease and infection treatment (the Controller-helmet idiom)
- the armor's autocast chains into the shoulders' shot

That hook is distinct and worth keeping exactly as built.

### Balance of the kit as implemented

**Mechanite dressing.** About ten times a doctor's per-wound pace (60 ticks per wound at
MedicalTendSpeed 1.0 against vanilla's 600 per treatment), never wastes medicine (fractional
kits by severity, overflow carried, last-kit shortfall forgiven), no fetching, usable under
fire and uninterruptible by damage. Against that: the potency clip (1.3) keeps a real doctor
with glitterworld ahead on quality below Medicine 8, the patient is stunned while held, and
tending stops bleeding without restoring capacity. Its combat value is therefore "nobody
bleeds out mid-fight", which sits well beside Guardian's dome. Ten kits at twenty severity
each is plenty; the kit supply, not the ability, is the throttle. XP scales with medicine
spent, so chip damage cannot farm it. No exploit found.

**Adrenaline shot.** One industrial kit (18 silver) for go-juice's rush verbatim for an hour,
fading over two more, no tolerance, no addiction, rest fall x2/x3/x4 as the comedown (about
0.24 rest per shot). A third of go-juice's price for its best hour with none of its downside,
which is generous, but capped at two per fight since a drafted pilot cannot reload. Heart
attack cure is the standout flavour.

**Passives.** MedicalTendSpeed +0.25 (about four Medicine levels) on the shoulders,
MedicalTendQuality +0.10 (one level, a hospital bed's offset) on the helmet, Manipulation and
Sight floors at 1.0. Small next to the abilities; they mostly serve the Doctor-work identity.

**Trait.** Doctor open, hauling kept, everything else as VFEP's pilot. Fine.

### What the set gave up: nothing

As a copy of Siegebreaker it carried that set's full plating and its 2.5 shield, which is that
set's specialty, plus the whole medic kit and two unpriced first fills (10 glitterworld kits,
500 silver; 2 industrial kits, 36 silver).

## 2. Decisions

Each section is the recommendation as reviewed, with the decision taken on it where it
differs.

### 2.1 Shield to the tier baseline

**EnergyShieldEnergyMax 1.5 at EnergyShieldRechargeRate 0.01**, down from 2.5. Guardian,
Controller and Sarcophagus all carry 1.5; Siegebreaker's 2.5 and Brute's 5 are their
specialties (Toxin's spacer tier reasoned the same way). A ranged-only bubble is exactly what
a medic needs to reach a patient under fire, so keep the bubble, not the premium.

Plating stays **Sharp 2 / Blunt 1 / Heat 1** on all three pieces: the tier's shared values,
never under the tier floor. Blunt 1.5 is Guardian's and Brute's and is paid for in plasteel;
the Medic has no claim to it.

Mass, insulation, the psychic, toxic, suppression and vacuum offsets stay the tier's shared
values.

### 2.2 Price the fills in full and use the component dial

Three precedents for a reloadable's first fill: vanilla prices the whole fill into the pack,
VFEP prices none (Aerial, Shock), Toxin splits the difference (Chemfuel 30 for an 80 tank).
Medicine is stock the player values, and Sarcophagus already demands ten glitterworld kits
per set for nothing but flavour, so price the Medic's fills in full.

Components are VFEP's tech-complexity dial: armor 6 for the utility sets (Brute 4,
Controller 12); shoulders 0, or 2 for a verb (Controller, Toxin spacer); helmet 2, or 4 for a
helmet with a function of its own (Controller, Brute, Toxin spacer). Plasteel at the utility
sets' 145 / 60 / 60.

| Piece     | costList                                                              | Siegebreaker | Market value                     |
| --------- | --------------------------------------------------------------------- | ------------ | -------------------------------- |
| Armor     | ComponentSpacer 6, Plasteel 145, Uranium 50, **MedicineUltratech 10** | 6 / 145 / 50 | 3305                             |
| Shoulders | **ComponentSpacer 2**, Plasteel 60, **MedicineIndustrial 2**          | 60           | 976                              |
| Helmet    | **ComponentSpacer 4**, **Plasteel 60**                                | 2 / 50       | 1340                             |
| **Set**   |                                                                       | 4195         | **5621** (5085 before the fills) |

Peers at the same prices:

| Set                 | Market value |
| ------------------- | ------------ |
| Siegebreaker        | 4195         |
| Shipcracker         | 4780         |
| Sarcophagus         | 4785         |
| Guardian            | 5164         |
| Brute               | 5220         |
| Toxin, spacer tier  | about 5250   |
| **Medic**           | **5621**     |
| Controller          | about 6330   |

A kit set just over Guardian and well under Controller reads right; before the fills it sits
between Shipcracker and Guardian. Warcasket parts are untradeable and destroyed on drop, so
market value is colony wealth and raid points only.

The alternative considered: MedicineUltratech 5 on the armor (Sarcophagus's armor number
exactly, Toxin's half-fill model), set 5371. Rejected: a suite that ships full with half its
fill free is odd to explain.

### 2.3 Adrenaline: keep the rush, add a crash

The rush being go-juice's numbers verbatim is the fiction's point; shaving Moving makes it
a worse go-juice rather than a different thing. Instead a fourth, bottom stage, a crash over
the last quarter hour (the bottom twelfth of the severity, wearing off's minSeverity moved
up to 0.084), with Consciousness and Moving -0.10, no pain relief and the rest ladder's top
rung. Pure XML. It is the one cost the player feels, since rest fall is paid invisibly, and
it lands after most fights, so it taxes the afternoon rather than the fight. Two shots stay.

### 2.4 Watch the autocast interaction

With two shots and autocast on, both are gone after the first two dressings of a fight,
spent on whoever got dressed. That reads as intended ("patch them up and get them moving"),
so leave it, but it is the reason not to raise charges or nerf the rush in the same pass:
change one, re-read the other.

### 2.5 The helmet is rounded out

Quality, a Sight floor and the disease gate already exceed Controller's helmet (+1 drone)
and Siegebreaker's and Guardian's (inert). The review proposed keeping the Sight floor at 1.0
and offered a small MedicalSurgerySuccessChance offset as optional.

**Decision:** the floors drop to the tier's 0.7 on both the helmet (Sight) and the shoulders
(Manipulation). Full capacity floors are Sarcophagus's identity, and a pilot who wants them
can mix that set's pieces in at the foundry. The surgery offset (+0.05) is added as the
helmet's hospital hook.

### 2.6 Passives stay

Both are small next to the abilities and read as the doctor's tools. Unchanged.

### 2.7 Raid tables: keep the tags

A raider Medic is a Siegebreaker with a smaller shield, threat and flavour only, like
Shipcracker. Toxin's raider auto-vent shows AI use of a family ability is doable, so raider
medics dressing their downed is a possible later feature. Not a 1.0 item.

### 2.8 Default medicine and the description

Glitterworld kits are trade-only in vanilla, so the suite is a trader sink by default. That
fits the spacer tier and the Sarcophagus precedent; the dressing's tooltip now names the
configured kit at runtime, as the shot's does, so players know what to stock.

## 3. What landed

1. Armor: shield 1.5; costList adds MedicineUltratech 10.
2. Shoulders: costList ComponentSpacer 2, MedicineIndustrial 2; Manipulation floor 0.7.
3. Helmet: costList ComponentSpacer 4, Plasteel 60; Sight floor 0.7;
   MedicalSurgerySuccessChance +0.05.
4. Hediff: the crash stage.
5. The dressing's tooltip names its reload item.
6. Def headers rewritten in Shipcracker's format; the specs' kit and shot counts corrected.
