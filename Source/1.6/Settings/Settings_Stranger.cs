using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// Man in black section: two boxes. The first overrides the pawnKind of vanilla's
// StrangerInBlackJoin IncidentDef, the kind its worker generates: MDWC_StrangerInBlack
// (PawnKindDefs/StrangerInBlack_Medic.xml) while on, and while off the kind the XML left there
// (vanilla's StrangerInBlack, or whatever another mod patched in; our kind's fallback if the
// XML already named ours). The second overrides that kind's StrangerInBlackExtension
// requiresResearch, the gate Patches/PawnGenerator_GeneratePawn applies, and is drawn
// inactive while the first is off, since the gate then has nothing to gate; its saved choice
// holds for when the first is back on. Both defaults are the def fields as loaded, so the
// first box defaults off unless an XML patch points the incident at our kind already. The
// section needs none of the set's tunable defs, so it loads and applies ahead of the other
// sections' guard, as the work section does; with vanilla's incident or our kind missing (a
// mod removed one) it is skipped and the settings warn once. Its header carries the armor's
// item icon, read off the def handle LoadDefaultsFromDefs refreshes, and a tooltip saying
// what the event is.
public partial class MedicWarcasketSettings
{
    private const string StrangerIncidentDefName = "StrangerInBlackJoin";
    private const string StrangerKindDefName = "MDWC_StrangerInBlack";

    // Whether the incident generates our kind, and whether that kind waits for the research.
    public readonly ToggleSetting stranger = new("Stranger");
    public readonly ToggleSetting strangerResearch = new("StrangerResearch");

    private IncidentDef strangerIncident;
    private PawnKindDef strangerKind;
    private PawnKindDef strangerVanillaKind;
    private StrangerInBlackExtension strangerExtension;
    private ThingDef strangerArmor;
    private static bool strangerWarned;

    private bool StrangerFound => strangerIncident != null;

    private void ExposeStrangerSettings()
    {
        stranger.ExposeData();
        strangerResearch.ExposeData();
    }

    private void LoadStrangerDefaults()
    {
        strangerIncident = DefDatabase<IncidentDef>.GetNamedSilentFail(StrangerIncidentDefName);
        strangerKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(StrangerKindDefName);
        strangerExtension = strangerKind?.GetModExtension<StrangerInBlackExtension>();
        strangerArmor = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName);
        PawnKindDef loaded = strangerIncident?.pawnKind;
        strangerVanillaKind = loaded == strangerKind ? strangerExtension?.fallback : loaded;
        if (strangerIncident == null || strangerExtension == null || strangerVanillaKind == null)
        {
            if (!strangerWarned)
                Log.Warning("[Medic Warcasket] Settings could not find the man in black incident, its pawn kind or the medic kind's extension and fallback; the man in black section is unavailable.");
            strangerWarned = true;
            strangerIncident = null;
            return;
        }
        stranger.DefaultValue = loaded == strangerKind;
        strangerResearch.DefaultValue = strangerExtension.requiresResearch;
    }

    private void ApplyStrangerToDefs()
    {
        if (!StrangerFound)
            return;
        strangerIncident.pawnKind = stranger.Value ? strangerKind : strangerVanillaKind;
        strangerExtension.requiresResearch = strangerResearch.Value;
    }

    private void ResetStrangerSettings()
    {
        stranger.Reset();
        strangerResearch.Reset();
    }

    private void DrawStrangerSection(Listing_Standard listing)
    {
        if (!StrangerFound)
            return;
        Texture2D icon = strangerArmor?.uiIcon;
        SectionHeader(listing, "MDWC_SectionStranger", icon, HeaderTip("MDWC_SectionStranger".Translate(), "MDWC_SectionStrangerDesc".Translate()));
        SummaryRow(listing, StrangerSummary);
        stranger.DoRow(listing);
        strangerResearch.DoRow(listing, disabled: !stranger.Value);
    }

    // What the two boxes amount to: the stranger as vanilla sends him, in the set once the
    // research is in, or in the set from the start.
    private string StrangerSummary()
    {
        if (!stranger.Value)
            return "MDWC_StrangerSummaryOff".Translate();
        return (strangerResearch.Value ? "MDWC_StrangerSummary" : "MDWC_StrangerSummaryAlways").Translate();
    }
}
