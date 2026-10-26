using Verse;

namespace MedicWarcasket;

// The mechanite dressing's timings beyond what VEF's AbilityDef carries, as a plain
// DefModExtension on MDWC_MechaniteDressing: VEF filters modExtensions to its own
// AbilityExtension_AbilityMod type, so this one is invisible to it and read only by the
// dressing (Ability_MechaniteDressing, JobDriver_MechaniteDressing). Values live in the def
// XML, rationale in its header; ticksPerTend is a mod setting defaulting to it, minTendSpeed
// and maxPotency have no setting.
//
// Tend quality is vanilla's formula (TendUtility.CalculateBaseTendQuality: the pilot's
// MedicalTendQuality times the medicine's potency, plus the bed's offset, clamped to the
// medicine's MedicalQualityMax, verified on 1.7) with the potency clipped at maxPotency: the
// carriers deliver only so much of a kit per wound, so a kit above the ceiling saturates the
// clamp later up the Medicine skill than it would in a doctor's hands (the def header has the
// levels). A kit under the ceiling is tended exactly as vanilla would.
//
// The beam's time per wound mirrors vanilla's tend duration, JobDriver_TendPatient's
// (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 600f) (BaseTendDuration 600,
// verified on 1.7): ticksPerTend / Max(MedicalTendSpeed, minTendSpeed), read off the pilot once
// as the beam connects. The Medicine skill, the shoulders' MedicalTendSpeed offset and the Sight
// and Manipulation factors reach the beam as they reach a vanilla tend. The stat's minValue is
// 0.1 and an unskilled pilot sits at 0.4, so unfloored the interval could reach ten times the
// setting and hold a patient under fire too long; the floor caps it at 1 / minTendSpeed times.
public class DressingExtension : DefModExtension
{
    // Ticks the beam spends on each wound at a MedicalTendSpeed of 1.0 (Medicine 10, no gear);
    // the first wound lands one interval after the beam connects.
    public int ticksPerTend = 60;

    // The MedicalTendSpeed below which the pilot's stat stops lengthening the interval.
    public float minTendSpeed = 0.5f;

    // The most of a kit's MedicalPotency the carriers deliver; a kit's potency above it is
    // clipped before vanilla's quality formula.
    public float maxPotency = 1.3f;

    // Ticks the pilot holds still after the beam while the glow fades, before the job ends.
    public int warmdownTicks = 60;
}
