using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Vampire
{
    public class PassiveEffect_DamageSoak : AbilityUser.PassiveEffectWorker
    {
        public virtual int DamageSoak => 1;
    }
}
