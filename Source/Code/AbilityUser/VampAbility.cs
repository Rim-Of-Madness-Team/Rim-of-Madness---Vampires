using System.Text;
using AbilityUser;
using UnityEngine;
using Verse;

namespace Vampire;

public class VampAbility : PawnAbility
{
    public VampAbility()
    {
    }

    public VampAbility(CompAbilityUser abilityUser) : base(abilityUser)
    {
        this.abilityUser = abilityUser as CompVampire;
    }

    public VampAbility(Pawn user, AbilityDef pdef) : base(user, pdef)
    {
    }

    public VampAbility(AbilityData data) : base(data)
    {
    }

    public CompVampire Vamp => Pawn.VampComp(); //VampUtility.GetVamp(this.Pawn);
    public VitaeAbilityDef AbilityDef => Def as VitaeAbilityDef;

    public override void PostAbilityAttempt()
    {
        base.PostAbilityAttempt();

        //Ghouls lose their CurGhoulVitaePoints
        var bloodNeed = Pawn.needs.TryGetNeed<Need_Blood>();
        if (Pawn.IsGhoul())
            bloodNeed.CurGhoulVitaePoints -= AbilityDef.bloodCost;
        else
            bloodNeed.AdjustBlood(-AbilityDef.bloodCost);

        //Ghouls suffer withdrawal without any vitae in their systems.
        if (Pawn.IsGhoul() && bloodNeed.CurGhoulVitaePoints <= 0)
        {
            var need = Pawn.needs.AllNeeds.FirstOrDefault(x => x.def == VampDefOf.ROMV_Chemical_Vitae);
            if (need != null)
                need.CurLevel = 0f;
        }
    }

    /// <summary>
    ///     Shows the required alignment (optional),
    ///     alignment change (optional),
    ///     and the force pool usage
    /// </summary>
    /// <param name="verb"></param>
    /// <returns></returns>
    public override string PostAbilityVerbCompDesc(VerbProperties_Ability verbDef)
    {
        //Log.Message("1");
        var result = "";
        if (verbDef == null) return result;
        if (verbDef?.abilityDef is VitaeAbilityDef vampDef)
        {
            var postDesc = new StringBuilder();
            var pointsDesc = "";
            pointsDesc = "ROMV_BloodPoints".Translate(new object[]
                {
                    Mathf.Abs(vampDef.bloodCost).ToString()
                })
                ;
            if (pointsDesc != "") postDesc.AppendLine(pointsDesc);
            result = postDesc.ToString();
        }

        return result;
    }

    public override bool ShouldShowGizmo()
    {
        if (Find.Selector.NumSelected == 1 && (this?.AbilityDef?.MainVerb?.hasStandardCommand ?? false) &&
            (!this?.Pawn?.Downed ?? false) && (!this?.Pawn?.Dead ?? false) && PassesAbilitySpecialCases())
            return true;
        return false;
    }

    public override void Notify_AbilityFailed(bool refund)
    {
        base.Notify_AbilityFailed(refund);
        if (refund)
        {
            if (Pawn.IsGhoul())
                Pawn.BloodNeed().CurGhoulVitaePoints += AbilityDef.bloodCost;
            else
                Pawn.BloodNeed().AdjustBlood(AbilityDef.bloodCost);
        }
    }

    public bool PassesAbilitySpecialCases()
    {
        if (AbilityDef == null)
            return false;
        var o = Pawn;
        if (o != null && !o.IsVampire(true) && !o.IsGhoul())
            return false;
        if (AbilityDef == VampDefOf.ROMV_RegenerateLimb)
            return AbilityUser?.AbilityUser?.health?.hediffSet?.hediffs.Any(x => x is Hediff_MissingPart) ?? false;
        if (AbilityDef == VampDefOf.ROMV_VampiricHealing)
            return !(AbilityUser?.AbilityUser?.health?.summaryHealth?.SummaryHealthPercent > 0.99f);
        if (AbilityDef == VampDefOf.ROMV_VampiricHealingScars)
            return AbilityUser?.AbilityUser?.health?.hediffSet?.hediffs.Any(x => x.IsPermanent()) ?? false;
        return true;
    }

    public override bool CanCastPowerCheck(AbilityContext context, out string reason)
    {
        if (base.CanCastPowerCheck(context, out reason))
        {
            reason = "";
            if (Def != null && Def is VitaeAbilityDef vampDef)
            {
                if (Pawn.IsVampire(true))
                {
                    if (Pawn.BloodNeed().CurBloodPoints < vampDef.bloodCost)
                    {
                        reason = "ROMV_NotEnoughBloodPoints".Translate(vampDef.bloodCost);
                        return false;
                    }
                }
                else if (Pawn.IsGhoul())
                {
                    if (Pawn.BloodNeed().CurGhoulVitaePoints < vampDef.bloodCost)
                    {
                        reason = "ROMV_NotEnoughBloodPoints".Translate(vampDef.bloodCost);
                        return false;
                    }
                }
            }

            return true;
        }

        return false;
    }
}