using Verse;

namespace Vampire.Disciplines.Dominate
{
    public class DisciplineEffect_Mesmerise : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            if (target.Faction == CasterPawn.Faction) //To avoid throwing red errors
                target.ClearMind();
        }
    }
}
