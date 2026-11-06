using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Verse;
using Xunit;

namespace MedicWarcasket.Tests;

// The man in black's research gate (StrangerInBlack.Kind): a kind carrying the extension
// generates as its fallback while its project is required and unfinished, and as itself
// otherwise; a kind without the extension, or without a fallback, is never swapped. Pure: the
// defs are built bare and the researched flag is passed in (Researched reads the game). The
// XML facts below tie the kind to the rest of the feature: its weapon tag is the one the
// minigun patch adds, its required apparel is the set, and its extension names a fallback
// that is not itself.
public class StrangerInBlackTests
{
    private static readonly PawnKindDef Vanilla = new() { defName = "StrangerInBlack" };

    private static PawnKindDef Medic(bool requiresResearch, PawnKindDef fallback) => new()
    {
        defName = "MDWC_StrangerInBlack",
        modExtensions = [new StrangerInBlackExtension { requiresResearch = requiresResearch, fallback = fallback }],
    };

    [Fact]
    public void Kind_FallsBackWhileUnresearched()
    {
        PawnKindDef medic = Medic(true, Vanilla);
        Assert.Same(Vanilla, StrangerInBlack.Kind(medic, false));
        Assert.Same(medic, StrangerInBlack.Kind(medic, true));
    }

    [Fact]
    public void Kind_IgnoresResearchWhenNotRequired()
    {
        PawnKindDef medic = Medic(false, Vanilla);
        Assert.Same(medic, StrangerInBlack.Kind(medic, false));
        Assert.Same(medic, StrangerInBlack.Kind(medic, true));
    }

    [Fact]
    public void Kind_LeavesOtherKindsAlone()
    {
        Assert.Same(Vanilla, StrangerInBlack.Kind(Vanilla, false));
        Assert.Null(StrangerInBlack.Kind(null, false));
        PawnKindDef noFallback = Medic(true, null);
        Assert.Same(noFallback, StrangerInBlack.Kind(noFallback, false));
    }

    [Fact]
    public void Xml_KindMatchesPatchAndSet()
    {
        XDocument defs = CompatPatchTests.MergedDefs();
        XElement kind = defs.XPathSelectElement("/Defs/PawnKindDef[defName = \"MDWC_StrangerInBlack\"]");
        Assert.NotNull(kind);

        XDocument patch = XDocument.Load(Path.Combine(CompatPatchTests.RepoRoot, "1.6/Patches/StrangerInBlackMinigun.xml"));
        string tag = Assert.Single(patch.Descendants("value").Elements("li")).Value;
        Assert.Equal(tag, Assert.Single(kind.Element("weaponTags")!.Elements("li")).Value);

        var required = kind.Element("apparelRequired")!.Elements("li").Select(li => li.Value).ToList();
        Assert.Equal(["MDWC_Warcasket_Medic", "MDWC_WarcasketShoulders_Medic", "MDWC_WarcasketHelmet_Medic"], required);
        foreach (string piece in required)
            Assert.NotNull(defs.XPathSelectElement($"/Defs/*[defName = \"{piece}\"]"));

        XElement ext = Assert.Single(kind.Element("modExtensions")!.Elements("li"));
        Assert.Equal("MedicWarcasket.StrangerInBlackExtension", (string)ext.Attribute("Class"));
        Assert.Equal("StrangerInBlack", ext.Element("fallback")!.Value);
        Assert.Equal("true", ext.Element("requiresResearch")!.Value);
    }
}
