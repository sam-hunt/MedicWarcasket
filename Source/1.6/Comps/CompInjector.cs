using RimWorld;
using Verse;

namespace MedicWarcasket;

// The shoulders' adrenaline injector: vanilla's CompApparelReloadable plus a trim of an
// injector holding more shots than its maximum, which vanilla never does. The shot capacity
// setting lowers the maximum in place, and vanilla's reload accounting breaks on the overfill
// (NeedsReload is "not equal", MaxAmmoNeeded goes negative, and the reload job ends in SplitOff
// throwing; 1.6), so the excess is dropped before any reload can see it: on load, on equip (a
// stored piece's saved count) and on a settings write (MedicWarcasketSettings.TrimWornInjectors).
// remainingCharges is protected on CompApparelVerbOwner_Charged, which is why this is a
// subclass rather than a sweep from outside (the Shipcracker repo's CompBreachTank is the
// same comp). Declared by compClass on the shoulders' CompProperties_ApparelReloadable entry;
// the DefInjected handle of that entry's text fields is this class's name.
public class CompInjector : CompApparelReloadable
{
    public void Trim()
    {
        if (remainingCharges > MaxCharges)
            remainingCharges = MaxCharges;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            Trim();
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        Trim();
    }
}
