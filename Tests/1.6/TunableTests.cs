using Xunit;

namespace MedicWarcasket.Tests;

// The settings sliders' pure arithmetic (Tunable): where a release lands (Snap, StoredStep), the
// stops of a banded row (Stops) and which stop a stored value sits on (Nearest). Pure functions
// over floats, so the suite needs no game context beyond loading the type.
public class TunableTests
{
    // Stored hours per real second at normal speed (sixty ticks a second, 2500 an hour), the
    // settings' Seconds, so the cooldown bands here are the rows' own.
    private const float HoursPerRealSecond = 60f / 2500f;

    [Fact]
    public void Snap_LandsOnTheStepAndClamps()
    {
        Assert.Equal(22f, Tunable.Snap(22.9f, 0f, 50f, 2f, 1f));
        Assert.Equal(24f, Tunable.Snap(23.1f, 0f, 50f, 2f, 1f));
        Assert.Equal(50f, Tunable.Snap(80f, 0f, 50f, 2f, 1f));
        Assert.Equal(10f, Tunable.Snap(3f, 10f, 300f, 10f, 1f));
    }

    [Fact]
    public void StoredStep_RoundsToWholeShownUnitsWhenScaled()
    {
        Assert.Equal(2f, Tunable.StoredStep(2f, 1f));
        Assert.Equal(3f, Tunable.StoredStep(2f, 3f));
        Assert.Equal(2f, Tunable.StoredStep(2f, 2f));
        Assert.Equal(4f, Tunable.StoredStep(3f, 2f));
    }

    // The casting time's bands: quarter seconds to three, halves to six, whole to ten, the
    // minimum first and each band's end a stop once.
    [Fact]
    public void Stops_FollowTheBands()
    {
        float[] stops = Tunable.Stops(0f, 10f, [(3f, 0.25f), (6f, 0.5f), (10f, 1f)]);
        Assert.Equal(0f, stops[0]);
        Assert.Equal(0.25f, stops[1]);
        Assert.Contains(3f, stops);
        Assert.Contains(3.5f, stops);
        Assert.DoesNotContain(3.25f, stops);
        Assert.Contains(7f, stops);
        Assert.DoesNotContain(6.5f, stops);
        Assert.Equal(10f, stops[^1]);
        Assert.Equal(1 + 12 + 6 + 4, stops.Length);
    }

    // The range's minimum is the touch stop off its band's grid: 1.9, then the whole cells.
    [Fact]
    public void Stops_KeepAnOffGridMinimum()
    {
        float[] stops = Tunable.Stops(1.9f, 12f, [(12f, 1f)]);
        Assert.Equal(1.9f, stops[0]);
        Assert.Equal(2f, stops[1]);
        Assert.Equal(12f, stops[^1]);
        Assert.Equal(12, stops.Length);
    }

    // The cooldowns' bands, real seconds then hours and days on one slider: a band of hours
    // starts off its own grid after the ten-second stop, and float error on the second bands'
    // fractional-hour steps neither drops a band's end nor doubles its start.
    [Fact]
    public void Stops_SpanSecondsToDays()
    {
        float[] stops = Tunable.Stops(0f, 240f,
        [
            (3f * HoursPerRealSecond, 0.25f * HoursPerRealSecond),
            (5f * HoursPerRealSecond, 0.5f * HoursPerRealSecond),
            (10f * HoursPerRealSecond, 1f * HoursPerRealSecond),
            (3f, 0.5f), (12f, 1f), (24f, 6f), (72f, 12f), (240f, 24f),
        ]);
        Assert.Equal(0f, stops[0]);
        Assert.Equal(10f * HoursPerRealSecond, stops[21], 5);
        Assert.Equal(0.5f, stops[22]);
        Assert.Contains(3f, stops);
        Assert.Contains(12f, stops);
        Assert.Contains(24f, stops);
        Assert.Contains(72f, stops);
        Assert.Equal(240f, stops[^1]);
        for (int i = 1; i < stops.Length; i++)
            Assert.True(stops[i] > stops[i - 1], $"stop {i} does not ascend");
        Assert.Equal(1 + 12 + 4 + 5 + 6 + 9 + 2 + 4 + 7, stops.Length);
    }

    [Fact]
    public void Nearest_PicksTheClosestStopLowerOnATie()
    {
        float[] stops = [0f, 1f, 2f, 4f];
        Assert.Equal(0, Tunable.Nearest(stops, -3f));
        Assert.Equal(2, Tunable.Nearest(stops, 2.4f));
        Assert.Equal(2, Tunable.Nearest(stops, 3f));
        Assert.Equal(3, Tunable.Nearest(stops, 9f));
    }
}
