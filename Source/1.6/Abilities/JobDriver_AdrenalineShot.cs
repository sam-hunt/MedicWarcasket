namespace MedicWarcasket;

// The adrenaline shot's job, declared by MDWC_AdrenalineShot's driverClass
// (JobDefs/AdrenalineShot.xml): the shared walk, cooldown wait, check, warmup and cast
// (JobDriver_MedicCast) and nothing after, since VEF's cast toil applies the shot
// (Ability_AdrenalineShot.Cast).
public class JobDriver_AdrenalineShot : JobDriver_MedicCast
{
}
