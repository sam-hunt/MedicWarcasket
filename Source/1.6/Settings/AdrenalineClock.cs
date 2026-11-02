namespace MedicWarcasket;

// The adrenaline hediff's clock as two durations, the boost and the crash, over the def fields
// that hold it: the severity-per-day comp's severityPerDay and the stages' minSeverity. Stages
// are vanilla's ascending list, the crash first at minSeverity 0 and the boost's levels above
// it, the top one running up to the shot's initial severity (top). Severity falls at a
// constant rate, so a stage's band over the whole span is its share of the clock: the crash's
// band is the crash's share of the total, and each boost level's band within the boost is its
// share of the boost. Read takes both durations and those shares off the XML; Apply writes a
// pair of durations back with the shares held, so the boost's levels keep the def's split at
// any length. Kept apart from the settings class, whose VEF-typed fields the headless tests
// cannot load.
public static class AdrenalineClock
{
    // The XML's boost and crash in game hours and each stage's share of the boost by stage
    // index (the crash, index 0, has none). A comp that does not fade reads as no clock.
    public static (float boostHours, float crashHours, float[] boostShares) Read(float[] minSeverities, float severityPerDay, float top)
    {
        int count = minSeverities.Length;
        float[] shares = new float[count];
        float boostBand = top - minSeverities[1];
        for (int i = 1; i < count; i++)
        {
            float upper = i == count - 1 ? top : minSeverities[i + 1];
            shares[i] = boostBand > 0f ? (upper - minSeverities[i]) / boostBand : 0f;
        }
        float totalHours = severityPerDay < 0f ? -24f * top / severityPerDay : 0f;
        float crashHours = totalHours * minSeverities[1] / top;
        return (totalHours - crashHours, crashHours, shares);
    }

    // The severityPerDay that runs the whole clock down from top in boost plus crash hours.
    public static float SeverityPerDay(float boostHours, float crashHours, float top) =>
        -24f * top / (boostHours + crashHours);

    // Each stage's minSeverity for a boost and a crash: the crash at 0, the first boost level
    // at the crash's share of top, each one above it a further share of the boost's band.
    public static float[] MinSeverities(float boostHours, float crashHours, float[] boostShares, float top)
    {
        float totalHours = boostHours + crashHours;
        float[] mins = new float[boostShares.Length];
        float floor = top * crashHours / totalHours;
        float boostBand = top - floor;
        for (int i = 1; i < mins.Length; i++)
        {
            mins[i] = floor;
            floor += boostShares[i] * boostBand;
        }
        return mins;
    }
}
