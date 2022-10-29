using Verse;

namespace Vampire;

public class HediffCompProperties_DamageSoak : HediffCompProperties_Disappears
{
    public int damageToSoak = 1;

    public HediffCompProperties_DamageSoak()
    {
        compClass = typeof(HediffComp_DamageSoak);
    }
}