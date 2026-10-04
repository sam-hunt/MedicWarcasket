using System.Reflection;
using System.Runtime.InteropServices;

// Kept explicit (GenerateAssemblyInfo=false in the csproj) so the release flow can bump
// AssemblyVersion/AssemblyFileVersion in lockstep with About.xml's <modVersion> and
// CHANGELOG.md. Four-part X.Y.Z.0, where X.Y.Z is the mod's semantic version.
[assembly: AssemblyTitle("MedicWarcasket")]
[assembly: AssemblyDescription("A medic warcasket set for Vanilla Factions Expanded - Pirates")]
[assembly: AssemblyProduct("MedicWarcasket")]
[assembly: AssemblyCopyright("Copyright © 2026")]
[assembly: ComVisible(false)]
[assembly: Guid("7f849955-b881-4f4f-90e1-eecf2bca1126")]
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
// Mirrors About.xml <modVersion> verbatim, including any SemVer prerelease suffix
// (1.4.0-rc.1); the two numeric attributes above can't hold one and stay X.Y.Z.0.
[assembly: AssemblyInformationalVersion("0.1.0")]
