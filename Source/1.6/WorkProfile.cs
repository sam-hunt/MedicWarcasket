using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using VEF.Pawns;
using Verse;

namespace MedicWarcasket;

// Derives the Medic trait (MDWC_WarcasketTrait_Medic) from VFEP_WarcasketTrait at startup, so the
// Medic's work profile and the trait's other effects follow VFEP's trait as the full mod list
// leaves it: a patch mod that opens work for every warcasket opens it for the Medic too, and
// whatever VFEP puts on its degree or VEF extension on the running game version (1.6 and 1.7
// differ) lands on ours. The trait def's header carries the player-facing contract; the spec
// (Docs/Specs/Medic-Work-Trait.md) the rationale.
//
// The Caring work types the mirror opens are the settings' toggles (WorkSetting): Mirror
// records which they are (Toggleable) and what the fixed rule makes of each (OpenByDefault:
// Doctor open, the rest blocked while VFEP blocks Caring), writes that default profile, and
// Apply writes the profile the settings choose, on every ApplyToDefs. A pawn caches its
// disabled work types, so Apply refreshes every live pawn carrying the trait; at startup there
// is none.
//
// Mirror runs first in Startup.Run, i.e. from a static constructor on startup and again after
// every in-process play-data reload, against defs fresh from XML and with every XML patch,
// VEF's toggleable ones included, already applied. Nothing it writes is cached anywhere by
// then: Trait.AllowsWorkType and Trait.GetDisabledWorkTypes read the def live, and
// Pawn.CombinedDisabledWorkTags is uncached.
public static class WorkProfile
{
    private const string VfepTraitName = "VFEP_WarcasketTrait";
    private const string MedicTraitName = "MDWC_WarcasketTrait_Medic";

    // The degree's text stays ours, and so do its identity (degree: the armor grants degree 0 and
    // saves store it) and its selection weight (commonality: irrelevant at TraitDef commonality
    // 0); everything else on it follows VFEP's.
    private static readonly HashSet<string> DegreeOwnFields =
    [
        nameof(TraitDegreeData.label),
        nameof(TraitDegreeData.labelMale),
        nameof(TraitDegreeData.labelFemale),
        nameof(TraitDegreeData.untranslatedLabel),
        nameof(TraitDegreeData.description),
        nameof(TraitDegreeData.degree),
        nameof(TraitDegreeData.commonality),
    ];

    // The mirror's result, replaced on every run and empty until the first or after a failed
    // one, so Apply then writes nothing and the trait keeps its XML profile.
    private static TraitDef medic;
    private static List<WorkTypeDef> baseTypes = [];
    private static List<WorkTypeDef> toggleable = [];
    private static HashSet<WorkTypeDef> openByDefault = [];

    // The Caring work types the Medic's tags leave open, in the work tab's order: what the
    // settings may open or block. Doctor always; Childcare under Biotech; any modded one.
    public static IReadOnlyList<WorkTypeDef> Toggleable => toggleable;

    // The fixed rule for a toggleable work type: Doctor is open, any other is open only when
    // VFEP's trait blocks neither Caring nor the type itself (a patch mod that opened Caring for
    // every warcasket opens it here too).
    public static bool OpenByDefault(WorkTypeDef work) => openByDefault.Contains(work);

    // Never throws out of Startup.Run: a failure here is logged and the settings still apply.
    public static void Mirror()
    {
        medic = null;
        baseTypes = [];
        toggleable = [];
        openByDefault = [];
        try
        {
            MirrorFromVfep();
        }
        catch (Exception e)
        {
            Log.Error($"[Medic Warcasket] Mirroring the medic warcasket trait from {VfepTraitName} failed; the trait keeps its XML profile and effects. {e}");
        }
    }

    private static void MirrorFromVfep()
    {
        TraitDef vfep = DefDatabase<TraitDef>.GetNamedSilentFail(VfepTraitName);
        TraitDef ours = DefDatabase<TraitDef>.GetNamedSilentFail(MedicTraitName);
        if (vfep == null || ours == null)
        {
            Log.Error($"[Medic Warcasket] Cannot mirror the medic warcasket trait: {(vfep == null ? VfepTraitName : MedicTraitName)} is missing. VFE Pirates is a hard dependency; this is a broken install.");
            return;
        }

        // Tags: ManualDumb covers only Hauling and Cleaning, and VFEP's hauling exemption is a
        // Harmony postfix keyed on its own def, so the swap keeps hauling open for the Medic.
        // Caring is the whole point of the trait.
        WorkTags tags = vfep.disabledWorkTags;
        if ((tags & WorkTags.ManualDumb) != 0)
            tags = (tags & ~WorkTags.ManualDumb) | WorkTags.Cleaning;
        bool caringBlocked = (tags & WorkTags.Caring) != 0;
        tags &= ~WorkTags.Caring;
        ours.disabledWorkTags = tags;

        // Work types: VFEP's own list by value, never aliased, split into the base the settings
        // never touch and the toggleables, every Caring work type the tags no longer block.
        // Scoped to Caring on purpose: a generic "VFEP blocks it, we do not" diff would also
        // re-block the Hauling the swap opened.
        var vfepTypes = new List<WorkTypeDef>(vfep.disabledWorkTypes ?? Enumerable.Empty<WorkTypeDef>());
        foreach (WorkTypeDef work in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
        {
            if ((work.workTags & WorkTags.Caring) == 0 || (tags & work.workTags) != 0)
                continue;
            toggleable.Add(work);
            if (work == WorkTypeDefOf.Doctor || (!caringBlocked && !vfepTypes.Contains(work)))
                openByDefault.Add(work);
        }
        baseTypes = vfepTypes.Where(work => !toggleable.Contains(work)).ToList();
        medic = ours;
        Write(OpenByDefault);

        // Effects: full overwrites, nulls included, so a field VFEP leaves at its default
        // replaces whatever our XML carried and the Medic matches VFEP's wearers on the running
        // version (1.7 has the pain factor and rest-fall on the degree; 1.6 has the pain factor
        // on the extension and nothing on the degree).
        if (vfep.degreeDatas.Count > 0 && ours.degreeDatas.Count > 0)
            CopyPublicFields(vfep.degreeDatas[0], ours.degreeDatas[0], DegreeOwnFields);
        else
            Log.Warning($"[Medic Warcasket] {VfepTraitName} or {MedicTraitName} has no degree; the medic trait keeps its XML effects.");

        TraitExtension vfepExtension = vfep.GetModExtension<TraitExtension>();
        TraitExtension medicExtension = ours.GetModExtension<TraitExtension>();
        if (vfepExtension != null && medicExtension != null)
            CopyPublicFields(vfepExtension, medicExtension, null);
        else
            Log.Warning($"[Medic Warcasket] {VfepTraitName} or {MedicTraitName} has no VEF TraitExtension; the medic trait keeps its XML extension.");
    }

    // Writes the profile the settings choose, open deciding each toggleable work type, and
    // refreshes every live pawn carrying the trait: Pawn.GetDisabledWorkTypes is cached per
    // pawn, and its Notify_DisabledWorkTypesChanged clears the cache and zeroes the work
    // priorities the write blocked (a type it opened stays unassigned until the player sets
    // it). World pawns too, since a pilot away on a quest comes back with the cache it left
    // with and nothing on its return clears it before the next save.
    public static void Apply(Func<WorkTypeDef, bool> open)
    {
        if (medic == null)
            return;
        Write(open);
        if (Current.ProgramState != ProgramState.Playing)
            return;
        foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive)
        {
            if (pawn.story?.traits != null && pawn.story.traits.HasTrait(medic))
                pawn.Notify_DisabledWorkTypesChanged();
        }
    }

    private static void Write(Func<WorkTypeDef, bool> open)
    {
        var types = new List<WorkTypeDef>(baseTypes);
        foreach (WorkTypeDef work in toggleable)
        {
            if (!open(work))
                types.Add(work);
        }
        medic.disabledWorkTypes = types;
    }

    // Copies every public instance field of T from source onto target by reflection, so a field
    // T gains in a later game or VEF version is carried without a code change. Enumerated on T,
    // not the source's runtime type, so a patch that subclasses VFEP's degree or extension still
    // copies cleanly. List<T> values are copied into new lists so the two defs never share one;
    // the elements (StatModifiers, curves) stay shared, which is fine as nothing mutates them.
    private static void CopyPublicFields<T>(T source, T target, ICollection<string> skip)
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (skip != null && skip.Contains(field.Name))
                continue;
            object value = field.GetValue(source);
            if (value is IList list && field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var copy = (IList)Activator.CreateInstance(field.FieldType);
                foreach (object item in list)
                    copy.Add(item);
                value = copy;
            }
            field.SetValue(target, value);
        }
    }
}
