using Verse;
using UnityEngine;

namespace Vampire
{
    public class HediffComp_TickGraphic : HediffComp
    {
        private bool activated = false;
        public bool Activated => activated;

        private int curGraphicIndex = 0;

        private Graphic curGraphic = null;
        public Graphic CurGraphic { get => curGraphic; set => curGraphic = value; }

        public HediffCompProperties_TickGraphic Props => (HediffCompProperties_TickGraphic)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % Props.cycleRate == 0)
            {
                if ((curGraphicIndex + 1) % Props.cycleGraphics.Count != 0)
                {
                    curGraphicIndex++;
                }
                else
                {
                    curGraphicIndex = 0;
                }
            }
            CurGraphic = Props.cycleGraphics[curGraphicIndex].Graphic;
            if (CurGraphic != null)
            {
                Material material = CurGraphic.MatSingle;
                Vector3 s = new Vector3(CurGraphic.drawSize.x, 1f, CurGraphic.drawSize.y);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(Pawn.DrawPos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref curGraphicIndex, "curGraphicIndex");
            Scribe_Values.Look(ref activated, "activated");
        }
    }
}
