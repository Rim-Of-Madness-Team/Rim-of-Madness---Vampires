using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Vampire
{
	public class ThoughtWorker_Precept_NoVampiresInColony : ThoughtWorker_Precept
	{
		// Token: 0x06003E80 RID: 16000 RVA: 0x0015A63E File Offset: 0x0015883E
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return p.IsColonist && !p.IsSlave && !p.IsPrisoner && VampireFactionUtility.GetVampiresInFactionCount(p.Faction) == 0;
		}
	}
}
