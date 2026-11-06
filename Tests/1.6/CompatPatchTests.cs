using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace MedicWarcasket.Tests;

// The gated compat roots under 1.6/Mods/<Mod>/Patches/ only ever run with their mod active, and
// 1.6/Patches/SOS2Patch.xml sits behind a PatchOperationFindMod gate of its own, so a renamed node
// in a main-tree def would break any of them silently for every player who has that mod and
// nobody else. Every operation's xpath must resolve in the main tree's defs, merged as the game
// merges them, to one node per MDWC_ def it names: the SOS2 patch names its two defs in one
// xpath, as VFEP's does. A PatchOperationConditional's own xpath is the exception, a probe for
// a node that should be absent, and so is an xpath naming only a dependency's defs (the
// slugthrower tag patch): the main tree cannot resolve it, and such a patch stays ungated, so a
// renamed target logs a patch error at every boot for everyone.
public class CompatPatchTests
{
    internal static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "MedicWarcasket.sln")))
                return dir.FullName;
        }
        throw new InvalidOperationException("MedicWarcasket.sln not found above the test output folder");
    }

    internal static XDocument MergedDefs()
    {
        var defs = new XElement("Defs");
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoRoot, "1.6/Defs"), "*.xml", SearchOption.AllDirectories))
            defs.Add(XDocument.Load(file).Root!.Elements());
        return new XDocument(defs);
    }

    public static TheoryData<string> PatchFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoRoot, "1.6"), "*.xml", SearchOption.AllDirectories)
                     .Where(f => f.Replace('\\', '/').Contains("/Patches/"))
                     .OrderBy(f => f, StringComparer.Ordinal))
            data.Add(file.Substring(RepoRoot.Length + 1));
        return data;
    }

    [Theory]
    [MemberData(nameof(PatchFiles))]
    public void EveryXPath_ResolvesToOneMainTreeNodePerNamedDef(string relativePath)
    {
        var defs = MergedDefs();
        var patch = XDocument.Load(Path.Combine(RepoRoot, relativePath));
        var operations = patch.Descendants().Where(e => e.Attribute("Class") != null && e.Element("xpath") != null);
        Assert.NotEmpty(operations);
        foreach (var op in operations)
        {
            var xpath = op.Element("xpath")!.Value;
            var hits = defs.XPathSelectElements(xpath).Count();
            var expected = (string)op.Attribute("Class") == "PatchOperationConditional"
                ? 0
                : Regex.Matches(xpath, "defName\\s*=\\s*\"MDWC_").Count;
            Assert.True(hits == expected, $"{relativePath}: {xpath} resolved {hits} node(s), expected {expected}");
        }
    }
}
