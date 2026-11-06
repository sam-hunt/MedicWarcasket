using Verse;

namespace MedicWarcasket;

// The adrenaline shot's rules beyond what VEF's AbilityDef carries, as a plain DefModExtension
// on MDWC_AdrenalineShot, invisible to VEF as the dressing's DressingExtension is. Values live in
// the def XML, rationale in its header; stopsHeartAttacks is a mod setting defaulting to it.
//
// stopsHeartAttacks is read by Ability_AdrenalineShot.StopsHeartAttack, the one test every
// heart attack rule goes through: the target check (a patient with a heart attack is valid
// hostile or not, and at any adrenaline level), the cast (the attack removed instead of the
// surge), the cursor's "Stops the heart attack" line, and the dressing's autocast exception for
// a bed patient (Ability_MechaniteDressing.ShotToFollow). With it off, a patient with a heart
// attack is any other patient and gets the surge, and a raider medic's patient test, which is
// ValidateTarget, follows.
public class ShotExtension : DefModExtension
{
    // Whether a shot given to a patient with a heart attack removes the attack instead of
    // adding the surge.
    public bool stopsHeartAttacks = true;
}
