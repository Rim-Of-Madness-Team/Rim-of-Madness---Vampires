using RimWorld;
using Verse;

namespace Vampire;

public class ThoughtWorker_NeedVampirism : ThoughtWorker_Precept
{
    // Token: 0x06003ED9 RID: 16089 RVA: 0x0015B59C File Offset: 0x0015979C
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        return !p.IsVampire(true);
    }
}