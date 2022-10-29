using System.Linq;
using Verse;

namespace Vampire;

[StaticConstructorOnStartup]
public static class VampireTracker
{
    public enum SunlightPolicy
    {
        Relaxed = 0,
        Restricted = 1,
        NoAI = 2
    }

    public static WorldComponent_VampireTracker Get => Find.World?.GetComponent<WorldComponent_VampireTracker>();

    public static SunlightPolicy GetSunlightPolicy(Pawn pawn)
    {
        if (Get?.SunlightPolicies.ContainsKey(pawn) == true) return Get.SunlightPolicies[pawn];
        return SunlightPolicy.NoAI;
    }

    public static void SetSunlightPolicy(Pawn pawn, SunlightPolicy sunlightPolicy)
    {
        if (Get?.SunlightPolicies?.ContainsKey(pawn) == true)
            Get.SunlightPolicies[pawn] = sunlightPolicy;
        else
            Get.SunlightPolicies.Add(pawn, sunlightPolicy);
    }


    public static bool IsVampire(Pawn pawn)
    {
        if (pawn == null)
            return false;


        if (Find.World?.GetComponent<WorldComponent_VampireTracker>() == null || !Get.vampiresLoaded)
            return pawn.IsVampire(false);

        if (Get?.VampireList?.FirstOrDefault(x => x == pawn) != null)
            return true;
        return false;
    }

    public static void AddVampire(Pawn pawn)
    {
        Get.VampireList.Add(pawn);
        //Log.Message("Added " + pawn.Label);
    }

    public static void RemoveVampire(Pawn pawn)
    {
        if (Get?.VampireList?.Contains(pawn) == true)
            Get.VampireList.Remove(pawn);
    }
}