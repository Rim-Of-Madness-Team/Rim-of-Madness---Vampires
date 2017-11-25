using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using AbilityUser;
using UnityEngine;

namespace Vampire
{
    public class VampAbility : PawnAbility
    {
        public CompVampire Vamp => this.Pawn.VampComp(); //VampUtility.GetVamp(this.Pawn);
        public VitaeAbilityDef AbilityDef => Def as VitaeAbilityDef;

        public VampAbility() : base() { }
        public VampAbility(CompAbilityUser abilityUser) : base(abilityUser) { this.abilityUser = abilityUser as CompVampire; }
        public VampAbility(Pawn user, AbilityDef pdef) : base(user, pdef) { }
        public VampAbility(AbilityData data) : base(data) { }

        public override void PostAbilityAttempt()
        {
            //Log.Message("VampAbility :: PostAbilityAttempt Called");
            base.PostAbilityAttempt();
            Pawn.needs.TryGetNeed<Need_Blood>().AdjustBlood(-AbilityDef.bloodCost);
        }

        /// <summary>
        /// Shows the required alignment (optional), 
        /// alignment change (optional),
        /// and the force pool usage
        /// </summary>
        /// <param name="verb"></param>
        /// <returns></returns>
        public override string PostAbilityVerbCompDesc(VerbProperties_Ability verbDef)
        {
            //Log.Message("1");
            string result = "";
            if (verbDef == null) return result;
            if (verbDef?.abilityDef is VitaeAbilityDef vampDef)
            {
                StringBuilder postDesc = new StringBuilder();
                string pointsDesc = "";
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
            if (Find.Selector.NumSelected == 1 && (this?.AbilityDef?.MainVerb?.hasStandardCommand ?? false) && (!this?.Pawn?.Downed ?? false) && (!this?.Pawn?.Dead ?? false))
                return true;
            return false;
        }

        public override bool CanCastPowerCheck(AbilityContext context, out string reason)
        {
            if (base.CanCastPowerCheck(context, out reason))
            {
                reason = "";
                if (this.Def != null && this.Def is VitaeAbilityDef vampDef)
                {
                    if (Pawn.BloodNeed().CurBloodPoints < vampDef.bloodCost)
                    {
                        reason = "ROMV_NotEnoughBloodPoints".Translate(vampDef.bloodCost);
                    }
                }
                return true;
            }
            return false;
        }
    }
}
