using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampire
{
    public class Thought_Situational_Precept_VampiresInColony : Thought_Situational
	{
		// Token: 0x06003E7E RID: 15998 RVA: 0x0015A61C File Offset: 0x0015881C
		public override float MoodOffset()
		{
			return this.BaseMoodOffset * (float)VampireFactionUtility.GetVampiresInFactionCount(this.pawn.Faction);
		}
	}
}
