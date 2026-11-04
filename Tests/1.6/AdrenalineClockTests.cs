using Xunit;

namespace MedicWarcasket.Tests;

// The shot's two duration settings over the hediff's clock (AdrenalineClock): the XML's
// severityPerDay and stage thresholds read back as a boost and a crash with the boost's split,
// and any pair of durations written back keeps that split. The stages here are the hediff's
// own: crash 0, wearing off 0.75, steady 0.875, rush 0.9375, -6 a day from 1.
public class AdrenalineClockTests
{
    private const float Top = 1f;
    private const int Precision = 4;
    private static readonly float[] XmlMins = [0f, 0.75f, 0.875f, 0.9375f];
    private const float XmlRate = -6f;

    [Fact]
    public void Read_SplitsTheXmlClock()
    {
        (float boost, float crash, float[] shares) = AdrenalineClock.Read(XmlMins, XmlRate, Top);
        Assert.Equal(1f, boost, Precision);
        Assert.Equal(3f, crash, Precision);
        Assert.Equal(0f, shares[0], Precision);
        Assert.Equal(0.5f, shares[1], Precision);
        Assert.Equal(0.25f, shares[2], Precision);
        Assert.Equal(0.25f, shares[3], Precision);
    }

    [Fact]
    public void XmlDurations_GiveBackTheXmlDefs()
    {
        (float boost, float crash, float[] shares) = AdrenalineClock.Read(XmlMins, XmlRate, Top);
        Assert.Equal(XmlRate, AdrenalineClock.SeverityPerDay(boost, crash, Top), Precision);
        float[] mins = AdrenalineClock.MinSeverities(boost, crash, shares, Top);
        for (int i = 0; i < XmlMins.Length; i++)
            Assert.Equal(XmlMins[i], mins[i], Precision);
    }

    // A two-hour boost and a six-hour crash: eight hours at -3 a day, the crash the bottom
    // three quarters again, the boost's levels half an hour, half an hour and an hour.
    [Fact]
    public void NewDurations_KeepTheBoostsSplit()
    {
        (_, _, float[] shares) = AdrenalineClock.Read(XmlMins, XmlRate, Top);
        Assert.Equal(-3f, AdrenalineClock.SeverityPerDay(2f, 6f, Top), Precision);
        float[] mins = AdrenalineClock.MinSeverities(2f, 6f, shares, Top);
        Assert.Equal(0f, mins[0], Precision);
        Assert.Equal(0.75f, mins[1], Precision);
        Assert.Equal(0.875f, mins[2], Precision);
        Assert.Equal(0.9375f, mins[3], Precision);
    }

    // A one-hour boost and a one-hour crash: the crash the bottom half, the boost's levels a
    // quarter of the top half each but the first, which takes half.
    [Fact]
    public void ShorterCrash_RaisesTheBoostsBand()
    {
        (_, _, float[] shares) = AdrenalineClock.Read(XmlMins, XmlRate, Top);
        Assert.Equal(-12f, AdrenalineClock.SeverityPerDay(1f, 1f, Top), Precision);
        float[] mins = AdrenalineClock.MinSeverities(1f, 1f, shares, Top);
        Assert.Equal(0.5f, mins[1], Precision);
        Assert.Equal(0.75f, mins[2], Precision);
        Assert.Equal(0.875f, mins[3], Precision);
    }
}
