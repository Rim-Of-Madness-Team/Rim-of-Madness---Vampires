using System;
using UnityEngine;
using Verse;

namespace Vampire
{
    [StaticConstructorOnStartup]
    internal class Gizmo_HediffShieldStatus : Gizmo
    {
        public HediffComp_Shield shield;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public override float Width
        {
            get
            {
                return 140f;
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.Width, 75f);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, this.shield.labelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = this.shield.Energy / Mathf.Max(1f, this.shield.EnergyMax);
                Widgets.FillableBar(rect3, fillPercent, Gizmo_HediffShieldStatus.FullShieldBarTex, Gizmo_HediffShieldStatus.EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (this.shield.Energy * 100f).ToString("F0") + " / " + (this.shield.EnergyMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
