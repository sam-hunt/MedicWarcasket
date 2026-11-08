using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MedicWarcasket;

// Work section: one checkbox per Caring or Intellectual work type the trait mirror leaves to the
// player (WorkSetting over WorkProfile.Toggleable), written onto the medic trait's work tags and
// types through WorkProfile.Apply, which also refreshes every entombed pilot; the mirror's own rule
// is each box's default. Its header's icon is vanilla's mining work site, a Core texture, so
// it survives a play-data reload as a plain static, and its tooltip says what the section
// covers and what it leaves to VFEP's trait. The section is skipped while the mirror found
// nothing to toggle (a broken install). It reads none of the set's defs, so it applies ahead
// of the other sections' guard (ApplyToDefs).
public partial class MedicWarcasketSettings
{
    public readonly WorkSetting work = new();

    // The section's header icon, vanilla's mining work site. Its art is in the base game's
    // Resources, not Ideology's, so it loads whatever DLCs are active and survives a reload.
    private Texture2D workIcon;

    private void ExposeWorkSettings() => work.ExposeData();

    private void LoadWorkDefaults() =>
        workIcon = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Sites/MiningWorkSite");

    private void ApplyWorkToDefs() => WorkProfile.Apply(work.IsOpen);

    private void ResetWorkSettings() => work.Reset();

    private void DrawWorkSection(Listing_Standard listing)
    {
        if (WorkProfile.Toggleable.Count == 0)
            return;
        SectionHeader(listing, "MDWC_SectionWork", workIcon, HeaderTip("MDWC_SectionWork".Translate(), "MDWC_SectionWorkDesc".Translate()));
        SummaryRow(listing, WorkSummary);
        work.DoRows(listing);
    }

    // What the work section's boxes amount to: the work the pilot may take on, named as the work
    // tab names it, or that there is none; the rest is VFEP's rule either way.
    private string WorkSummary()
    {
        List<string> open = work.Open.Select(WorkSetting.Name).ToList();
        if (open.Count == 0)
            return "MDWC_WorkSummaryNone".Translate();
        return "MDWC_WorkSummary".Translate(open.ToCommaList(useAnd: true));
    }
}
