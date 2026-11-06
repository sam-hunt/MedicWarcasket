using System.Collections.Generic;
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
// that is not itself. The extension mirror's merge (MirroredExtensions) is pure too.
public class StrangerInBlackTests
{
    private sealed class OtherExtension : DefModExtension;

    private sealed class LoadoutExtension : DefModExtension;

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
    public void Mirror_AppendsFallbackExtensionsButNotTheGate()
    {
        var gate = new StrangerInBlackExtension();
        var loadout = new LoadoutExtension();
        var fallbackGate = new StrangerInBlackExtension();
        List<DefModExtension> merged = StrangerInBlack.MirroredExtensions([gate], [loadout, fallbackGate, null]);
        Assert.Equal<DefModExtension>([gate, loadout], merged);
    }

    [Fact]
    public void Mirror_KeepsOwnTypeAndIsIdempotent()
    {
        var gate = new StrangerInBlackExtension();
        var own = new LoadoutExtension();
        var other = new OtherExtension();
        List<DefModExtension> merged = StrangerInBlack.MirroredExtensions([gate, own], [new LoadoutExtension(), other]);
        Assert.Equal<DefModExtension>([gate, own, other], merged);
        Assert.Same(merged, StrangerInBlack.MirroredExtensions(merged, [new LoadoutExtension(), other]));
    }

    [Fact]
    public void Mirror_HandlesMissingLists()
    {
        List<DefModExtension> own = [new StrangerInBlackExtension()];
        Assert.Same(own, StrangerInBlack.MirroredExtensions(own, null));
        Assert.Null(StrangerInBlack.MirroredExtensions(null, null));
        var other = new OtherExtension();
        Assert.Equal<DefModExtension>([other], StrangerInBlack.MirroredExtensions(null, [other]));
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
