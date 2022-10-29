using Verse;

namespace Vampire;

public class HediffCompProperties_Hidden : HediffCompProperties_Disappears
{
    public bool canGiveDamage = true;
    public bool canMove = true;
    public float damageFactor = 1f;
    public bool immuneTodamage = false;

    public HediffCompProperties_Hidden()
    {
        compClass = typeof(HediffComp_Hidden);
    }
}