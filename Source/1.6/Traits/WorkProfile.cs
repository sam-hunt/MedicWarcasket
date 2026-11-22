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
// differ) lands on ours. The trait def's header carries the player-facing contract; the specs
// (Docs/Specs/Medic-Work-Trait.md, Docs/Specs/Intellectual-Work.md) the rationale.
//
// The work the settings may open or block is every work type carrying one of ToggleTags (Caring,
// Intellectual) that the trait's other tags leave open: Doctor, Childcare under Biotech,
// Research, Dark study under Anomaly, and any modded type with either tag. Derive records which
// they are (Toggleable) and what the fixed rule makes of each (OpenByDefault: Doctor open, any
// other open only while VFEP's trait blocks neither its toggle tags nor the type itself), and
// Compose turns a choice of open types into the trait's tags and work types. Mirror writes the
// default profile and Apply the one the settings choose, on every ApplyToDefs. A pawn caches its
// disabled work types, so Apply refreshes every live pawn carrying the trait; at startup there
// is none.
//
// The two tags differ in how they leave the trait. Caring is always cleared, Doctor being the
// point of the trait. Intellectual stays a tag, as VFEP sets it, until an open type carries it,
// so the default trait is VFEP's Intellectual block unchanged: the game reads the tag itself in
// HackUtility.IsCapableOfHacking and the mechlink install warning, and other mods may too. While
// the tag stays, the closed types it blocks are not listed again as work types. Once a type
// opens, the tag clears and every closed Intellectual type is listed instead; the Intellectual
// skill then follows the work types (SkillDef.IsDisabled counts a skill disabled when every work
// type using it is), and hacking with it. Verified against 1.7.5000 rev1275 and the 1.6 runtime.
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

    // The tags whose work types the settings may open. A fixed list on purpose: a generic "VFEP
    // blocks it, we do not" diff would also catch the Hauling the ManualDumb swap opens.
    public const WorkTags ToggleTags = WorkTags.Caring | WorkTags.Intellectual;

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

    // What VFEP's trait leaves the Medic before the settings: the tags (ManualDumb swapped for
    // Cleaning, Caring cleared, Intellectual kept if VFEP has it), the work types the settings
    // never touch, and the toggleable ones with their defaults.
    public sealed class Profile
    {
        public WorkTags baseTags;
        public List<WorkTypeDef> baseTypes = [];
        public List<WorkTypeDef> toggleable = [];
        public HashSet<WorkTypeDef> openByDefault = [];
    }

    // The mirror's result, replaced on every run and empty until the first or after a failed
    // one, so Apply then writes nothing and the trait keeps its XML profile.
    private static TraitDef medic;
    private static Profile profile = new();

    // The toggleable work types, in the work tab's order: what the settings may open or block.
    public static IReadOnlyList<WorkTypeDef> Toggleable => profile.toggleable;

    // The fixed rule for a toggleable work type (Derive).
    public static bool OpenByDefault(WorkTypeDef work) => profile.openByDefault.Contains(work);

    // Never throws out of Startup.Run: a failure here is logged and the settings still apply.
    public static void Mirror()
    {
        medic = null;
        profile = new Profile();
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

        profile = Derive(vfep.disabledWorkTags, vfep.disabledWorkTypes, WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder, WorkTypeDefOf.Doctor);
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

    // The Medic's profile from VFEP's tags and work types, over every work type in the work
    // tab's order. Pure, so the tests reach it with bare defs; doctor is passed in for the same
    // reason. VFEP's list is read by value, never aliased.
    public static Profile Derive(WorkTags vfepTags, IEnumerable<WorkTypeDef> vfepTypes, IEnumerable<WorkTypeDef> allTypes, WorkTypeDef doctor)
    {
        // ManualDumb covers only Hauling and Cleaning, and VFEP's hauling exemption is a Harmony
        // postfix keyed on its own def, so the swap keeps hauling open for the Medic.
        WorkTags tags = vfepTags;
        if ((tags & WorkTags.ManualDumb) != 0)
            tags = (tags & ~WorkTags.ManualDumb) | WorkTags.Cleaning;
        tags &= ~WorkTags.Caring;

        var result = new Profile { baseTags = tags };
        var vfepList = new List<WorkTypeDef>(vfepTypes ?? Enumerable.Empty<WorkTypeDef>());
        WorkTags otherTags = tags & ~ToggleTags;
        WorkTags vfepToggleTags = vfepTags & ToggleTags;
        foreach (WorkTypeDef work in allTypes)
        {
            if ((work.workTags & ToggleTags) == 0 || (otherTags & work.workTags) != 0)
                continue;
            result.toggleable.Add(work);
            if (work == doctor || ((vfepToggleTags & work.workTags) == 0 && !vfepList.Contains(work)))
                result.openByDefault.Add(work);
        }
        result.baseTypes = vfepList.Where(work => !result.toggleable.Contains(work)).ToList();
        return result;
    }

    // The trait's tags and work types for a choice of open toggleable types: Intellectual clears
    // once an open type carries it, and a closed type is listed unless the tags already block it.
    public static (WorkTags tags, List<WorkTypeDef> types) Compose(Profile from, Func<WorkTypeDef, bool> open)
    {
        WorkTags tags = from.baseTags;
        if (from.toggleable.Any(work => (work.workTags & WorkTags.Intellectual) != 0 && open(work)))
            tags &= ~WorkTags.Intellectual;
        var types = new List<WorkTypeDef>(from.baseTypes);
        foreach (WorkTypeDef work in from.toggleable)
        {
            if (!open(work) && (tags & work.workTags) == 0)
                types.Add(work);
        }
        return (tags, types);
    }

    // Writes the profile the settings choose, open deciding each toggleable work type, and
    // refreshes every live pawn carrying the trait: Pawn.GetDisabledWorkTypes is cached per
    // pawn, and its Notify_DisabledWorkTypesChanged clears the cache, refreshes the skills'
    // disabled state (the Intellectual skill follows a Research toggle) and zeroes the work
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
        (medic.disabledWorkTags, medic.disabledWorkTypes) = Compose(profile, open);
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
