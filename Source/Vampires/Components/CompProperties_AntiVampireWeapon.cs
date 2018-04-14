using RimWorld;
using Verse;

namespace Vampire
{
    public class CompProperties_AntiVampireWeapon : CompProperties
    {
        public BodyPartDef partToParalyze = BodyPartDefOf.Heart;
        public float paralysisChance = 1.0f;
        public float paralysisTime = 15.0f;
        public float dmgSoakIgnorePercentage = 0.5f;
    }
}