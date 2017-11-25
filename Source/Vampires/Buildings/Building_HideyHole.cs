using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class Building_HideyHole : Building_Grave
    {
        public override void Open()
        {
            base.Open();
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
