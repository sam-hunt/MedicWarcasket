using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace MedicWarcasket.Tests;

// The gated compat roots under 1.6/Mods/<Mod>/Patches/ only ever run with their mod active, so a
// renamed node in a main-tree def would break them silently for every player who has that mod
// and nobody else. Every operation's xpath must resolve to exactly one node in the main tree's
// defs merged as the game merges them, except a PatchOperationConditional's own xpath, which is
// a probe for a node that should be absent.
public class CompatPatchTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "MedicWarcasket.sln")))
                return dir.FullName;
        }
        throw new InvalidOperationException("MedicWarcasket.sln not found above the test output folder");
    }

    private static XDocument MergedDefs()
    {
        var defs = new XElement("Defs");
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoRoot, "1.6/Defs"), "*.xml", SearchOption.AllDirectories))
            defs.Add(XDocument.Load(file).Root!.Elements());
        return new XDocument(defs);
    }

    public static TheoryData<string> CompatPatchFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoRoot, "1.6/Mods"), "*.xml", SearchOption.AllDirectories)
                     .Where(f => f.Replace('\\', '/').Contains("/Patches/")))
            data.Add(file.Substring(RepoRoot.Length + 1));
        return data;
    }

    [Theory]
    [MemberData(nameof(CompatPatchFiles))]
    public void EveryXPath_ResolvesToOneMainTreeNode(string relativePath)
    {
        var defs = MergedDefs();
        var patch = XDocument.Load(Path.Combine(RepoRoot, relativePath));
        var operations = patch.Descendants().Where(e => e.Attribute("Class") != null && e.Element("xpath") != null);
        Assert.NotEmpty(operations);
        foreach (var op in operations)
        {
            var xpath = op.Element("xpath")!.Value;
            var hits = defs.XPathSelectElements(xpath).Count();
            var expected = (string)op.Attribute("Class") == "PatchOperationConditional" ? 0 : 1;
            Assert.True(hits == expected, $"{relativePath}: {xpath} resolved {hits} node(s), expected {expected}");
        }
    }
}
