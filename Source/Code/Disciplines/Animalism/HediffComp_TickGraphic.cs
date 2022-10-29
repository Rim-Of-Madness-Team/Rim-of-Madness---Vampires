using UnityEngine;
using Verse;

namespace Vampire;

public class HediffComp_TickGraphic : HediffComp
{
    private bool activated;

    private int curGraphicIndex;
    public bool Activated => activated;
    public Graphic CurGraphic { get; set; }

    public HediffCompProperties_TickGraphic Props => (HediffCompProperties_TickGraphic)props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (Find.TickManager.TicksGame % Props.cycleRate == 0)
        {
            if ((curGraphicIndex + 1) % Props.cycleGraphics.Count != 0)
                curGraphicIndex++;
            else
                curGraphicIndex = 0;
        }

        CurGraphic = Props.cycleGraphics[curGraphicIndex].Graphic;
        if (CurGraphic != null)
        {
            var material = CurGraphic.MatSingle;
            var s = new Vector3(CurGraphic.drawSize.x, 1f, CurGraphic.drawSize.y);
            var matrix = default(Matrix4x4);
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