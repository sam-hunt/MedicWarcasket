using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace MedicWarcasket;

// The man in black's replacement kind and its research gate. MDWC_StrangerInBlack
// (1.6/Defs/PawnKindDefs/StrangerInBlack_Medic.xml) is vanilla's StrangerInBlack in the set
// with the warcasket slugthrower; the mod setting (Settings/Settings_Stranger.cs) writes it onto
// vanilla's StrangerInBlackJoin IncidentDef as its pawnKind, and the vanilla worker generates
// it as it would any kind: IncidentWorker_WandererJoin.GeneratePawn reads def.pawnKind into a
// PawnGenerationRequest (the same on 1.6 and 1.7). VEF's transpiler on that read
// (Storyteller/Harmony/IncidentWorker_WandererJoin_GeneratePawn) keeps the def's kind unless
// the player faction's FactionDefExtension names a strangerInBlackReplacement, which then wins
// over ours; a faction mod that redraws its own stranger has the stronger claim.
//
// The gate is the kind's own contract, carried by its StrangerInBlackExtension: a kind that
// requires a research project generates as its fallback kind while the project is unfinished.
// Patches/PawnGenerator_GeneratePawn.cs applies it to every generation request, so the vanilla
// stranger walks in until the colony can build warcaskets, whatever asked for ours. Kind is the
// pure rule, reached by the tests; Researched reads the game.
//
// The kind is a copy of its fallback, so what other mods attach to the fallback misses ours
// unless it is mirrored. MirrorFallbackExtensions, run from Startup.Run, appends the fallback's
// mod extensions to the kind's own: Combat Extended's LoadoutPropertiesExtension (the magazines
// its generation postfix adds for the kind's weapon) and Altered Carbon's StackSpawnModExtension
// (the chance of a cortical stack) are two that vanilla's StrangerInBlack carries under those
// mods. Only extensions are mirrored: a patch on the fallback's fields (apparel, weapon, race)
// describes the duster stranger, not ours, and no mod patches its other fields. Extensions are
// read at generation time off the kind, so sharing the fallback's instances is enough.
//
// WorkerReadsKind is the settings' test for whether the incident's pawnKind is read at all: a
// worker that overrides IncidentWorker_WandererJoin.GeneratePawn picks its own kind (Stranger
// In Black Techlevel's IncidentWorker_StrangerInBlack chooses one per player tech level and
// never calls base), so the setting that writes pawnKind is drawn inactive.
public sealed class StrangerInBlackExtension : DefModExtension
{
    // The project the colony must have finished for the kind to generate as itself, and whether
    // it must: requiresResearch is the mod setting's field, research the project it names.
    public ResearchProjectDef research;
    public bool requiresResearch = true;

    // The kind generated in its place while the gate is closed: vanilla's own stranger.
    public PawnKindDef fallback;
}

public static class StrangerInBlack
{
    // The kind a request for the given kind generates: the fallback while the gate is closed,
    // the kind itself otherwise. A kind without the extension, or with no fallback, is never
    // swapped, and research is only read when the extension requires it.
    public static PawnKindDef Kind(PawnKindDef requested, bool researched)
    {
        StrangerInBlackExtension ext = requested?.GetModExtension<StrangerInBlackExtension>();
        if (ext == null || ext.fallback == null || !ext.requiresResearch || researched)
            return requested;
        return ext.fallback;
    }

    // Whether the kind's project is finished in the running game. A kind naming no project
    // counts as researched; with no game running (a pawn generated from the main menu) the gate
    // stays closed, since there is no colony to have researched anything.
    public static bool Researched(PawnKindDef kind)
    {
        StrangerInBlackExtension ext = kind?.GetModExtension<StrangerInBlackExtension>();
        if (ext?.research == null)
            return true;
        return Current.Game != null && ext.research.IsFinished;
    }

    // Appends each kind's fallback's mod extensions to its own, for every kind carrying the
    // gate. Idempotent: a type the kind already carries is not added again, so a re-run, or an
    // extension a compat patch put on our kind directly, keeps the kind's own.
    public static void MirrorFallbackExtensions()
    {
        foreach (PawnKindDef kind in DefDatabase<PawnKindDef>.AllDefsListForReading)
        {
            PawnKindDef fallback = kind.GetModExtension<StrangerInBlackExtension>()?.fallback;
            if (fallback != null && fallback != kind)
                kind.modExtensions = MirroredExtensions(kind.modExtensions, fallback.modExtensions);
        }
    }

    // The kind's extensions followed by every fallback extension of a type the kind does not
    // carry, the gate itself excepted; the kind's own list, unchanged, when nothing is added.
    public static List<DefModExtension> MirroredExtensions(List<DefModExtension> own, List<DefModExtension> fallback)
    {
        List<DefModExtension> result = null;
        foreach (DefModExtension extension in fallback ?? [])
        {
            if (extension == null || extension is StrangerInBlackExtension || Carries(result ?? own, extension.GetType()))
                continue;
            result ??= own == null ? [] : [.. own];
            result.Add(extension);
        }
        return result ?? own;
    }

    private static bool Carries(List<DefModExtension> extensions, Type type)
    {
        if (extensions == null)
            return false;
        foreach (DefModExtension extension in extensions)
        {
            if (extension?.GetType() == type)
                return true;
        }
        return false;
    }

    // Whether an incident with this worker class generates its def's pawnKind: true for
    // IncidentWorker_WandererJoin and any subclass that inherits its GeneratePawn(Map), false
    // for an override or a worker outside that family.
    public static bool WorkerReadsKind(Type worker)
    {
        if (worker == null || !typeof(IncidentWorker_WandererJoin).IsAssignableFrom(worker))
            return false;
        MethodInfo generate = worker.GetMethod(nameof(IncidentWorker_WandererJoin.GeneratePawn),
            BindingFlags.Public | BindingFlags.Instance, null, [typeof(Map)], null);
        return generate?.DeclaringType == typeof(IncidentWorker_WandererJoin);
    }
}
