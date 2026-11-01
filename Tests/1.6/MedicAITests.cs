using RimWorld;
using Verse.AI.Group;
using Xunit;

namespace MedicWarcasket.Tests;

// The raider medics' pure rules (MedicAI): the quiet window, which lord toils count as
// leaving, and which of two patients is preferred. The toils are built bare, as their
// constructors take no game state; the faction and lord rules need a world and are left to
// the in-game checks (Docs/Specs/Raider-Medics.md 6).
public class MedicAITests
{
    private const int QuietTicks = 300;

    [Fact]
    public void Quiet_HoldsOnceBothWindowsHavePassed()
    {
        Assert.True(MedicAI.Quiet(1000, 700, 700, QuietTicks));
        Assert.True(MedicAI.Quiet(1000, -99999, -99999, QuietTicks));
        Assert.False(MedicAI.Quiet(999, 700, 0, QuietTicks));
        Assert.False(MedicAI.Quiet(999, 0, 700, QuietTicks));
        Assert.True(MedicAI.Quiet(1000, 1000, 1000, 0));
    }

    [Fact]
    public void Leaving_IsTheExitAndFleeToils()
    {
        Assert.True(MedicAI.Leaving(new LordToil_ExitMap()));
        Assert.True(MedicAI.Leaving(new LordToil_ExitMapFighting()));
        Assert.True(MedicAI.Leaving(new LordToil_ExitMapRandom()));
        Assert.True(MedicAI.Leaving(new LordToil_ExitMapAndDefendSelf()));
        Assert.True(MedicAI.Leaving(new LordToil_PanicFlee()));
        Assert.False(MedicAI.Leaving(new LordToil_AssaultColony()));
        Assert.False(MedicAI.Leaving(null));
    }

    [Fact]
    public void Prefer_NearerThenBleeding()
    {
        Assert.True(MedicAI.Prefer(4, false, 9, true));
        Assert.False(MedicAI.Prefer(9, true, 4, false));
        Assert.True(MedicAI.Prefer(9, true, 9, false));
        Assert.False(MedicAI.Prefer(9, false, 9, true));
        Assert.False(MedicAI.Prefer(9, true, 9, true));
        Assert.True(MedicAI.Prefer(0, false, int.MaxValue, false));
    }
}
