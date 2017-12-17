using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Vampire.Buildings
{
    public class Building_Coffin : Building_Sarcophagus
    {
        // RimWorld.Building_Grave
        private Graphic cachedGraphicFull;


        //public override void Draw()

        //public override bool Accepts(Thing thing)


        // RimWorld.Building_Grave
        //public override Graphic Graphic

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            Pawn p = (Pawn)ContainedThing;
            if (p == null)
            {
                p = this?.Corpse?.InnerPawn ?? null;
            }
            if (p != null)
            {
                foreach (Gizmo y in HarmonyPatches.HarmonyPatches.GraveGizmoGetter(p, this))
                    yield return y;
            }
        }


        //public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)


    }
}
