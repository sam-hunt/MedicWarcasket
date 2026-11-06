using HarmonyLib;
using Verse;

namespace MedicWarcasket.Patches;

// Applies a kind's research gate (StrangerInBlack) to every generation request: a request for
// a kind whose StrangerInBlackExtension requires an unfinished project generates the
// extension's fallback kind instead. The request is a struct passed by value, so the prefix
// takes it by ref and vanilla generates from the rewritten copy (Harmony hands a ref parameter
// the original's own argument); its KindDef setter writes the field the getter reads unless
// the request carries a PawnKindDefGetter (a xenotype-driven kind), which no wanderer-join
// request does. Every other kind passes through at the cost of one extension lookup. The
// one-argument overload is the same on 1.6 and 1.7; the kind-and-faction overload builds a
// request and calls it.
[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
public static class PawnGenerator_GeneratePawn
{
    public static void Prefix(ref PawnGenerationRequest request)
    {
        PawnKindDef kind = request.KindDef;
        PawnKindDef resolved = StrangerInBlack.Kind(kind, StrangerInBlack.Researched(kind));
        if (resolved != kind)
            request.KindDef = resolved;
    }
}
