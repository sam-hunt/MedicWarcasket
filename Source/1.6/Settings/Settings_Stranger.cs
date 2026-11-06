using RimWorld;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// Man in black section: three boxes, each a write onto vanilla's StrangerInBlackJoin IncidentDef
// or our kind.
//
// The first overrides the incident's pawnKind, the kind its worker generates: MDWC_StrangerInBlack
// (PawnKindDefs/StrangerInBlack_Medic.xml) while on, and while off the kind the XML left there
// (vanilla's StrangerInBlack, or whatever another mod patched in; our kind's fallback if the
// XML already named ours). The second overrides that kind's StrangerInBlackExtension
// requiresResearch, the gate Patches/PawnGenerator_GeneratePawn applies, and is drawn inactive
// while the first is off, since the gate then has nothing to gate; its saved choice holds for
// when the first is back on. Both are drawn inactive, and the summary says why, when the
// incident's worker does not read pawnKind (StrangerInBlack.WorkerReadsKind: a mod such as
// Stranger In Black Techlevel replaced the worker with one that picks its own kind); the writes
// still happen and have no effect.
//
// The third lifts the incident's pawnFixedGender, the one place vanilla makes the stranger a
// man (the kind sets no gender and carries labelFemale). While on it writes Gender.None and
// swaps the incident's letter for neutral wording: letterLabel ("Man in black") for
// MDWC_StrangerLetterLabel, "Stranger in black", the kind's own neutral label, and letterText,
// which names the pawn by its gendered kind label and pronoun ([PAWN_kind], [PAWN_pronoun]),
// for MDWC_StrangerLetterText, vanilla's sentences with "stranger in black" and "they". While
// off it writes back the gender and letter as loaded, Male if the XML left none. If the XML
// already left the gender unfixed, the loaded letter is kept either way: the mod that freed it
// owns its letter. It applies whatever the first box says, to vanilla's stranger, ours, or a
// VEF faction replacement alike, and stays active under a replaced worker that still reads the
// gender (Stranger In Black Techlevel does, and titles the letter from its own strings).
//
// Every default is the def field as loaded, so the first box defaults off unless an XML patch
// points the incident at our kind already, and the third off unless one unfixed the gender.
// The section needs none of the set's tunable defs, so it loads and applies ahead of the other
// sections' guard, as the work section does; with vanilla's incident or our kind missing (a
// mod removed one) it is skipped and the settings warn once. Its header carries the armor's
// item icon, read off the def handle LoadDefaultsFromDefs refreshes, and a tooltip saying
// what the event is.
public partial class MedicWarcasketSettings
{
    private const string StrangerIncidentDefName = "StrangerInBlackJoin";
    private const string StrangerKindDefName = "MDWC_StrangerInBlack";

    // Whether the incident generates our kind, whether that kind waits for the research, and
    // whether the stranger may be a woman.
    public readonly ToggleSetting stranger = new("Stranger");
    public readonly ToggleSetting strangerResearch = new("StrangerResearch");
    public readonly ToggleSetting strangerAnyGender = new("StrangerAnyGender");

    private IncidentDef strangerIncident;
    private PawnKindDef strangerKind;
    private PawnKindDef strangerVanillaKind;
    private StrangerInBlackExtension strangerExtension;
    private bool strangerKindIgnored;
    private Gender strangerFixedGender;
    private string strangerFixedLabel;
    private string strangerAnyGenderLabel;
    private string strangerFixedText;
    private string strangerAnyGenderText;
    private ThingDef strangerArmor;
    private static bool strangerWarned;

    private bool StrangerFound => strangerIncident != null;

    private void ExposeStrangerSettings()
    {
        stranger.ExposeData();
        strangerResearch.ExposeData();
        strangerAnyGender.ExposeData();
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
        strangerKindIgnored = !StrangerInBlack.WorkerReadsKind(strangerIncident.workerClass);

        Gender loadedGender = strangerIncident.pawnFixedGender;
        strangerAnyGender.DefaultValue = loadedGender == Gender.None;
        strangerFixedGender = loadedGender == Gender.None ? Gender.Male : loadedGender;
        strangerFixedLabel = strangerIncident.letterLabel;
        strangerAnyGenderLabel = loadedGender == Gender.None ? strangerFixedLabel : "MDWC_StrangerLetterLabel".Translate().RawText;
        strangerFixedText = strangerIncident.letterText;
        strangerAnyGenderText = loadedGender == Gender.None ? strangerFixedText : "MDWC_StrangerLetterText".Translate().RawText;
    }

    private void ApplyStrangerToDefs()
    {
        if (!StrangerFound)
            return;
        strangerIncident.pawnKind = stranger.Value ? strangerKind : strangerVanillaKind;
        strangerExtension.requiresResearch = strangerResearch.Value;
        strangerIncident.pawnFixedGender = strangerAnyGender.Value ? Gender.None : strangerFixedGender;
        strangerIncident.letterLabel = strangerAnyGender.Value ? strangerAnyGenderLabel : strangerFixedLabel;
        strangerIncident.letterText = strangerAnyGender.Value ? strangerAnyGenderText : strangerFixedText;
    }

    private void ResetStrangerSettings()
    {
        stranger.Reset();
        strangerResearch.Reset();
        strangerAnyGender.Reset();
    }

    private void DrawStrangerSection(Listing_Standard listing)
    {
        if (!StrangerFound)
            return;
        Texture2D icon = strangerArmor?.uiIcon;
        SectionHeader(listing, "MDWC_SectionStranger", icon, HeaderTip("MDWC_SectionStranger".Translate(), "MDWC_SectionStrangerDesc".Translate()));
        SummaryRow(listing, StrangerSummary);
        stranger.DoRow(listing, disabled: strangerKindIgnored);
        strangerResearch.DoRow(listing, disabled: strangerKindIgnored || !stranger.Value);
        strangerAnyGender.DoRow(listing);
    }

    // What the boxes amount to: the stranger as vanilla sends them, in the set once the research
    // is in, or in the set from the start, or another mod's stranger whatever the first two say;
    // then whether they may be a woman.
    private string StrangerSummary()
    {
        string summary;
        if (strangerKindIgnored)
            summary = "MDWC_StrangerSummaryReplaced".Translate();
        else if (!stranger.Value)
            summary = "MDWC_StrangerSummaryOff".Translate();
        else
            summary = (strangerResearch.Value ? "MDWC_StrangerSummary" : "MDWC_StrangerSummaryAlways").Translate();
        return strangerAnyGender.Value ? summary + " " + "MDWC_StrangerSummaryAnyGender".Translate() : summary;
    }
}
