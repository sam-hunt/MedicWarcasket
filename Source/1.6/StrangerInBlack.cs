using RimWorld;
using Verse;

namespace MedicWarcasket;

// The man in black's replacement kind and its research gate. MDWC_StrangerInBlack
// (1.6/Defs/PawnKindDefs/StrangerInBlack_Medic.xml) is vanilla's StrangerInBlack in the set
// with the warcasket minigun; the mod setting (Settings/Settings_Stranger.cs) writes it onto
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
}
